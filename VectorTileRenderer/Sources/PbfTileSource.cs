﻿using AliFlex.VectorTileRenderer.Drawing;
using Mapbox.VectorTile.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;


namespace AliFlex.VectorTileRenderer.Sources
{
    public class PbfTileSource : IVectorTileSource
    {
        public string Path { get; set; } = "";
        public Stream Stream { get; set; } = null;

        public PbfTileSource(string path)
        {
            this.Path = path;
        }

        public PbfTileSource(Stream stream)
        {
            this.Stream = stream;
        }

        public async Task<Stream> GetTile(int x, int y, int zoom)
        {
            return await Task.Run(() =>
            {
                var qualifiedPath = Path
                    .Replace("{x}", x.ToString())
                    .Replace("{y}", y.ToString())
                    .Replace("{z}", zoom.ToString());
                return File.Open(qualifiedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            });
        }

        public async Task<VectorTile> GetVectorTile(int x, int y, int zoom)
        {
            if (Path != "")
            {
                using (var stream = await GetTile(x, y, zoom))
                {
                    return await UnzipStream(stream);
                }
            }
            else if (Stream != null)
            {
                return await UnzipStream(Stream);
            }

            return null;
        }

        async Task<VectorTile> UnzipStream(Stream stream)
        {
            if (IsGZipped(stream))
            {
                using (var zipStream = new GZipStream(stream, CompressionMode.Decompress))
                using (var resultStream = new MemoryStream())
                {
                    zipStream.CopyTo(resultStream);
                    resultStream.Seek(0, SeekOrigin.Begin);
                    return await LoadStream(resultStream);
                }
            }
            else
            {
                return await LoadStream(stream);
            }
        }

        async Task<VectorTile> LoadStream(Stream stream)
        {
            var mbLayers = new Mapbox.VectorTile.VectorTile(ReadTillEnd(stream));

            return await BaseTileToVector(mbLayers);
        }

        static string ConvertGeometryType(GeomType type)
        {
            if (type == GeomType.LINESTRING)
            {
                return "LineString";
            }
            else if (type == GeomType.POINT)
            {
                return "Point";
            }
            else if (type == GeomType.POLYGON)
            {
                return "Polygon";
            }
            else
            {
                return "Unknown";
            }
        }

        static async Task<VectorTile> BaseTileToVector(object baseTile)
        {
            return await Task.Run(() =>
            {
                var tile = baseTile as Mapbox.VectorTile.VectorTile;
                var result = new VectorTile();

                foreach (var lyrName in tile.LayerNames())
                {
                    Mapbox.VectorTile.VectorTileLayer lyr = tile.GetLayer(lyrName);

                    var vectorLayer = new VectorTileLayer
                    {
                        Name = lyrName
                    };

                    for (int i = 0; i < lyr.FeatureCount(); i++)
                    {
                        Mapbox.VectorTile.VectorTileFeature feat = lyr.GetFeature(i);

                        var vectorFeature = new VectorTileFeature
                        {
                            Extent = 1,
                            GeometryType = ConvertGeometryType(feat.GeometryType),
                            Attributes = feat.GetProperties()
                        };

                        var vectorGeometry = new List<List<Point>>();

                        foreach (var points in feat.Geometry<int>())
                        {
                        var vectorPoints = new List<Point>();

                            foreach (var coordinate in points)
                            {
                                var dX = (double)coordinate.X / (double)lyr.Extent;
                                var dY = (double)coordinate.Y / (double)lyr.Extent;

                                vectorPoints.Add(new Point(dX, dY));

                                //var newX = Utils.ConvertRange(dX, extent.Left, extent.Right, 0, vectorFeature.Extent);
                                //var newY = Utils.ConvertRange(dY, extent.Top, extent.Bottom, 0, vectorFeature.Extent);

                                //vectorPoints.Add(new Point(newX, newY));
                            }

                            vectorGeometry.Add(vectorPoints);
                        }

                        vectorFeature.Geometry = vectorGeometry;
                        vectorLayer.Features.Add(vectorFeature);
                    }

                    result.Layers.Add(vectorLayer);
                }

                return result;
            });
        }

        byte[] ReadTillEnd(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        bool IsGZipped(Stream stream)
        {
            return IsZipped(stream, 3, "1F-8B-08");
        }

        bool IsZipped(Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;
            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, SeekOrigin.Begin);
            string actualSignature = BitConverter.ToString(signature);
            if (actualSignature == expectedSignature) return true;
            return false;
        }
    }
}

﻿using AliFlex.VectorTileRenderer.Enums;
using BruTile;
using BruTile.Predefined;


namespace TileTest
{
    class VectorMbTilesSource : BruTile.ITileSource
    {
        readonly VectorMbTilesProvider provider;

        public ITileSchema Schema { get; }
        public string Name { get; } = "VectorMbTileSource";
        public Attribution Attribution { get; } = new Attribution();

        public VectorMbTilesSource(string path, string cachePath, VectorStyleKind style = VectorStyleKind.Basic)
        {
            Schema = GetTileSchema();
            provider = new VectorMbTilesProvider(path, cachePath, style);
        }

        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            //schema.Resolutions.Clear();
            //schema.Resolutions["0"] = new Resolution("0", 156543.033900000);
            //schema.Resolutions["1"] = new Resolution("1", 78271.516950000);
            return schema;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return provider.GetTile(tileInfo);
        }

        public ITileProvider Provider
        {
            get { return provider; }
        }

    }
}

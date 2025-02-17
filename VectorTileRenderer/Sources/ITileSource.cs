﻿using System.IO;
using System.Threading.Tasks;

namespace AliFlex.VectorTileRenderer.Sources
{
    public interface ITileSource
    {
        Task<Stream> GetTile(int x, int y, int zoom);
    }
}

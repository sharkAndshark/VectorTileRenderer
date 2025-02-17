﻿using System;

namespace AliFlex.VectorTileRenderer
{
    static class DoubleExtension
    {
        private const double DefaultPrecision = 0.0001;

        internal static bool BasicallyEqualTo(this double a, double b)
        {
            return a.BasicallyEqualTo(b, 0.0001);
        }

        internal static bool BasicallyEqualTo(this double a, double b, double precision)
        {
            return Math.Abs(a - b) <= precision;
        }
    }
}

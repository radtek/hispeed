﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GeoDo.RSS.RasterTools
{
    internal class ContourGeneratorInt16 : ContourGenerator<Int16>
    {
        protected override Int16 GetNoDataValueOutsideAOI()
        {
            return (Int16)_noData;
        }

        protected override bool CheckIsContained(Int16 v1, Int16 v2, Int16 val)
        {
            if (v1 == v2)
                return false;
            if (v1 == val)
                v1 -= 1;
            if (v2 == val)
                v2 -= 1;
            return (v1 - val) * (v2 - val) < 0;
        }

        protected override Int16 ToTValue(double v)
        {
            return (Int16)v;
        }

        /*
         * var factor = (float)((value - edge.P1.Value) / (edge.P2.Value - edge.P1.Value));
           result.X = edge.P1.X + factor * (edge.P2.X - edge.P1.X);
           result.Y = edge.P1.Y + factor * (edge.P2.Y - edge.P1.Y);
         */
        protected override PointF GetVPoint(enumEdgeType edgeType, int pixelIdxV1, int pixelIdxV2, double contValue)
        {
            float factor = (float)(contValue - _bandValues[pixelIdxV1]) / (_bandValues[pixelIdxV2] - _bandValues[pixelIdxV1]);
            int y = pixelIdxV1 / _width;//row
            int x = pixelIdxV1 % _width;//col
            switch (edgeType)
            {
                case enumEdgeType.A:
                    return new PointF(x + factor, y);
                case enumEdgeType.B:
                    return new PointF(x, y + factor);
                case enumEdgeType.C:
                    return new PointF(x + factor, y + factor);
            }
            return PointF.Empty;
        }
    }
}

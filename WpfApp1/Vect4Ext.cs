﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public static class Vect4Ext
    {
        public static void FindMaxes(this ref Vector4 max, Vector4 vect)
        {
            max.X = Math.Max(max.X, vect.X);
            max.Y = Math.Max(max.Y, vect.Y);
            max.Z = Math.Max(max.Z, vect.Z);
        }

        public static void FindMin(this ref Vector4 min, Vector4 vect)
        {
            min.X = Math.Min(min.X, vect.X);
            min.Y = Math.Min(min.Y, vect.Y);
            min.Z = Math.Min(min.Z, vect.Z);
        }
    }
}
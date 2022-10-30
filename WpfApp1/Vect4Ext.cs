using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public static void MinByY(ref Vector4 min, ref Vector4 vect) {
            if (min.Y > vect.Y) { 
                (min,vect)=(vect,min);
            }
        }

        public static void MinByYWitnNorm(ref (Vector4 v,Vector3 n) min,ref (Vector4 v, Vector3 n) v)
        {
            if (min.v.Y > v.v.Y)
            {
                (min, v) = (v, min);
            }
        }
        public static void MinByYWitNE(ref (Vector4 v, Vector3 n,Vector3 e) min, ref (Vector4 v, Vector3 n, Vector3 e) v)
        {
            if (min.v.Y > v.v.Y)
            {
                (min, v) = (v, min);
            }
        }

        public static (float k, float b) GetEquation(this ref Vector4 xy1, Vector4 xy2) {
            var k = (xy1.Y - xy2.Y) / (xy1.X - xy2.X);
            return (k, xy1.Y - xy1.X * k);
        }

        public static Vector3 GetVect3(this ref Vector4 v) {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}

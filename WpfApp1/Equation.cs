using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Equation
    {
        float kx, kz;
        Vector4 x1y1z1;

        public Equation(Vector4 xy1, Vector4 xy2)
        {
            x1y1z1 = xy1;

            var sub = xy1 - xy2;
            kx = (sub.X) / sub.Y;
            kz = (sub.Z) / sub.Y;
        }

        public Vector4 GetCoords(float y) {
            
            var x = kx * (y - x1y1z1.Y) + x1y1z1.X;
            var z = kz * (y - x1y1z1.Y) + x1y1z1.Z;
            return new Vector4(x, y, z,1);
        }
    }
}

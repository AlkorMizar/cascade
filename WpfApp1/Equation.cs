using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Equation
    {
        float kx, kz;
        Vector3 kvn,kve, n1,e1;
        Vector4 x1y1z1;
        Polygon.AllDotInfo vstart, vdelta, vend;

        public Equation(Vector4 xy1, Vector4 xy2)
        {
            x1y1z1 = xy1;

            var sub = xy1 - xy2;
            kx = (sub.X) / sub.Y;
            kz = (sub.Z) / sub.Y;
        }

        public Equation(Vector4 xy1, Vector4 xy2, Vector3 n1, Vector3 n2)
        {
            x1y1z1 = xy1;
            this.n1 = n1;

            var sub = xy1 - xy2;
            kx = (sub.X) / sub.Y;
            kz = (sub.Z) / sub.Y;
            kvn = (n1 - n2) / sub.Y;
        }

        public Equation(Vector4 xy1, Vector4 xy2, Vector3 n1, Vector3 n2, Vector3 e1, Vector3 e2)
        {
            x1y1z1 = xy1;
            this.n1 = n1;
            this.e1 = e1;

            var sub = xy1 - xy2;
            kx = (sub.X) / sub.Y;
            kz = (sub.Z) / sub.Y;
            kvn = (n1 - n2) / sub.Y;
            kve = (e1 - e2) / sub.Y;
        }

        public Equation(Polygon.AllDotInfo v1, Polygon.AllDotInfo v2)
        {
            vstart = v1;
            vend = v2;
            vdelta = vstart.delta(v2);
        }

        public Polygon.AllDotInfo GetAll(float y)
        {
            return vdelta * (y - vstart.v.Y) + vstart;
        }

        public Vector4 GetCoords(float y)
        {

            var x = kx * (y - x1y1z1.Y) + x1y1z1.X;
            var z = kz * (y - x1y1z1.Y) + x1y1z1.Z;
            return new Vector4(x, y, z, 1);
        }

        public (Vector4, Vector3) GetCoordsWithNormal(float y)
        {

            var x = kx * (y - x1y1z1.Y) + x1y1z1.X;
            var z = kz * (y - x1y1z1.Y) + x1y1z1.Z;

            var n = kvn * (y - x1y1z1.Y) + n1;
            return (new Vector4(x, y, z, 1), n);
        }
        public (Vector4, Vector3, Vector3) GetCoordsWithNormalandE(float y)
        {

            var x = kx * (y - x1y1z1.Y) + x1y1z1.X;
            var z = kz * (y - x1y1z1.Y) + x1y1z1.Z;

            var n = kvn * (y - x1y1z1.Y) + n1;
            var e = kve * (y - x1y1z1.Y) + e1;
            return (new Vector4(x, y, z, 1), n,e);
        }

        public Polygon.AllDotInfo GetByAllDot(float y)
        {
            var t = (y - vstart.v.Y)/(vend.v.Y-vstart.v.Y);
            var tex = vstart.t * (1 - t)/vstart.v.W + vend.t * t/vend.v.W;
            var z = (1 - t) / vstart.v.W + t / vend.v.W;
            var res = vdelta * (y - vstart.v.Y) + vstart;
            res.t = tex/z;
            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace WpfApp1
{
    struct ModelFrame
    {
        Vector4[] vertexes;
        float[] w;
        Vector3[] textures, normals;
        Vector3[,] TextMap, NormMap, MirrMap;
        List<Polygon> polygons;



        Vector4 minVect, maxVect;
        public Vector4 MinVect { get => minVect; }
        public Vector4 MaxVect { get => maxVect; }
        public Vector4 Middle
        {
            get
            {
                return (MaxVect + MinVect) / 2;
            }
        }
        public Vector4 Eye { get; private set; }



        public Vector4 this[int ind] { get { return vertexes[ind]; } }

        public Vector4[] Vertexes { get { return vertexes; } }
        public Vector3[] Normals { get { return normals; } }
        public Vector3[] Textures { get { return textures; } }

        public Vector3 GetTexture(double dx, double dy)
        {
            int x = (int)Math.Floor(dx * (TextMap.GetLength(1) - 1));
            int y = (int)Math.Floor((1 - dy) * (TextMap.GetLength(0) - 1));
            return TextMap[y, x];
        }

        public Vector3 GetNormals(double dx, double dy)
        {
            int x = (int)Math.Floor(dx * (NormMap.GetLength(1) - 1));
            int y = (int)Math.Floor((1 - dy) * (NormMap.GetLength(0) - 1));
            return (NormMap[y, x] / 255);
        }

        public Vector3 GetMirror(double dx, double dy)
        {
            int x = (int)Math.Floor(dx * (MirrMap.GetLength(1) - 1));
            int y = (int)Math.Floor((1 - dy) * (MirrMap.GetLength(0) - 1));
            return MirrMap[y, x] / 255;
        }

        public ModelFrame(int vs, int vns, int vts, List<Polygon> _polygons, Vector4 _minVect, Vector4 _maxVect, Vector3[,] t, Vector3[,] m, Vector3[,] n)
        {
            vertexes = new Vector4[vs];
            w = new float[vs];
            textures = new Vector3[vts];
            normals = new Vector3[vns];

            polygons = _polygons;
            minVect = _minVect;
            maxVect = _maxVect;

            Eye = new Vector4();

            TextMap = t;
            MirrMap = m;
            NormMap = n;
        }

        public void TranslateTo(System.Numerics.Matrix4x4 transfMatrix)
        {
            maxVect.X = maxVect.Y = maxVect.Z = float.MinValue;
            minVect.X = minVect.Y = minVect.Z = float.MaxValue;
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] = Vector4.Transform(vertexes[i], transfMatrix);
                maxVect.FindMaxes(vertexes[i]);
                minVect.FindMin(vertexes[i]);
            }
        }

        public void remember()
        {
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i].W = w[i];

            }
        }

        public void DivideByW()
        {
            maxVect.X = maxVect.Y = maxVect.Z = 0;
            minVect.X = minVect.Y = minVect.Z = float.MaxValue;
            for (int i = 0; i < vertexes.Length; i++)
            {
                w[i] = vertexes[i].W;
                vertexes[i] /= vertexes[i].W;
                maxVect.FindMaxes(vertexes[i]);
                minVect.FindMin(vertexes[i]);
            }
        }

        public async Task ResetVertexes(List<Vector4> vrts, List<Vector3> ns, List<Vector3> txts)
        {
            var t1 = CopyData(vertexes, vrts);
            var t2 = CopyData(normals, ns);
            var t3 = CopyData(textures, txts);
            Task.WaitAll(t1, t2, t3);
        }

        public async Task CopyData<T>(T[] to, List<T> from)
        {
            for (int i = 0; i < from.Count; i++)
            {
                to[i] = from[i];
            }
        }

        public IEnumerator<Polygon> GetEnumerator()
        {
            foreach (var polygon in polygons)
            {
                yield return polygon;
            }
        }
    }
}

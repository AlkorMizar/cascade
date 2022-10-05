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
        System.Numerics.Vector4[] vertexes;
        public float[] zCoords;
        List<Polygon> polygons;

        Vector4 minVect, maxVect;
        public Vector4 MinVect { get => minVect; }
        public Vector4 MaxVect { get => maxVect; }
        public Vector4 Middle { get {
                return (MaxVect + MinVect)/2;
            }
        }
        public Vector4 Light { get; private set; }



        public Vector4 this[int ind] { get { return vertexes[ind]; } }

        IReadOnlyCollection<System.Numerics.Vector4> Vertexes { get { return vertexes; } }

        public ModelFrame(int size,List<Polygon> _polygons,Vector4 _minVect,Vector4 _maxVect) { 
            vertexes = new Vector4[size];
            polygons = _polygons;
            minVect = _minVect;
            maxVect = _maxVect;
            zCoords=new float[size];
            Light = new Vector4();
        }

        public void TranslateTo(System.Numerics.Matrix4x4 transfMatrix) 
        {
            maxVect.X = maxVect.Y = maxVect.Z = float.MinValue;
            minVect.X = minVect.Y = minVect.Z = float.MaxValue;
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] = Vector4.Transform(vertexes[i],transfMatrix);
                maxVect.FindMaxes(vertexes[i]);
                minVect.FindMin(vertexes[i]);
            }
        }

        public void DivideByW()
        {
            maxVect.X = maxVect.Y = maxVect.Z = 0;
            minVect.X = minVect.Y = minVect.Z = float.MaxValue;
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] /= vertexes[i].W;
                maxVect.FindMaxes(vertexes[i]);
                minVect.FindMin(vertexes[i]);
            }
        }

        public void ResetVertexes(List<System.Numerics.Vector4> vrts,Vector4 light)
        {
            Light=light;
            for (int i = 0; i < vrts.Count; i++)
            {
                vertexes[i]=vrts[i];
                zCoords[i] = 0;
            }
        }

        public void SetZCoord() {
            for (int i = 0; i < vertexes.Length; i++)
            {
                zCoords[i] = vertexes[i].Z;
            }
        }

        /*public IEnumerator<(System.Numerics.Vector4 x1y1, System.Numerics.Vector4 x2y2)> GetLines()
        {
            foreach (var polygon in polygons)
            {
                foreach (var line in polygon)
                {
                    yield return (vertexes[line.f-1], vertexes[line.s-1]);
                }
            }
        }*/

        public (System.Numerics.Vector4 x1y1, System.Numerics.Vector4 x2y2) GetLine((int f, int s) ids) { 
            return (vertexes[ids.f - 1], vertexes[ids.s - 1]);
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

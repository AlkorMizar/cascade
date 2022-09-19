using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    struct ModelFrame
    {
        System.Numerics.Vector4[] vertexes;
        List<Polygon> polygons;

        Vector4 minVect, maxVect;
        public Vector4 MinVect { get => minVect; }
        public Vector4 MaxVect { get => maxVect; }
        public Vector4 Middle { get {
                return (MaxVect + MinVect)/2;
            }
        }

        IReadOnlyCollection<System.Numerics.Vector4> Vertexes { get { return vertexes; } }

        public ModelFrame(int size,List<Polygon> _polygons,Vector4 _minVect,Vector4 _maxVect) { 
            vertexes = new Vector4[size];
            polygons = _polygons;
            minVect = _minVect;
            maxVect = _maxVect;
        }

        public void TranslateTo(System.Numerics.Matrix4x4 transfMatrix) 
        {
            maxVect.X = maxVect.Y = maxVect.Z = 0;
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

        public void ResetVertexes(List<System.Numerics.Vector4> vrts)
        {
            for (int i = 0; i < vrts.Count; i++)
            {
                vertexes[i]=vrts[i];
            }
        }

        public IEnumerator<(System.Numerics.Vector4 x1y1, System.Numerics.Vector4 x2y2)> GetEnumerator()
        {
            foreach (var polygon in polygons)
            {
                foreach (var line in polygon)
                {
                    yield return (vertexes[line.f-1], vertexes[line.s-1]);
                }
            }
        }
    }
}

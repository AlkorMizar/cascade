using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace WpfApp1
{
    class Obj
    {
        List<Vector4> vertexes;
        List<Vector3> textures;
        List<Vector3> normals;
        List<Vector3> polygNorm;
        List<Polygon> polygons;
        ModelFrame frame;

        Vector4 minVect, maxVect;
        public Vector4 MinVect { get => frame.MaxVect; }//deprecated
        public Vector4 MaxVect { get => frame.MaxVect; }//deprecated
        public Vector4 Middle { get => frame.Middle; }//deprecated

        public Vector4 this[int ind] { get { return vertexes[ind]; } }


        public Obj()
        {
            vertexes = new List<Vector4>();
            textures = new List<Vector3>();
            normals = new List<Vector3>();
            polygons = new List<Polygon>();
            minVect = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, 0);
        }

        public void AddVertex(float x, float y, float z, float? w)
        {
            vertexes.Add(new Vector4(x, y, z, w.HasValue ? w.Value : 1));
            maxVect.FindMaxes(vertexes.Last());
            minVect.FindMin(vertexes.Last());
        }

        public void AddTexture(float u, float? v, float? w)
        {
            textures.Add(new Vector3(u, v.HasValue ? v.Value : 0, w.HasValue ? w.Value : 0));
        }

        public void AddNormal(float i, float j, float k)
        {
            normals.Add(new Vector3(i, j, k));
        }

        public void AddPolygon(List<(int, int?, int?)> vert)
        {
            polygons.Add(new Polygon(vert, this));
        }

        public void Finish()
        {
            frame = new ModelFrame(vertexes.Count, polygons, minVect, maxVect);
        }

        public ModelFrame NewFrame(Vector4 light)
        {
            frame.ResetVertexes(vertexes,light);
            return frame;
        }
    }

    struct Polygon
    {
        public List<int> vertexes;
        public List<int> textures;
        public List<int> normals;
        int vertL;
        public Vector3 normal { get; private set; }

        public Polygon(List<(int, int?, int?)> vertTextNorm, Obj obj)
        {
            vertexes = new List<int>(vertTextNorm.Count);
            textures = new List<int>(vertTextNorm.Count);
            normals = new List<int>(vertTextNorm.Count);

            foreach (var vert in vertTextNorm)
            {
                vertexes.Add(vert.Item1);
                textures.Add(vert.Item2.GetValueOrDefault());
                normals.Add(vert.Item3.GetValueOrDefault());
            }
            vertL = vertexes.Count;

            var (v4_12, v4_13) = (obj[vertexes[0] - 1] - obj[vertexes[1] - 1], obj[vertexes[0] - 1] - obj[vertexes[2] - 1]);

            var v12 = new Vector3(v4_12.X, v4_12.Y, v4_12.Z);
            var v13 = new Vector3(v4_13.X, v4_13.Y, v4_13.Z);
            normal = Vector3.Normalize(Vector3.Cross(v12, v13));
        }

        public IEnumerator<(int f, int s)> GetEnumerator()
        {
            for (int i = 0; i < vertL; i++)
            {
                yield return (vertexes[i], vertexes[(i + 1) % vertL]);
            }
        }

        public (Vector4 xy1, Vector4 xy2, Vector4) GetPolygonCoordsByY(ModelFrame fr)
        {
            var (xy1, xy2, xy3) = (fr[vertexes[0] - 1], fr[vertexes[1] - 1], fr[vertexes[2] - 1]);
            Vect4Ext.MinByY(ref xy1, ref xy2);
            Vect4Ext.MinByY(ref xy1, ref xy3);
            Vect4Ext.MinByY(ref xy2, ref xy3);
            return (xy1, xy2, xy3);
        }

        public Vector3 GetNormal(ModelFrame fr)
        {
            var (v4_12, v4_13) = (fr[vertexes[0] - 1] - fr[vertexes[1] - 1], fr[vertexes[0] - 1] - fr[vertexes[2] - 1]);
            var v12 = new Vector3(v4_12.X, v4_12.Y, v4_12.W);
            var v13 = new Vector3(v4_13.X, v4_13.Y, v4_13.W);
            return Vector3.Normalize(Vector3.Cross(v12, v13));
        }
    }
}

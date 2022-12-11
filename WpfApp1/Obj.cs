using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace WpfApp1
{
    class Obj
    {
        public List<Vector4> vertexes;
        public List<Vector3> textures;
        public List<Vector3> Normals { get; set; }
        List<Polygon> polygons;
        ModelFrame frame;

        Vector3[,] TextMap, NormMap, MirrMap;

        Vector4 minVect, maxVect;
        public Vector4 MinVect { get => frame.MaxVect; }//deprecated
        public Vector4 MaxVect { get => frame.MaxVect; }//deprecated
        public Vector4 Middle { get => frame.Middle; }//deprecated

        public Vector4 this[int ind] { get { return vertexes[ind]; } }


        public Obj()
        {
            vertexes = new List<Vector4>();
            textures = new List<Vector3>();
            Normals = new List<Vector3>();
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
            Normals.Add(new Vector3(i, j, k));
        }

        public void AddPolygon(List<(int, int?, int?)> vert)
        {
            polygons.Add(new Polygon(vert, this));
        }

        public void SetMaps(Vector3[,] t, Vector3[,] m, Vector3[,] n)
        {
            TextMap = t;
            NormMap = n;
            MirrMap = m;
        }

        public void Finish()
        {
            frame = new ModelFrame(vertexes.Count, Normals.Count, textures.Count, polygons, minVect, maxVect, TextMap, MirrMap, NormMap);
        }

        public ModelFrame NewFrame()
        {
            frame.ResetVertexes(vertexes, Normals, textures);
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
        private Obj obj;

        public struct AllDotInfo
        {
            public Vector4 v;
            public Vector3 t;
            public Vector3 e;

            public AllDotInfo(Vector4 v, Vector3 t, Vector3 e)
            {
                this.v = v;
                this.t = t;
                this.e = e;
            }

            public static void Min(ref AllDotInfo min, ref AllDotInfo v)
            {
                if (min.v.Y > v.v.Y)
                {
                    (min, v) = (v, min);
                }
            }

            public AllDotInfo delta(AllDotInfo v2)
            {
                AllDotInfo res = this;

                var sub = res.v - v2.v;
                res.v = sub / sub.Y;

                res.e = (res.e - v2.e) / sub.Y;
                res.t = (res.t - v2.t) / sub.Y;
                return res;
            }

            public static AllDotInfo operator *(AllDotInfo v1, AllDotInfo v2)
            {
                v1.v *= v2.v;
                v1.e *= v2.e;
                v1.t *= v2.t;
                return v1;
            }
            public static AllDotInfo operator *(AllDotInfo v1, float k)
            {
                v1.v *= k;
                v1.e *= k;
                v1.t *= k;
                return v1;
            }

            public static AllDotInfo operator /(AllDotInfo v1, float k)
            {
                v1.v /= k;
                v1.e /= k;
                v1.t /= k;
                return v1;
            }
            public static AllDotInfo operator +(AllDotInfo v1, AllDotInfo v2)
            {
                v1.v += v2.v;
                v1.e += v2.e;
                v1.t += v2.t;
                return v1;
            }

            public static AllDotInfo operator -(AllDotInfo v1, AllDotInfo v2)
            {
                v1.v -= v2.v;
                v1.e -= v2.e;
                v1.t -= v2.t;
                return v1;
            }
        }

        public Polygon(List<(int, int?, int?)> vertTextNorm, Obj _obj)
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

            obj = _obj;

            normal = (obj.Normals[normals[0] - 1] + obj.Normals[normals[1] - 1] + obj.Normals[normals[2] - 1]) / 3.0f;
        }

        public IEnumerator<(int f, int s)> GetEnumerator()
        {
            for (int i = 0; i < vertL; i++)
            {
                yield return (vertexes[i], vertexes[(i + 1) % vertL]);
            }
        }

        public (Vector4 xy1, Vector4 xy2, Vector4 xy3) GetPolygonCoordsByY(ModelFrame fr)
        {
            var (xy1, xy2, xy3) = (fr[vertexes[0] - 1], fr[vertexes[1] - 1], fr[vertexes[2] - 1]);
            Vect4Ext.MinByY(ref xy1, ref xy2);
            Vect4Ext.MinByY(ref xy1, ref xy3);
            Vect4Ext.MinByY(ref xy2, ref xy3);
            return (xy1, xy2, xy3);
        }
        public (Vector4 xy1, Vector3 n1, Vector3 e1, Vector4 xy2, Vector3 n2, Vector3 e2, Vector4 xy3, Vector3 n3, Vector3 e3) GetPolygonCoordsWithNormByY(ModelFrame fr, Vector4 Eye)
        {
            var (xy1, xy2, xy3) = (fr[vertexes[0] - 1], fr[vertexes[1] - 1], fr[vertexes[2] - 1]);
            var (n1, n2, n3) = GetNormals();
            var (e1, e2, e3) = GetEyeVects(Eye);
            (Vector4 v, Vector3 n, Vector3 e) v1 = (xy1, n1, e1), v2 = (xy2, n2, e2), v3 = (xy3, n3, e3);
            Vect4Ext.MinByYWitNE(ref v1, ref v2);
            Vect4Ext.MinByYWitNE(ref v1, ref v3);
            Vect4Ext.MinByYWitNE(ref v2, ref v3);
            return (v1.v, v1.n, v1.e, v2.v, v2.n, v2.e, v3.v, v3.n, v3.e);
        }

        public (AllDotInfo v1, AllDotInfo v2, AllDotInfo v3) GetPolygonCoordsWithNormANDTextByY(ModelFrame fr, Vector4 Eye)
        {
            var (xy1, xy2, xy3) = (fr.Vertexes[vertexes[0] - 1], fr.Vertexes[vertexes[1] - 1], fr.Vertexes[vertexes[2] - 1]);
            var (t1, t2, t3) = (fr.Textures[textures[0] - 1], fr.Textures[textures[1] - 1], fr.Textures[textures[2] - 1]);
            var (e1, e2, e3) = GetEyeVects(Eye);

            var iv1 = new AllDotInfo(xy1, t1, e1);
            var iv2 = new AllDotInfo(xy2, t2, e2);
            var iv3 = new AllDotInfo(xy3, t3, e3);

            AllDotInfo.Min(ref iv1, ref iv2);
            AllDotInfo.Min(ref iv1, ref iv3);
            AllDotInfo.Min(ref iv2, ref iv3);

            return (iv1, iv2, iv3);
        }

        public float GetNormal(ModelFrame fr)
        {
            var (v4_12, v4_13) = (fr[vertexes[0] - 1] - fr[vertexes[1] - 1], fr[vertexes[0] - 1] - fr[vertexes[2] - 1]);
            var v12 = new Vector3(v4_12.X, v4_12.Y, v4_12.Z);
            var v13 = new Vector3(v4_13.X, v4_13.Y, v4_13.Z);
            return Vector3.Dot(v12, v13);
        }

        public (Vector3 n1, Vector3 n2, Vector3 n3) GetNormals()
        {
            return (obj.Normals[normals[0] - 1], obj.Normals[normals[1] - 1], obj.Normals[normals[2] - 1]);
        }
        public (Vector3 n1, Vector3 n2, Vector3 n3) GetEyeVects(Vector4 Eye)
        {
            var v1 = -Vector4.Normalize(Eye - obj.vertexes[vertexes[0] - 1]);
            var v2 = -Vector4.Normalize(Eye - obj.vertexes[vertexes[1] - 1]);
            var v3 = -Vector4.Normalize(Eye - obj.vertexes[vertexes[2] - 1]);
            return (v1.GetVect3(), v2.GetVect3(), v3.GetVect3());
        }
    }
}
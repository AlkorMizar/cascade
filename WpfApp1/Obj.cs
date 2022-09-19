using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace WpfApp1
{
    class Obj
    {
        List<Vector4> vertexes;
        List<Vector3> textures;
        List<Vector3> normals;
        List<Polygon> polygons;
        ModelFrame frame;

        Vector4 minVect,maxVect;
        public Vector4 MinVect { get => frame.MaxVect; }
        public Vector4 MaxVect { get => frame.MaxVect; }
        public Vector4 Middle { get => frame.Middle; }


        
        public Obj()
        {
            vertexes = new List<Vector4>();
            textures = new List<Vector3>();
            normals = new List<Vector3>();
            polygons = new List<Polygon>();
            minVect= new Vector4(float.MaxValue,float.MaxValue,float.MaxValue,0);
        }

        public void AddVertex(float x, float y, float z, float? w) {
            vertexes.Add(new Vector4(x, y, z,w.HasValue?w.Value:1));
            maxVect.FindMaxes(vertexes.Last());
            minVect.FindMin(vertexes.Last());
        }

        public void AddTexture(float u, float? v, float? w) {
            textures.Add(new Vector3(u, v.HasValue ? v.Value : 0, w.HasValue ? w.Value : 0));
        }
        
        public void AddNormal(float i, float j, float k)
        {
            normals.Add(new Vector3(i, j, k));
        }

        public void AddPolygon(List<(int,int?,int?)> vert)
        {
            polygons.Add(new Polygon(vert));
        }

        public void Finish()
        {
            frame=new ModelFrame(vertexes.Count,polygons,minVect,maxVect);
        }

        public ModelFrame NewFrame()
        {
            frame.ResetVertexes(vertexes);
            return frame;
        }
    }

    struct Polygon 
    {
        public List<int> vertexes;
        public List<int> textures;
        public List <int> normals;
        int vertL;

        public Polygon(List<(int,int?,int?)> vertTextNorm)
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
            vertL=vertexes.Count;
        }

        public IEnumerator<(int f,int s)> GetEnumerator()
        {
            for (int i = 0; i < vertL; i++)
            {
                yield return (vertexes[i], vertexes[(i + 1)%vertL]);
            }
        }
    }
}

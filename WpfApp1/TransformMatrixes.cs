using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace WpfApp1
{
    class TransformMatrixes
    {
        public TransformMatrixes() { }

        public System.Numerics.Matrix4x4 MatrixOfMove(System.Numerics.Vector3 translation){
            return new System.Numerics.Matrix4x4(1, 0, 0, translation.X,
                                                 0, 1, 0, translation.Y,
                                                 0, 0, 1, translation.Z,
                                                 0, 0, 0, 1);
        }
        public System.Numerics.Matrix4x4 MatrixOfScale(System.Numerics.Vector3 scale)
        {
            return new System.Numerics.Matrix4x4(scale.X, 0,       0,       0,
                                                 0,       scale.Y, 0,       0,
                                                 0,       0,       scale.Z, 0,
                                                 0,       0,       0,       1);
        }
        public System.Numerics.Matrix4x4 MatrixOfXRot(double edge)
        {
            edge = edge * Math.PI / 180.0;
            float cos = (float)Math.Cos(edge);
            float sin = (float)Math.Sin(edge); 
            return new System.Numerics.Matrix4x4(1, 0,    0,   0,
                                                 0, cos, -sin, 0,
                                                 0, sin,  cos, 0,
                                                 0, 0,    0,   1);
        }
        public System.Numerics.Matrix4x4 MatrixOfYRot(double edge)
        {
            edge = edge * Math.PI / 180.0;
            float cos = (float)Math.Cos(edge);
            float sin = (float)Math.Sin(edge);
            return new System.Numerics.Matrix4x4(cos,  0, sin, 0,
                                                 0,    1, 0,   0,
                                                 -sin, 0, cos, 0,
                                                 0,    0, 0,   1);
        }
        public System.Numerics.Matrix4x4 MatrixOfZRot(double edge)
        {
            edge = edge * Math.PI / 180.0;
            float cos = (float)Math.Cos(edge);
            float sin = (float)Math.Sin(edge);
            return new System.Numerics.Matrix4x4(cos, -sin, 0, 0,
                                                 sin,  cos, 0, 0,
                                                 0,    0,   1, 0,
                                                 0,    0,   0, 1);
        }
    }
}

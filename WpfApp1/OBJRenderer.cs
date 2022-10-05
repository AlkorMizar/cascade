using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Data.Common;
using System.Windows;
using System.Numerics;
using System.Security.Cryptography;
using System.Drawing;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Diagnostics;

namespace WpfApp1
{
    class OBJRenderer
    {
        private const uint V = 0xFF4C47EF;
        Obj obj;
        Vector3 delta;
        Vector3 currEye = new Vector3(-1, -1, -1); // in sperical
        Vector3 Xway = new Vector3(-1, -1, -1);
        float fov = 60;
        float[][] zMap;

        public OBJRenderer(Obj _obj)
        {
            obj = _obj;
            currEye = GenrateCurrCameraPoint();
            zMap=new float[1][];
            zMap[0]=new float[1];
        }

        private Matrix4x4 GenerateWorldTransform(Vector4 middle)
        {
            return new Matrix4x4(1, 0, 0, 0,
                                 0, 1, 0, 0,
                                 0, 0, 1, 0,
                                 middle.X, middle.Y, middle.Z, 1);
        }

        public void MoveOnDeltaX(float deltaX)
        {
            delta.X += deltaX;
        }

        public void MoveOnDeltaZ(float deltaZ)
        {
            delta.Z += deltaZ;
        }

        public void SetCamera(float pitch,float yaw)
        {

            currEye.X = (float)(pitch / 180 * Math.PI);
            currEye.Y = (float)(yaw / 180 * Math.PI);
        }

        private Vector3 GenrateCurrCameraPoint() {
            var frame = obj.NewFrame(new Vector4(0,1,1,1));

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            var size= (frame.MaxVect - frame.MinVect);

            var max1 = Math.Max(size.X, size.Y);
            var max2 = Math.Max(size.Z, size.Y);
            var dist = (float)Math.Sqrt(max2 * max2 + max1 * max1);


            return new Vector3((float)(90 / 180.0 * Math.PI), (float)(0 / 180.0 * Math.PI), dist );

        }

        private Matrix4x4 GenerateCameraTransform(Vector4 min, Vector4 max, Vector4 mid,ref Vector3 eye)
        {
            var target = new Vector3(mid.X, mid.Y, mid.Z) + delta;

            eye = new Vector3((float)(currEye.Z * Math.Sin(currEye.X) * Math.Sin(currEye.Y)),
                              (float)(currEye.Z * Math.Cos(currEye.X)),
                              (float)(currEye.Z * Math.Sin(currEye.X) * Math.Cos(currEye.Y)));

            Vector3 x = new Vector3((float)Math.Cos(currEye.Y), 0, -(float)Math.Sin(currEye.Y));

            var zAsix = Vector3.Normalize(eye - target);

            var yAsix = Vector3.Normalize(Vector3.Cross(zAsix, x ));

            var xAsix = Vector3.Normalize(Vector3.Cross(yAsix,zAsix));

            return new Matrix4x4(xAsix.X, yAsix.X, zAsix.X, 0,
                                 xAsix.Y, yAsix.Y, zAsix.Y, 0,
                                 xAsix.Z, yAsix.Z, zAsix.Z, 0,
                                 -Vector3.Dot(xAsix, eye), -Vector3.Dot(yAsix, eye), -Vector3.Dot(zAsix, eye), 1);
        }

        private Matrix4x4 GenerateProjectionTransform(int h, int w,float fov)
        {
            float znear = 10;
            float zfar = -1000;
            float zz = zfar / (znear - zfar);
            float aspect = h / (float)w;
            float tan = (float)Math.Tan(Math.PI / 180 * fov / 2);

            return new Matrix4x4(
                    1 / (aspect * tan), 0, 0, 0,
                    0, 1 / tan, 0, 0,
                    0, 0, zz, -1,
                    0, 0, znear * zz, 0
                );
        }

        private Matrix4x4 GenerateWindowTransform(int h, int w, Vector4 min)
        {
            var hlfW = (float)(w / 2.0);
            var hlH = (float)(h / 2.0);
            return new Matrix4x4(hlfW, 0, 0, 0,
                                 0, -hlH, 0, 0,
                                 0, 0, 1, 0,
                                 min.X + hlfW, min.Y + hlH, 0, 1);
        }


        private void AlgDDA(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2,int color)
        {
            var L = Math.Max(Math.Abs(x2y2.X - x1y1.X), Math.Abs(x2y2.Y - x1y1.Y))*2.5f;
            var deltX = (x2y2.X - x1y1.X) / L;
            var deltY = (x2y2.Y - x1y1.Y) / L;
            var deltZ = (x2y2.Z - x1y1.Z) / L;
            var delta = new Vector3(deltX, deltY, deltZ);
            var dot =new Vector3(x1y1.X, x1y1.Y, x1y1.Z) ;

            unsafe
            {
                for (int i = 0; i < Math.Round(L); i++)
                {
                    IntPtr pBackBuffer = bitmap.BackBuffer;
                    var deltRow = (int)Math.Floor(dot.Y);
                    var deltColumn = (int)Math.Floor(dot.X);

                    var depth = dot.Z;
                    dot += delta;

                    var z = zMap[deltRow][deltColumn];
                    if (z < depth)
                    {
                       continue;
                    }

                    pBackBuffer += deltRow * bitmap.BackBufferStride;
                    pBackBuffer += deltColumn * 4;
                   
                    *((int*)pBackBuffer) = color;
                    zMap[deltRow][deltColumn] = depth;
                }
            }
        }

        private void StraigthLine(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2, int color)
        {
            var L = Math.Abs(x2y2.X - x1y1.X)*1.5f;
            var deltX =(x2y2.X - x1y1.X)/L;
            var deltZ = (x2y2.Z - x1y1.Z) / L;

            var delta = new Vector3(deltX, 0, deltZ);
            var dot = new Vector3(x1y1.X, x1y1.Y, x1y1.Z);

            var deltRow = (int)Math.Floor(x1y1.Y);

            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer;
                pBackBuffer += deltRow * bitmap.BackBufferStride;
                
                for (int i = 0; i < Math.Ceiling(L); i++,dot+=delta)
                {

                    var deltColumn = (int)Math.Floor(dot.X);
                    
                    var z = zMap[deltRow][deltColumn];
                    if (z < dot.Z)
                    {
                        continue;
                    }

                    *((int*)pBackBuffer + deltColumn ) = color;

                    zMap[deltRow][deltColumn] = dot.Z;
                }
            }
        }

        public void DrawPolygon(ModelFrame frame, Polygon poly,WriteableBitmap bitmap, Vector3 eye, Vector3 light) {
            var r = Vector3.Dot(Vector3.Normalize(eye), poly.normal);
            if (r <-0.15)
            {
                return;
            }
            var (xy1, xy2, xy3) = poly.GetPolygonCoordsByY(frame);

            var eq12 = new Equation(xy1, xy2);
            var eq13 = new Equation(xy1, xy3);
            var eq23 = new Equation(xy2, xy3);
            var color = GetColor(xy1, xy2, xy3, poly.normal,light);
            
            for (var y = xy1.Y;  y < xy2.Y ; y+=0.5f)
            {
                var xyz1 = eq12.GetCoords(y);
                var xyz2 = eq13.GetCoords(y);
                StraigthLine(bitmap, xyz1, xyz2, color);

            }

            for (var y = xy2.Y;   y < xy3.Y; y += 0.5f)
            {
                var xyz1 = eq23.GetCoords(y);
                var xyz2 = eq13.GetCoords(y);
                StraigthLine(bitmap, xyz1, xyz2, color);
            }
        }

        private int GetColor(Vector4 xy1, Vector4 xy2, Vector4 xy3, Vector3 normal,Vector3 eye) {
            float cos1 =Math.Max(0, Vector3.Dot(eye, normal)) ;
            var cos = (byte)(cos1 * 255);
            var c = System.Drawing.Color.FromArgb(255, cos, cos, cos).ToArgb();

            return c;
        }

        public void Render(WriteableBitmap bitmap)
        {
            var frame = obj.NewFrame(new Vector4());
            Vector3 eye =new Vector3();

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            frame.TranslateTo(GenerateCameraTransform(frame.MinVect, frame.MaxVect, frame.Middle,ref eye));
            frame.TranslateTo(GenerateProjectionTransform(bitmap.PixelHeight, bitmap.PixelWidth,fov));
            frame.DivideByW();
            frame.TranslateTo(GenerateWindowTransform(bitmap.PixelHeight, bitmap.PixelWidth, frame.MinVect));

            try
            {
                bitmap.Lock();
                UpdateZMap(bitmap);
                foreach (var poly in frame)
                {
                    DrawPolygon(frame,poly, bitmap,eye,Vector3.Normalize(new Vector3(1,1,1)));
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        private void UpdateZMap(WriteableBitmap btm) {
            
            var h = btm.PixelHeight;
            var w = btm.PixelWidth;
            
            if (zMap.Length!= btm.PixelHeight && zMap[0].Length != btm.PixelWidth)
            {
                zMap = new float[h][];
                for (int i = 0; i < h; i++)
                {
                    zMap[i] = new float[w];
                }
            }
            
            Parallel.For(0, btm.PixelHeight, (i) => {
                for (int j = 0; j < w; j++)
                {
                    zMap[i][j] = float.MaxValue;
                }
            });
        }
    }
}

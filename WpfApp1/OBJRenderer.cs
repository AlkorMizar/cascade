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

namespace WpfApp1
{
    class OBJRenderer
    {
        Obj obj;
        Vector3 delta;
        Vector3 currEye = new Vector3(-1, -1, -1); // in sperical
        Vector3 Xway = new Vector3(-1, -1, -1);
        float fov = 60;

        public OBJRenderer(Obj _obj)
        {
            obj = _obj;
            currEye = GenrateCurrCameraPoint();
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
            var frame = obj.NewFrame();

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            float w= (frame.MaxVect.X - frame.MinVect.X),h=(frame.MaxVect.Y - frame.MinVect.Y);
            var dist = Math.Max(w, h);
            dist = dist * 1.5f/(dist / Math.Min(w, h));

            return new Vector3((float)(0 / 180.0 * Math.PI), (float)(90 / 180.0 * Math.PI), dist+frame.MaxVect.Z );

        }

        private Matrix4x4 GenerateCameraTransform(Vector4 min, Vector4 max, Vector4 mid)
        {
            var target = new Vector3(mid.X, mid.Y, mid.Z) + delta;

            var eye = new Vector3((float)(currEye.Z * Math.Cos(currEye.X) * Math.Cos(currEye.Y)),
                                  (float)(currEye.Z * Math.Sin(currEye.X)),
                                  (float)(currEye.Z * Math.Cos(currEye.X) * Math.Sin(currEye.Y)));

            Vector3 x = new Vector3((float)Math.Sin(currEye.Y), 0, -(float)Math.Cos(currEye.Y));

            var zAsix = Vector3.Normalize(eye - target);

            var yAsix = Vector3.Normalize(Vector3.Cross(zAsix, x ));

            var xAsix = Vector3.Normalize(Vector3.Cross(yAsix,zAsix));

            return new Matrix4x4(xAsix.X, yAsix.X, zAsix.X, 0,
                                 xAsix.Y, yAsix.Y, zAsix.Y, 0,
                                 xAsix.Z, yAsix.Z, zAsix.Z, 0,
                                 -Vector3.Dot(xAsix, eye), -Vector3.Dot(yAsix, eye), -Vector3.Dot(zAsix, eye), 1);
            /*
             
            var target = new Vector3(mid.X, mid.Y, max.Z) + delta;

            var dist = Math.Max((max.X - min.X), (max.Y - min.Y)) ;


            var eye = target;

            eye.Z += dist;

            currEye = new Vector3(0, 0, eye.Z);

            var up = new Vector3(0, 1, 0);

            var zAsix = Vector3.Normalize(eye - target);

            var xAsix = Vector3.Normalize(Vector3.Cross(up, zAsix));

            Xway = xAsix;

            var yAsix = up;

            return new Matrix4x4(xAsix.X, yAsix.X, zAsix.X, 0,
                                 xAsix.Y, yAsix.Y, zAsix.Y, 0,
                                 xAsix.Z, yAsix.Z, zAsix.Z, 0,
                                 -Vector3.Dot(xAsix, eye), -Vector3.Dot(yAsix, eye), -Vector3.Dot(zAsix, eye), 1);
             
             */
        }

        private Matrix4x4 GenerateProjectionTransform(int h, int w, Vector4 min, Vector4 max, float fov)
        {
            float znear = max.Z;
            float zfar = min.Z;
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


        private void AlgDDA(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2)
        {
            var L = Math.Max(Math.Abs(x2y2.X - x1y1.X), Math.Abs(x2y2.Y - x1y1.Y));
            var deltX = (x2y2.X - x1y1.X) / L;
            var deltY = (x2y2.Y - x1y1.Y) / L;

            float plusX = 0, plusY = 0;

            int black = ~0;

            unsafe
            {
                for (int i = 0; i < Math.Round(L); i++)
                {
                    IntPtr pBackBuffer = bitmap.BackBuffer;

                    pBackBuffer += (int)Math.Round(x1y1.Y + plusY) * bitmap.BackBufferStride;
                    pBackBuffer += (int)Math.Round(x1y1.X + plusX) * 4;

                    plusX += deltX;
                    plusY += deltY;

                    *((int*)pBackBuffer) = black;
                }
            }
        }

        public void Render(WriteableBitmap bitmap)
        {
            var frame = obj.NewFrame();

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            frame.TranslateTo(GenerateCameraTransform(frame.MinVect, frame.MaxVect, frame.Middle));
            frame.TranslateTo(GenerateProjectionTransform(bitmap.PixelHeight, bitmap.PixelWidth, frame.MinVect, frame.MaxVect,fov));
            frame.DivideByW();
            frame.TranslateTo(GenerateWindowTransform(bitmap.PixelHeight, bitmap.PixelWidth, frame.MinVect));

            try
            {
                bitmap.Lock();
                foreach (var line in frame)
                {
                    AlgDDA(bitmap, line.x1y1, line.x2y2);
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                }
            }
            finally
            {
                bitmap.Unlock();
            }
        }
    }
}

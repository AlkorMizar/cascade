using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Numerics;

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
        Colors coloGen;

        public OBJRenderer(Obj _obj)
        {
            obj = _obj;
            currEye = GenrateCurrCameraPoint();
            zMap = new float[1][];
            zMap[0] = new float[1];
            var c = System.Drawing.Color.FromArgb(255,225,225,225);
            var L = Vector3.Normalize(new Vector3(0, 1, 1));
            coloGen = new Colors(c, 0.15f, c, 0.7f, L, c, 0.7f, 15f, L);
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

        public void SetCamera(float pitch, float yaw)
        {

            currEye.X = (float)(pitch / 180 * Math.PI);
            currEye.Y = (float)(yaw / 180 * Math.PI);
        }

        private Vector3 GenrateCurrCameraPoint()
        {
            var frame = obj.NewFrame();

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            var size = (frame.MaxVect - frame.MinVect);

            var max1 = Math.Max(size.X, size.Y);
            var max2 = Math.Max(size.Z, size.Y);
            var dist = (float)Math.Sqrt(max2 * max2 + max1 * max1);


            return new Vector3((float)(90 / 180.0 * Math.PI), (float)(0 / 180.0 * Math.PI), dist);

        }

        private Matrix4x4 GenerateCameraTransform(Vector4 min, Vector4 max, Vector4 mid, ref Vector3 eye)
        {
            var target = new Vector3(mid.X, mid.Y, mid.Z) + delta;

            eye = new Vector3((float)(currEye.Z * Math.Sin(currEye.X) * Math.Sin(currEye.Y)),
                              (float)(currEye.Z * Math.Cos(currEye.X)),
                              (float)(currEye.Z * Math.Sin(currEye.X) * Math.Cos(currEye.Y)));

            Vector3 x = new Vector3((float)Math.Cos(currEye.Y), 0, -(float)Math.Sin(currEye.Y));

            var zAsix = Vector3.Normalize(eye - target);

            var yAsix = Vector3.Normalize(Vector3.Cross(zAsix, x));

            var xAsix = Vector3.Normalize(Vector3.Cross(yAsix, zAsix));

            return new Matrix4x4(xAsix.X, yAsix.X, zAsix.X, 0,
                                 xAsix.Y, yAsix.Y, zAsix.Y, 0,
                                 xAsix.Z, yAsix.Z, zAsix.Z, 0,
                                 -Vector3.Dot(xAsix, eye), -Vector3.Dot(yAsix, eye), -Vector3.Dot(zAsix, eye), 1);
        }

        private Matrix4x4 GenerateProjectionTransform(int h, int w, float fov)
        {
            float znear = 1;
            float zfar = 100;
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


        private void AlgDDA(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2, int color)
        {
            var L = Math.Max(Math.Abs(x2y2.X - x1y1.X), Math.Abs(x2y2.Y - x1y1.Y)) * 2f;
            var deltX = (x2y2.X - x1y1.X) / L;
            var deltY = (x2y2.Y - x1y1.Y) / L;
            var deltZ = (x2y2.Z - x1y1.Z) / L;
            var delta = new Vector3(deltX, deltY, deltZ);
            var dot = new Vector3(x1y1.X, x1y1.Y, x1y1.Z);

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
            var L = Math.Abs(x2y2.X - x1y1.X) * 2f;
            var deltX = (x2y2.X - x1y1.X) / L;
            var deltZ = (x2y2.Z - x1y1.Z) / L;

            var delta = new Vector3(deltX, 0, deltZ);
            var dot = new Vector3(x1y1.X, x1y1.Y, x1y1.Z);

            var deltRow = (int)Math.Floor(x1y1.Y);

            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer;
                pBackBuffer += deltRow * bitmap.BackBufferStride;

                for (int i = 0; i < Math.Ceiling(L); i++, dot += delta)
                {

                    var deltColumn = (int)Math.Floor(dot.X);

                    var z = zMap[deltRow][deltColumn];
                    if (z < dot.Z)
                    {
                        continue;
                    }

                    *((int*)pBackBuffer + deltColumn) = color;

                    zMap[deltRow][deltColumn] = dot.Z;
                }
            }
        }

        private void StraigthLine(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2, Vector3 n1, Vector3 n2)
        {
            var L = (float)Math.Ceiling(Math.Abs(x2y2.X - x1y1.X) * 2f);
            var deltX = (float)((x2y2.X - x1y1.X) / L);
            var deltZ = (float)((x2y2.Z - x1y1.Z) / L);

            var delta = new Vector4(deltX, 0, deltZ, 0);
            var dot = new Vector4(x1y1.X, x1y1.Y, x1y1.Z, 0);

            var deltRow = (int)Math.Floor(x1y1.Y);

            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer;
                pBackBuffer += deltRow * bitmap.BackBufferStride;

                for (int i = 0; i <= L; i++, dot += delta)
                {

                    var deltColumn = (int)Math.Floor(dot.X);

                    var z = zMap[deltRow][deltColumn];
                    if (z < dot.Z)
                    {
                        continue;
                    }
                    var n = Vector3.Lerp(n1, n2, i / L);
                    *((int*)pBackBuffer + deltColumn) = coloGen.FonColor + coloGen.GetDifColor(n);

                    zMap[deltRow][deltColumn] = dot.Z;
                }
            }
        }

        private void StraigthLine(WriteableBitmap bitmap, Vector4 x1y1, Vector4 x2y2, Vector3 n1, Vector3 n2, Vector3 e1, Vector3 e2)
        {
            var L = (float)Math.Ceiling(Math.Abs(x2y2.X - x1y1.X) * 2f);
            var deltX = (float)((x2y2.X - x1y1.X) / L);
            var deltZ = (float)((x2y2.Z - x1y1.Z) / L);

            var delta = new Vector4(deltX, 0, deltZ, 0);
            var dot = new Vector4(x1y1.X, x1y1.Y, x1y1.Z, 0);

            var deltRow = (int)Math.Floor(x1y1.Y);

            unsafe
            {
                IntPtr pBackBuffer = bitmap.BackBuffer;
                pBackBuffer += deltRow * bitmap.BackBufferStride;

                for (int i = 0; i <= L; i++, dot += delta)
                {

                    var deltColumn = (int)Math.Floor(dot.X);

                    var z = zMap[deltRow][deltColumn];
                    if (z < dot.Z)
                    {
                        continue;
                    }
                    var n = Vector3.Lerp(n1, n2, i / L);
                    var e = Vector3.Lerp(e1, e2, i / L);
                    *((int*)pBackBuffer + deltColumn) = coloGen.CalcAll(n,e);

                    zMap[deltRow][deltColumn] = dot.Z;
                }
            }
        }

        public void DrawPolygon(ModelFrame frame, Polygon poly, WriteableBitmap bitmap, Vector4 eye)
        {
            var tmp = poly.GetNormal(frame);
            if (tmp < float.MinValue)
            {
                return;
            }
            var (xy1, n1, e1, xy2, n2, e2, xy3, n3, e3) = poly.GetPolygonCoordsWithNormByY(frame, eye);

            var eq12 = new Equation(xy1, xy2, n1, n2, e1, e2);
            var eq13 = new Equation(xy1, xy3, n1, n3, e1, e3);
            var eq23 = new Equation(xy2, xy3, n2, n3, e2, e3);

            for (var y = xy1.Y; y < xy2.Y; y += 0.9f)
            {
                var (xyz1, _n1, _e1) = eq12.GetCoordsWithNormalandE(y);
                var (xyz2, _n2, _e2) = eq13.GetCoordsWithNormalandE(y);
                StraigthLine(bitmap, xyz1, xyz2, _n1, _n2,_e1,_e2);

            }

            for (var y = xy2.Y; y < xy3.Y; y += 0.9f)
            {
                var (xyz1, _n1, _e1) = eq23.GetCoordsWithNormalandE(y);
                var (xyz2, _n2, _e2) = eq13.GetCoordsWithNormalandE(y);
                StraigthLine(bitmap, xyz1, xyz2, _n1, _n2, _e1, _e2);
            }
        }

        private int GetColor(Vector3 normal, Vector3 light)
        {
            float cos1 = Math.Max(0, Vector3.Dot(light, normal));
            var cos = (byte)(cos1 * 255);
            var c = System.Drawing.Color.FromArgb(255, cos, cos, cos).ToArgb();

            return c;
        }

        public void Render(WriteableBitmap bitmap)
        {
            var frame = obj.NewFrame();
            Vector3 eye = new Vector3();

            frame.TranslateTo(GenerateWorldTransform(-frame.Middle));
            frame.TranslateTo(GenerateCameraTransform(frame.MinVect, frame.MaxVect, frame.Middle, ref eye));
            frame.TranslateTo(GenerateProjectionTransform(bitmap.PixelHeight, bitmap.PixelWidth, fov));
            frame.DivideByW();
            frame.TranslateTo(GenerateWindowTransform(bitmap.PixelHeight, bitmap.PixelWidth, frame.MinVect));

            try
            {
                bitmap.Lock();
                UpdateZMap(bitmap);
                var eye4 = new Vector4(eye.X, eye.Y, eye.Z, 1);
                foreach (var poly in frame)
                {
                    DrawPolygon(frame, poly, bitmap, eye4);
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        private void UpdateZMap(WriteableBitmap btm)
        {

            var h = btm.PixelHeight;
            var w = btm.PixelWidth;

            if (zMap.Length != btm.PixelHeight && zMap[0].Length != btm.PixelWidth)
            {
                zMap = new float[h][];
                for (int i = 0; i < h; i++)
                {
                    zMap[i] = new float[w];
                }
            }

            Parallel.For(0, btm.PixelHeight, (i) =>
            {
                for (int j = 0; j < w; j++)
                {
                    zMap[i][j] = float.MaxValue;
                }
            });
        }
    }

    struct Colors
    {
        private System.Drawing.Color ia;
        private float ka;
        public int FonColor
        {
            get
            {
                return System.Drawing.Color.FromArgb(255, (byte)(ia.R * ka), (byte)(ia.G * ka), (byte)(ia.B * ka)).ToArgb();
            }
        }
        private System.Drawing.Color id;
        private float kd;
        private Vector3 L;
        public int GetDifColor(Vector3 N)
        {
            var k = Math.Max(0, Vector3.Dot(N, L)) * kd;
            return System.Drawing.Color.FromArgb(255 , (byte)(id.R * k), (byte)(id.G * k), (byte)(id.B * k)).ToArgb();
        }


        private System.Drawing.Color im;
        private float km;
        private float alpha;
        private Vector3 Lm;

        public int GetMirror(Vector3 N,Vector3 E)
        {
            var R =Vector3.Normalize( L - 2 * Vector3.Dot(Lm,N) * N);
            var k = km * Math.Pow(Math.Max(0,Vector3.Dot(R, E)), alpha);
            return System.Drawing.Color.FromArgb(255, (byte)(im.R * k), (byte)(im.G * k), (byte)(im.B * k)).ToArgb();
        }

        public Colors(System.Drawing.Color ia, float ka, System.Drawing.Color id, float kd, Vector3 L, System.Drawing.Color im, float km, float alpha, Vector3 Lm)
        {
            this.ia = ia;
            this.ka = ka;
            this.id = id;
            this.kd = kd;
            this.L = L;
            this.im = im;
            this.km = km;
            this.alpha = alpha;
            this.Lm = Lm;
        }

        public int CalcAll(Vector3 N,Vector3 E)
        {
            var fontC = System.Drawing.Color.FromArgb(255, (byte)(ia.R * ka), (byte)(ia.G * ka), (byte)(ia.B * ka));

            var kdif = Math.Max(0, Vector3.Dot(N, L)) * kd;
            var dif = System.Drawing.Color.FromArgb(255, (byte)(id.R * kdif), (byte)(id.G * kdif), (byte)(id.B * kdif));


            var R = Vector3.Normalize(L - 2 * Vector3.Dot(Lm, N) * N);
            var kmirr = km * Math.Pow(Math.Max(0, Vector3.Dot(R, E)), alpha);
            var mirr = System.Drawing.Color.FromArgb(255, (byte)(im.R * kmirr), (byte)(im.G * kmirr), (byte)(im.B * kmirr));

            byte r = (byte)Math.Min(255, dif.R + fontC.R + mirr.R);
            byte g = (byte)Math.Min(255, dif.G + fontC.G + mirr.G);
            byte b = (byte)Math.Min(255, dif.B + fontC.B + mirr.B);

            return System.Drawing.Color.FromArgb(255,r,g,b).ToArgb();
        }
    }
}

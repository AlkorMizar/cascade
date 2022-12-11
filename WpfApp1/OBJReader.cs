using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace WpfApp1
{
    internal class OBJReader
    {
        BitmapImage txt, norm, mirror;
        public OBJReader()
        {

        }

        public Obj ReadObjFrom(ObjParts parts)
        {
            Obj obj = new Obj();
            var c = CultureInfo.InvariantCulture;
            using (StreamReader reader = new StreamReader(parts.pathToOBJ))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length == 0)
                    {
                        continue;
                    }
                    switch (data[0])
                    {
                        case "v":
                            {
                                obj.AddVertex(float.Parse(data[1], c), float.Parse(data[2], c), float.Parse(data[3], c), data.Length == 5 ? float.Parse(data[4], c) : null);
                                break;
                            }
                        case "vt":
                            {
                                obj.AddTexture(float.Parse(data[1], c), data.Length >= 3 ? float.Parse(data[2], c) : null, data.Length == 4 ? float.Parse(data[3], c) : null);
                                break;
                            }
                        case "vn":
                            {
                                obj.AddNormal(float.Parse(data[1], c), float.Parse(data[2], c), float.Parse(data[3], c));
                                break;
                            }
                        case "f":
                            {
                                List<(int, int?, int?)> polyg = new List<(int, int?, int?)>();
                                for (int i = 1; i < data.Length; i++)
                                {
                                    var dt = data[i].Split('/');
                                    int vert = int.Parse(dt[0]);
                                    int? text = dt.Length >= 2 && dt[1] != "" ? int.Parse(dt[1]) : null;
                                    int? norm = dt.Length == 3 ? int.Parse(dt[2]) : null;

                                    polyg.Add((vert, text, norm));
                                }
                                obj.AddPolygon(polyg);
                                break;
                            }
                    }

                }
                Bitmap txtMap = BitmapImage2Bitmap(new BitmapImage(new Uri(parts.pathToTexture)));
                Bitmap nMap = BitmapImage2Bitmap(new BitmapImage(new Uri(parts.pathToNormal)));
                Bitmap mMap = BitmapImage2Bitmap(new BitmapImage(new Uri(parts.pathToMirr)));

                obj.SetMaps(fromImg(txtMap), fromImg(mMap), fromImg(nMap));
                obj.Finish();
            }
            return obj;
        }
        public static Vector3[,] fromImg(Bitmap image)
        {
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            
            int numBytes = bitmapData.Stride * image.Height;
            byte[] rgbValues = new byte[numBytes];
            byte[] rgbValues2 = new byte[numBytes];
            Marshal.Copy(bitmapData.Scan0, rgbValues, 0, numBytes);
            
            image.UnlockBits(bitmapData);
            var res = new Vector3[image.Height, image.Width];

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    var b =i * bitmapData.Stride + j * 3;
                    res[i, j] = new Vector3(rgbValues[b+2], rgbValues[b + 1], rgbValues[b]);//rgb
                    
                }
            }
            return res;
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string path = @"D:\Projects\NET\cascade\model";
        bool isMoving = false;
        Point prevPosition;
        WriteableBitmap[] bitmaps;
        int currBitmap;
        const int bitmapCount = 2;
        OBJRenderer render;
        float pitch = 0, yaw=90f;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            InitBitmaps();
            StartRender();
        }

        private void InitBitmaps()
        {
            bitmaps = new WriteableBitmap[bitmapCount];

            currBitmap = 0;
            bitmaps[currBitmap] = new WriteableBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                96,
                96,
                PixelFormats.Bgra32,
                null);
            bitmaps[1] = bitmaps[currBitmap].Clone();//switch beatween
            img.Source = bitmaps[currBitmap];

        }

        private void StartRender()
        {
            var parts = GetOBJFiles(path);
            var objReader = new OBJReader();
            var obj = objReader.ReadObjFrom(parts);

            render = new OBJRenderer(obj);
            Redraw();
        }

        private ObjParts GetOBJFiles(string directory)
        {
            string[] files = System.IO.Directory.GetFiles(directory, "*.obj");
            if (files.Length == 0)
            {
                throw new Exception("no files");
            }
            ObjParts parts;
            parts.pathToOBJ = files[0];
            return parts;
        }


        private void Redraw()
        {
            var next = (currBitmap + 1) % bitmapCount;
            
            render.Render(bitmaps[next]);
            img.Source = bitmaps[next];
            ClearWriteableBitmap(bitmaps[currBitmap]);
            currBitmap=next;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern void RtlZeroMemory(IntPtr dst, int length);

        protected void ClearWriteableBitmap(WriteableBitmap bmp)
        {
            RtlZeroMemory(bmp.BackBuffer, bmp.PixelWidth * bmp.PixelHeight * (bmp.Format.BitsPerPixel / 8));

            bmp.Dispatcher.Invoke(() =>
            {
                bmp.Lock();
                bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                bmp.Unlock();
            });
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas.CaptureMouse();
            isMoving = true; 
            prevPosition = e.GetPosition(canvas);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();
            isMoving = false;
            prevPosition = e.GetPosition(canvas);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMoving)
                return;

            float xoffset = (float)(e.GetPosition(canvas).X - prevPosition.X);
            float yoffset = (float)(prevPosition.Y - e.GetPosition(canvas).Y);
            
            prevPosition = e.GetPosition(canvas);

            float sensitivity = 0.3f;


            xoffset *= sensitivity;
            yoffset *= sensitivity;

            yaw += xoffset;
            pitch -= yoffset;

            if (pitch > 89.0f)
                pitch = 89.0f;
            if (pitch < -89.0f)
                pitch = -89.0f;

            render.SetCamera(pitch, yaw);
            Redraw();
        }

    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenshotTaker
{
    public static class Display
    {
        //===================================================================== API
        [DllImport("gdi32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

        //===================================================================== STRUCTS
        private enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }

        //===================================================================== FUNCTIONS
        public static void TakeScreenshot(Screen screen, string path, ImageFormat format)
        {
            TakeScreenshot(screen.Bounds, path, format);
        }
        public static void TakeScreenshot(Rectangle rect, string path, ImageFormat format)
        {
            Size size = new Size(rect.Width, rect.Height); // define dimensions
            using (Bitmap bitmap = new Bitmap(size.Width, size.Height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, size);
                bitmap.Save(path, format);
            }
        }
        public static void TakeScreenshotEx(Rectangle rect, string path, ImageFormat format)
        {
            // initialize
            IntPtr srcDC = GetDC(IntPtr.Zero);
            IntPtr destDC = CreateCompatibleDC(srcDC);
            IntPtr hBitmap = CreateCompatibleBitmap(srcDC, rect.Width, rect.Height);
            SelectObject(destDC, hBitmap);

            // copy
            BitBlt(destDC, 0, 0, rect.Width, rect.Height, srcDC, rect.Left, rect.Top, TernaryRasterOperations.SRCCOPY);
            using (Bitmap bitmap = Image.FromHbitmap(hBitmap)) bitmap.Save(path, format);

            // terminate
            DeleteObject(hBitmap);
            DeleteDC(destDC);
            ReleaseDC(IntPtr.Zero, srcDC);
        }
    }
}

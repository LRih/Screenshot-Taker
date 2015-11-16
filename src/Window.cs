using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ScreenshotTaker
{
    public class Window
    {
        //===================================================================== API
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)] private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
        [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetCursorPos(out Point lpPoint);

        //===================================================================== STRUCTS
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        //===================================================================== ENUMS
        private enum SpecialWindowHandles
        {
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2
        }
        [Flags]
        private enum SetWindowPosFlags : uint
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000
        }
        public enum SelectionMethod
        {
            ForegroundParent, MousePositionFrame
        }

        //===================================================================== VARIABLES
        private IntPtr _handle;

        //===================================================================== INITIALIZE
        public Window(SelectionMethod windowType)
        {
            if (windowType == SelectionMethod.ForegroundParent) _handle = GetForegroundWindow();
            else if (windowType == SelectionMethod.MousePositionFrame)
            {
                Point pt;
                GetCursorPos(out pt);
                _handle = WindowFromPoint(pt.X, pt.Y);
            }
        }
        public Window(string caption)
        {
            _handle = FindWindowByCaption(IntPtr.Zero, caption);
        }

        //===================================================================== PROPERTIES
        public IntPtr Handle
        {
            get { return _handle; }
        }
        public string ClassName
        {
            get
            {
                StringBuilder className = new StringBuilder(100);
                GetClassName(_handle, className, className.Capacity);
                return className.ToString();
            }
        }
        public bool TopMost
        {
            set
            {
                IntPtr hWndInsertAfter = (IntPtr)(value ? SpecialWindowHandles.HWND_TOPMOST : SpecialWindowHandles.HWND_NOTOPMOST);
                SetWindowPos(_handle, hWndInsertAfter, 0, 0, 0, 0, SetWindowPosFlags.NOMOVE | SetWindowPosFlags.NOSIZE);
            }
        }
        public Rectangle Rect
        {
            get
            {
                RECT rect = new RECT();
                GetWindowRect(new HandleRef(this, _handle), out rect);
                return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
        }
        public int Left
        {
            get { return Rect.Left; }
        }
        public int Top
        {
            get { return Rect.Top; }
        }
        public int Right
        {
            get { return Rect.Right; }
        }
        public int Bottom
        {
            get { return Rect.Bottom; }
        }
    }
}

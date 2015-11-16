using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ScreenshotTaker
{
    public class ScreenshotTaker : Form
    {
        //===================================================================== CONSTANTS
        private const Keys HOTKEY = Keys.PrintScreen;
        private const int MARGIN = 12;

        //===================================================================== CONTROLS
        private IContainer _components = new Container();
        private TextBox _txtPath = new TextBox();
        private TextBox _txtName = new TextBox();
        private ComboBox _comScreen = new ComboBox();
        private ComboBox _comFormat = new ComboBox();
        private Button _btnBrowse = new Button();
        private ComboBox _comScope = new ComboBox();
        private ContextMenuStrip _contextMenu;
        private NotifyIcon _notifyIcon;
        private Timer _timer;

        //===================================================================== VARIABLES
        private bool _isTaking = false;

        //===================================================================== INITIALIZE
        public ScreenshotTaker()
        {
            this.ClientSize = new Size(300, 98);

            _txtPath.Location = new Point(MARGIN, MARGIN);
            _txtPath.ReadOnly = true;
            _txtPath.TabStop = false;
            _txtPath.Text = Application.StartupPath;
            _txtPath.SelectionStart = _txtPath.Text.Length;
            _txtPath.Width = ClientSize.Width - MARGIN * 2;

            _txtName.Location = new Point(MARGIN, _txtPath.Bottom + 6);
            _txtName.Text = "screenshot";
            _txtName.Width = 120;

            _comFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            _comFormat.Items.AddRange(new object[] { "jpg", "png" });
            _comFormat.Location = new Point(_txtName.Right + 6, _txtPath.Bottom + 5);
            _comFormat.SelectedIndex = 1;
            _comFormat.Width = 60;

            _btnBrowse.Location = new Point(_comFormat.Right + 6, _comFormat.Top - 1);
            _btnBrowse.Width = ClientSize.Width - _comFormat.Right - 6 - MARGIN;
            _btnBrowse.Text = "Browse";
            _btnBrowse.Click += btnBrowse_Click;

            _comScreen.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (Screen s in Screen.AllScreens)
                _comScreen.Items.Add(string.Format("{0} - {1} x {2}", s.DeviceName, s.Bounds.Width, s.Bounds.Height));
            _comScreen.Location = new Point(MARGIN, _comFormat.Bottom + 6);
            _comScreen.SelectedIndex = 0;
            _comScreen.Width = _comFormat.Right - _txtName.Left;

            _comScope.DropDownStyle = ComboBoxStyle.DropDownList;
            _comScope.Items.AddRange(new object[] { "Screen", "Top Window", "Frame" });
            _comScope.Location = new Point(_comScreen.Right + 6, _comScreen.Top);
            _comScope.SelectedIndex = 0;
            _comScope.Width = _btnBrowse.Width;

            _contextMenu = new ContextMenuStrip(_components);
            _contextMenu.Items.Add("Open Folder", null, menuOpenFolder_Click);
            _contextMenu.Items.Add("Exit", null, menuExit_Click);

            _notifyIcon = new NotifyIcon(_components);
            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.Icon = new Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("ScreenshotTaker.Icon.ico"));
            _notifyIcon.Text = "Screenshot Taker";
            _notifyIcon.Visible = true;
            _notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

            _timer = new Timer(_components);
            _timer.Enabled = true;
            _timer.Interval = 10;
            _timer.Tick += timer_Tick;

            this.Controls.Add(_txtPath);
            this.Controls.Add(_txtName);
            this.Controls.Add(_comFormat);
            this.Controls.Add(_btnBrowse);
            this.Controls.Add(_comScreen);
            this.Controls.Add(_comScope);

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = _notifyIcon.Icon;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "Screenshot Taker v" + Assembly.GetCallingAssembly().GetName().Version.ToString();
            this.TopMost = true;

            this.Load += ScreenshotTaker_Load;
            this.Resize += ScreenshotTaker_Resize;
        }
        private void ScreenshotTaker_Load(object sender, EventArgs e)
        {
            SetWindowLocation();
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null) _components.Dispose();
            base.Dispose(disposing);
        }
        private void menuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //===================================================================== FUNCTIONS
        private void SetWindowLocation()
        {
            this.Location = new Point(SystemInformation.WorkingArea.Right - this.Width, SystemInformation.WorkingArea.Bottom - this.Height);
        }
        private void PlayShutterSound()
        {
            SoundPlayer player = new SoundPlayer(Assembly.GetCallingAssembly().GetManifestResourceStream("ScreenshotTaker.camera.wav"));
            player.Play();
            player.Dispose();
        }

        //===================================================================== PROPERTIES
        private string SavePath
        {
            get
            {
                string path = string.Format(@"{0}\{1}", _txtPath.Text, _txtName.Text);
                string completePath = string.Empty;
                // Loop until screenshot filename does not exist
                int saveNo = -1;
                do
                {
                    saveNo++;
                    completePath = string.Format("{0}{1}.{2}", path, saveNo, Format.ToString().ToLower());
                }
                while (File.Exists(completePath));
                return completePath;
            }
        }
        private ImageFormat Format
        {
            get { return (_comFormat.SelectedIndex == 0 ? ImageFormat.Jpeg : ImageFormat.Png); }
        }
        private Screen SelectedScreen
        {
            get { return Screen.AllScreens[_comScreen.SelectedIndex]; }
        }
        private Rectangle CaptureRectangle
        {
            get
            {
                switch (_comScope.SelectedIndex)
                {
                    case 1: return new Window(Window.SelectionMethod.ForegroundParent).Rect;
                    case 2: return new Window(Window.SelectionMethod.MousePositionFrame).Rect;
                    default: return SelectedScreen.Bounds;
                }
            }
        }

        //===================================================================== EVENTS
        private void timer_Tick(object sender, EventArgs e)
        {
            if (Hotkey.IsKeyDown(HOTKEY) && !_isTaking)
            {
                _isTaking = true;
                _txtName.Text = _txtName.Text.RemoveInvalidCharacters(); // update to regex later
                Display.TakeScreenshot(CaptureRectangle, SavePath, Format); // take screenshot
                PlayShutterSound();
                Hotkey.WaitUntilKeyUp(HOTKEY);
                _isTaking = false;
            }
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    _txtPath.Text = dialog.SelectedPath;
            }
        }
        private void ScreenshotTaker_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) this.Hide();
        }
        private void menuOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_txtPath.Text);
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            SetWindowLocation();
        }
    }
}

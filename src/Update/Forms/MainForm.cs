using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Squirrel.Update.Forms
{
    /// <summary>
    /// 主面板
    /// </summary>
    public partial class MainForm : Form
    {
        private static MainForm _instance = null;
        public static MainForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainForm();
                return _instance;
            }
        }

        private UpdateType updateType = default(UpdateType);
        private string currentVersion = default(string);
        private string latestVersion = default(string);
        private Thread _checkingThread = null;//检查更新线程
        private Thread _updatingThread = null;//更新线程

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            _instance = this;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        public MainForm(string[] args)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            _instance = this;
            this.FormBorderStyle = FormBorderStyle.None;

            if (args.Any(arg => arg.Contains("--autoUpdate")))
                updateType = UpdateType.Auto;
            else if (args.Any(arg => arg.Contains("--remindUpdate")))
            {
                updateType = UpdateType.Remind;
                currentVersion = args[1];
                latestVersion = args[2];
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetButtonNoFocus(Btn_Min);
            SetButtonNoFocus(Btn_Close);

            switch (updateType)
            {
                case UpdateType.Normal:
                    StartCheckingForUpdate();
                    break;
                case UpdateType.Auto:
                    StartUpdating();
                    break;
                case UpdateType.Remind:
                    StartRemindingUpdate();
                    break;
                default:
                    break;
            }
        }

        #region 窗口拖拽
        private Point mouseDownPos;
        private bool mousePressed = false;
        private void Panel_Title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                mouseDownPos = new Point(-e.X, -e.Y);
            mousePressed = true;
        }

        private void Panel_Title_MouseUp(object sender, MouseEventArgs e)
        {
            mousePressed = false;
        }

        private void Panel_Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mousePressed || e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            Point tempPos = Control.MousePosition;
            tempPos.Offset(mouseDownPos);
            this.Location = tempPos;
        }

        #region 如果拖拽的标题栏上有UI
        //private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == System.Windows.Forms.MouseButtons.Left)
        //        mouseDownPos = new Point(-e.X - (sender as PictureBox).Location.X, -e.Y - (sender as PictureBox).Location.Y);
        //    mousePressed = true;
        //}

        //private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        //{
        //    mousePressed = false;
        //}

        //private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!mousePressed || e.Button != System.Windows.Forms.MouseButtons.Left)
        //        return;

        //    Point tempPos = Control.MousePosition;
        //    tempPos.Offset(mouseDownPos);
        //    this.Location = tempPos;
        //}
        #endregion
        #endregion

        #region 最小化按钮和关闭按钮
        private void Btn_Min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Btn_Min_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.min_highlighted;
        }

        private void Btn_Min_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.min_normal;
        }

        private void Btn_Close_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Btn_Close_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.close_highlighted;
        }

        private void Btn_Close_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.close_normal;
        }
        #endregion

        /// <summary>
        /// 开始检查更新
        /// </summary>
        public void StartCheckingForUpdate()
        {
            //显示Loading面板
            LoadingForm.Instance.SetLoadingMessage("正在检查更新");
            ConfigurePanel(LoadingForm.Instance);
            LoadingForm.Instance.loadingType = LoadingType.CheckLoading;
            LoadingForm.Instance.Invalidate();

            //启动检查更新线程
            _checkingThread = new Thread(new ThreadStart(CheckForUpdate))
            {
                IsBackground = true
            };
            _checkingThread.Start();
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        public void StartUpdating()
        {
            //终止检查更新线程
            if (_checkingThread != null)
                _checkingThread.Abort();

            //显示Loading面板
            LoadingForm.Instance.SetLoadingMessage("正在为您下载最新版本");
            ConfigurePanel(LoadingForm.Instance);
            LoadingForm.Instance.loadingType = LoadingType.DownloadLoading;
            LoadingForm.Instance.Label_DownloadInfo.Visible = true;
            LoadingForm.Instance.Invalidate();

            //启动更新线程
            _updatingThread = new Thread(new ThreadStart(Update))
            {
                IsBackground = true
            };
            _updatingThread.Start();
        }

        /// <summary>
        /// 开始提示更新
        /// </summary>
        public void StartRemindingUpdate()
        {
            UpdateForm.Instance.SetUpdateMessage(currentVersion, latestVersion);
            ConfigurePanel(UpdateForm.Instance);
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckForUpdate()
        {
            string[] args = new string[] { string.Format("--checkForUpdate={0}", GlobalPararm.UpdateUrl) };
            Program.excute(args);
        }

        /// <summary>
        /// 更新
        /// </summary>
        private new void Update()
        {
            string[] args = new string[] { string.Format("--update={0}", GlobalPararm.UpdateUrl) };
            Program.excute(args);
        }

        /// <summary>
        /// 设置面板
        /// </summary>
        /// <param name="control"></param>
        public void ConfigurePanel(UserControl control)
        {
            this.Panel_Main.Controls.Clear();
            this.Panel_Main.Controls.Add(control);
            control.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// 点击任务栏图标最小化
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作
                return cp;
            }
        }

        /// <summary>
        /// 设置按钮为无焦点
        /// <remark>防止按下回车触发焦点按钮，也防止焦点在按钮上然后失去焦点时按钮有白色边框的情况</remark>>
        /// </summary>
        /// <param name="button"></param>
        public void SetButtonNoFocus(Button button)
        {
            MethodInfo methodinfo = button.GetType().GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            methodinfo.Invoke(button, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, new object[] { ControlStyles.Selectable, false }, Application.CurrentCulture);
        }
    }
}

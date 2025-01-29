using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Squirrel.Update.Forms
{
    /// <summary>
    /// 加载类型
    /// </summary>
    public enum LoadingType
    {
        CheckLoading,
        DownloadLoading,
        ApplyLoading,
        Failed
    }

    /// <summary>
    /// Loading面板
    /// </summary>
    public partial class LoadingForm : UserControl
    {
        private static LoadingForm _instance = null;
        public static LoadingForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadingForm();
                return _instance;
            }
        }

        private bool _isBackgroundProgressShow = false;

        /// <summary>
        /// 加载类型
        /// </summary>
        public LoadingType loadingType;

        /// <summary>
        /// 进度目标值
        /// </summary>
        public float toProgress = 0;

        /// <summary>
        /// 进度过程是否完成
        /// </summary>
        public bool isCompleted = false;

        private LoadingForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            CustomProgressBar.Visible = true;
        }

        #region 更新日志文本按钮
        private void Label_UpdateLog_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GlobalPararm.UpdateLogUrl);
        }

        private void Label_UpdateLog_MouseEnter(object sender, EventArgs e)
        {
            Label_UpdateLog.ForeColor = ColorTranslator.FromHtml("#0078D7");
        }

        private void Label_UpdateLog_MouseLeave(object sender, EventArgs e)
        {
            Label_UpdateLog.ForeColor = ColorTranslator.FromHtml("#0066CC");
        }
        #endregion
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //绘制logo图片
            //Image img = Properties.Resources.logo;
            //Rectangle rect = new Rectangle((this.Width - img.Width) / 2, (this.Height - img.Height) / 2 - 50, img.Width, img.Height);
            //e.Graphics.DrawImage(img, rect);

            //根据加载类型显示对应的提示信息
            switch (loadingType)
            {
                case LoadingType.CheckLoading:
                    if (_isBackgroundProgressShow)
                    {
                        BackgroundImage = null;
                        _isBackgroundProgressShow = false;
                    }
                    CustomProgressBar.Visible = false;
                    Label_DownloadInfo.Visible = false;
                    Label_Tip.Location = Label_Tip.Text == "正在检查更新" ? new Point((Width - Label_Tip.Width) / 2, (Height - Label_Tip.Height) / 2) : new Point((Width - Label_Tip.Width) / 2, (Height - Label_Tip.Height) / 2 - 20);
                    Label_Tip.ForeColor = Color.White;
                    break;
                case LoadingType.DownloadLoading:
                    if (!_isBackgroundProgressShow)
                    {
                        BackgroundImage = Properties.Resources.background_progress;
                        _isBackgroundProgressShow = true;
                    }
                    CustomProgressBar.Visible = true;
                    Label_Tip.Location = new Point((Width - Label_Tip.Width) / 2, Height / 2 - Label_Tip.Height - 20);
                    Label_Tip.ForeColor = Color.White;
                    Label_DownloadInfo.Location = new Point((Width - Label_DownloadInfo.Width) / 2, CustomProgressBar.Location.Y - Label_DownloadInfo.Height - 5);
                    break;
                case LoadingType.ApplyLoading:
                    if (!_isBackgroundProgressShow)
                    {
                        BackgroundImage = Properties.Resources.background_progress;
                        _isBackgroundProgressShow = true;
                    }
                    CustomProgressBar.Visible = true;
                    Label_DownloadInfo.Visible = false;
                    Label_Tip.Location = new Point((Width - Label_Tip.Width) / 2, Height / 2 - Label_Tip.Height - 20);
                    Label_Tip.ForeColor = Color.White;
                    break;
                case LoadingType.Failed:
                    if (_isBackgroundProgressShow)
                    {
                        BackgroundImage = null;
                        _isBackgroundProgressShow = false;
                    }
                    CustomProgressBar.Visible = false;
                    Label_DownloadInfo.Visible = false;
                    Label_Tip.Location = new Point((Width - Label_Tip.Width) / 2, (Height - Label_Tip.Height) / 2);
                    Label_Tip.ForeColor = Color.FromArgb(212, 48, 48);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置Loading面板提示信息
        /// </summary>
        /// <param name="message"></param>
        public void SetLoadingMessage(string message)
        {
            if (Label_Tip != null)
                Label_Tip.Text = message;
        }

        /// <summary>
        /// 设置下载信息
        /// </summary>
        /// <param name="message"></param>
        public void SetDownloadMessage(string message, bool isVisable)
        {
            if (Label_DownloadInfo != null)
            {
                if (isVisable)
                {
                    Label_DownloadInfo.Text = message;
                    if (!Label_DownloadInfo.Visible)
                    {
                        Label_DownloadInfo.Visible = true;
                        Invalidate();
                    }
                }
                else
                {
                    if (Label_DownloadInfo.Visible)
                    {
                        Label_DownloadInfo.Visible = false;
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 显示更新日志
        /// </summary>
        public void ShowUpdateLog()
        {
            if (Label_UpdateLog != null)
            {
                Label_UpdateLog.Visible = true;
                Label_UpdateLog.Location = new Point((Width - Label_UpdateLog.Width) / 2, Label_Tip.Location.Y + Label_Tip.Height / 2 + Label_UpdateLog.Height / 2);
            }
        }

        /// <summary>
        /// 设置进度条数值
        /// </summary>
        /// <param name="interval">平滑间隔</param>
        public void SetProgressValue(object interval)
        {
            while (true)
            {
                if (isCompleted && CustomProgressBar.Value == 100)
                    break;
                
                if (toProgress > CustomProgressBar.Value)
                {
                    if ((int)interval == 0)
                        CustomProgressBar.Value = toProgress;
                    else
                    {
                        CustomProgressBar.Value += .1f;
                        int i = Environment.TickCount;
                        while (Environment.TickCount - i <= (int)interval)
                        {
                            Application.DoEvents();
                        }
                    }
                }
                else if(toProgress < CustomProgressBar.Value)
                {
                    if (CustomProgressBar.Value == 100)
                        CustomProgressBar.Value = 0;
                    else
                        CustomProgressBar.Value = toProgress;
                }
            }
        }
    }
}

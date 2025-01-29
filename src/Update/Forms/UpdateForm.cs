using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace Squirrel.Update.Forms
{
    /// <summary>
    /// 更新面板
    /// </summary>
    public partial class UpdateForm : UserControl
    {
        private static UpdateForm _instance = null;
        public static UpdateForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UpdateForm();
                return _instance;
            }
        }

        private UpdateForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.Dock = DockStyle.Fill;

            //更新日志网址（加时间戳，避免缓存）
            string url = GlobalPararm.UpdateLogUrl;
            url += url.IndexOf("?") > 0 ? "&" : "?";
            url += "random=" + DateTime.Now.ToString("yyyyMMddHHmmss");
            WebBrowser.Url = new Uri(url);
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            MainForm.Instance.SetButtonNoFocus(Btn_Update);
            MainForm.Instance.SetButtonNoFocus(Btn_Cancel);
        }

        #region 更新按钮
        private void Btn_Update_Click(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_red_pressed;
            MainForm.Instance.StartUpdating();
        }

        private void Btn_Update_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_red_highlighted;
        }

        private void Btn_Update_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_red_normal;
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_blue_pressed;
            Process.GetCurrentProcess().Kill();
        }

        private void Btn_Cancel_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_blue_highlighted;
        }

        private void Btn_Cancel_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackgroundImage = Properties.Resources.button_blue_normal;
        }
        #endregion

        /// <summary>
        /// 设置更新面板提示信息
        /// </summary>
        /// <param name="curVersion">当前版本号</param>
        /// <param name="latVersion">最新版本号</param>
        public void SetUpdateMessage(string curVersion, string latVersion)
        {
            Label_CurVersion.Text = curVersion;
            Label_NewVersion.Text = latVersion;
            this.Invalidate();
        }
    }
}

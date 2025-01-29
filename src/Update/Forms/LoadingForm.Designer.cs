namespace Squirrel.Update.Forms
{
    partial class LoadingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Label_Tip = new System.Windows.Forms.Label();
            this.Label_UpdateLog = new System.Windows.Forms.Label();
            this.Label_DownloadInfo = new System.Windows.Forms.Label();
            this.CustomProgressBar = new Custom.Windows.Forms.CustomProgressBar();
            this.SuspendLayout();
            // 
            // Label_Tip
            // 
            this.Label_Tip.AutoSize = true;
            this.Label_Tip.BackColor = System.Drawing.Color.Transparent;
            this.Label_Tip.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_Tip.ForeColor = System.Drawing.Color.White;
            this.Label_Tip.Location = new System.Drawing.Point(265, 150);
            this.Label_Tip.Margin = new System.Windows.Forms.Padding(0);
            this.Label_Tip.Name = "Label_Tip";
            this.Label_Tip.Size = new System.Drawing.Size(145, 30);
            this.Label_Tip.TabIndex = 0;
            this.Label_Tip.Text = "正在检查更新";
            this.Label_Tip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_UpdateLog
            // 
            this.Label_UpdateLog.AutoSize = true;
            this.Label_UpdateLog.BackColor = System.Drawing.Color.Transparent;
            this.Label_UpdateLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label_UpdateLog.Font = new System.Drawing.Font("微软雅黑", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_UpdateLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.Label_UpdateLog.Location = new System.Drawing.Point(297, 115);
            this.Label_UpdateLog.Margin = new System.Windows.Forms.Padding(0);
            this.Label_UpdateLog.Name = "Label_UpdateLog";
            this.Label_UpdateLog.Size = new System.Drawing.Size(82, 24);
            this.Label_UpdateLog.TabIndex = 3;
            this.Label_UpdateLog.Text = "更新日志";
            this.Label_UpdateLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label_UpdateLog.Visible = false;
            this.Label_UpdateLog.Click += new System.EventHandler(this.Label_UpdateLog_Click);
            this.Label_UpdateLog.MouseEnter += new System.EventHandler(this.Label_UpdateLog_MouseEnter);
            this.Label_UpdateLog.MouseLeave += new System.EventHandler(this.Label_UpdateLog_MouseLeave);
            // 
            // Label_DownloadInfo
            // 
            this.Label_DownloadInfo.AutoSize = true;
            this.Label_DownloadInfo.BackColor = System.Drawing.Color.Transparent;
            this.Label_DownloadInfo.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label_DownloadInfo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_DownloadInfo.ForeColor = System.Drawing.Color.Silver;
            this.Label_DownloadInfo.Location = new System.Drawing.Point(219, 207);
            this.Label_DownloadInfo.Margin = new System.Windows.Forms.Padding(0);
            this.Label_DownloadInfo.Name = "Label_DownloadInfo";
            this.Label_DownloadInfo.Size = new System.Drawing.Size(234, 20);
            this.Label_DownloadInfo.TabIndex = 4;
            this.Label_DownloadInfo.Text = "下载速度: 0 B/s   剩余时间: 00:00:00";
            this.Label_DownloadInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label_DownloadInfo.Visible = false;
            // 
            // CustomProgressBar
            // 
            this.CustomProgressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.CustomProgressBar.ForeColor = System.Drawing.Color.White;
            this.CustomProgressBar.Location = new System.Drawing.Point(88, 238);
            this.CustomProgressBar.Margin = new System.Windows.Forms.Padding(0);
            this.CustomProgressBar.Name = "CustomProgressBar";
            this.CustomProgressBar.ShowPercentage = true;
            this.CustomProgressBar.Size = new System.Drawing.Size(500, 15);
            this.CustomProgressBar.Step = 1;
            this.CustomProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.CustomProgressBar.TabIndex = 1;
            this.CustomProgressBar.Value = 0F;
            // 
            // LoadingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.Label_DownloadInfo);
            this.Controls.Add(this.Label_UpdateLog);
            this.Controls.Add(this.Label_Tip);
            this.Controls.Add(this.CustomProgressBar);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "LoadingForm";
            this.Size = new System.Drawing.Size(676, 380);
            this.Load += new System.EventHandler(this.LoadingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Custom.Windows.Forms.CustomProgressBar CustomProgressBar;
        private System.Windows.Forms.Label Label_Tip;
        private System.Windows.Forms.Label Label_UpdateLog;
        public System.Windows.Forms.Label Label_DownloadInfo;
    }
}
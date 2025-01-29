namespace Squirrel.Update.Forms
{
    partial class UpdateForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Label_CurVersion = new System.Windows.Forms.Label();
            this.Label_CurVersionDesc = new System.Windows.Forms.Label();
            this.Label_NewVersion = new System.Windows.Forms.Label();
            this.Label_NewVersionDesc = new System.Windows.Forms.Label();
            this.Label_UpdateDesc = new System.Windows.Forms.Label();
            this.Btn_Update = new System.Windows.Forms.Button();
            this.Panel_Html = new System.Windows.Forms.Panel();
            this.WebBrowser = new System.Windows.Forms.WebBrowser();
            this.Panel_Update = new System.Windows.Forms.Panel();
            this.Btn_Cancel = new System.Windows.Forms.Button();
            this.Panel_Html.SuspendLayout();
            this.Panel_Update.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label_CurVersion
            // 
            this.Label_CurVersion.AutoSize = true;
            this.Label_CurVersion.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_CurVersion.ForeColor = System.Drawing.Color.White;
            this.Label_CurVersion.Location = new System.Drawing.Point(320, 52);
            this.Label_CurVersion.Margin = new System.Windows.Forms.Padding(0);
            this.Label_CurVersion.Name = "Label_CurVersion";
            this.Label_CurVersion.Size = new System.Drawing.Size(62, 30);
            this.Label_CurVersion.TabIndex = 2;
            this.Label_CurVersion.Text = "0.0.0";
            // 
            // Label_CurVersionDesc
            // 
            this.Label_CurVersionDesc.AutoSize = true;
            this.Label_CurVersionDesc.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_CurVersionDesc.ForeColor = System.Drawing.Color.White;
            this.Label_CurVersionDesc.Location = new System.Drawing.Point(220, 52);
            this.Label_CurVersionDesc.Margin = new System.Windows.Forms.Padding(0);
            this.Label_CurVersionDesc.Name = "Label_CurVersionDesc";
            this.Label_CurVersionDesc.Size = new System.Drawing.Size(106, 30);
            this.Label_CurVersionDesc.TabIndex = 0;
            this.Label_CurVersionDesc.Text = "当前版本:";
            // 
            // Label_NewVersion
            // 
            this.Label_NewVersion.AutoSize = true;
            this.Label_NewVersion.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_NewVersion.ForeColor = System.Drawing.Color.White;
            this.Label_NewVersion.Location = new System.Drawing.Point(481, 52);
            this.Label_NewVersion.Margin = new System.Windows.Forms.Padding(0);
            this.Label_NewVersion.Name = "Label_NewVersion";
            this.Label_NewVersion.Size = new System.Drawing.Size(62, 30);
            this.Label_NewVersion.TabIndex = 3;
            this.Label_NewVersion.Text = "0.0.0";
            // 
            // Label_NewVersionDesc
            // 
            this.Label_NewVersionDesc.AutoSize = true;
            this.Label_NewVersionDesc.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_NewVersionDesc.ForeColor = System.Drawing.Color.White;
            this.Label_NewVersionDesc.Location = new System.Drawing.Point(381, 52);
            this.Label_NewVersionDesc.Margin = new System.Windows.Forms.Padding(0);
            this.Label_NewVersionDesc.Name = "Label_NewVersionDesc";
            this.Label_NewVersionDesc.Size = new System.Drawing.Size(106, 30);
            this.Label_NewVersionDesc.TabIndex = 1;
            this.Label_NewVersionDesc.Text = "最新版本:";
            // 
            // Label_UpdateDesc
            // 
            this.Label_UpdateDesc.AutoSize = true;
            this.Label_UpdateDesc.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label_UpdateDesc.ForeColor = System.Drawing.Color.White;
            this.Label_UpdateDesc.Location = new System.Drawing.Point(132, 52);
            this.Label_UpdateDesc.Margin = new System.Windows.Forms.Padding(0);
            this.Label_UpdateDesc.Name = "Label_UpdateDesc";
            this.Label_UpdateDesc.Size = new System.Drawing.Size(85, 30);
            this.Label_UpdateDesc.TabIndex = 1;
            this.Label_UpdateDesc.Text = string.Format("{0}", GlobalPararm.ProductName);
            // 
            // Btn_Update
            // 
            this.Btn_Update.BackgroundImage = global::Squirrel.Update.Properties.Resources.button_red_normal;
            this.Btn_Update.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_Update.FlatAppearance.BorderSize = 0;
            this.Btn_Update.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_Update.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_Update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Update.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_Update.ForeColor = System.Drawing.Color.White;
            this.Btn_Update.Location = new System.Drawing.Point(137, 295);
            this.Btn_Update.Margin = new System.Windows.Forms.Padding(0);
            this.Btn_Update.Name = "Btn_Update";
            this.Btn_Update.Size = new System.Drawing.Size(185, 45);
            this.Btn_Update.TabIndex = 5;
            this.Btn_Update.Text = "立即更新";
            this.Btn_Update.UseVisualStyleBackColor = true;
            this.Btn_Update.Click += new System.EventHandler(this.Btn_Update_Click);
            this.Btn_Update.MouseEnter += new System.EventHandler(this.Btn_Update_MouseEnter);
            this.Btn_Update.MouseLeave += new System.EventHandler(this.Btn_Update_MouseLeave);
            // 
            // Panel_Html
            // 
            this.Panel_Html.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Html.Controls.Add(this.WebBrowser);
            this.Panel_Html.ForeColor = System.Drawing.Color.Transparent;
            this.Panel_Html.Location = new System.Drawing.Point(137, 100);
            this.Panel_Html.Margin = new System.Windows.Forms.Padding(0);
            this.Panel_Html.Name = "Panel_Html";
            this.Panel_Html.Size = new System.Drawing.Size(402, 170);
            this.Panel_Html.TabIndex = 6;
            // 
            // WebBrowser
            // 
            this.WebBrowser.AllowWebBrowserDrop = false;
            this.WebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebBrowser.IsWebBrowserContextMenuEnabled = false;
            this.WebBrowser.Location = new System.Drawing.Point(0, 0);
            this.WebBrowser.Margin = new System.Windows.Forms.Padding(0);
            this.WebBrowser.Name = "WebBrowser";
            this.WebBrowser.Size = new System.Drawing.Size(402, 170);
            this.WebBrowser.TabIndex = 4;
            this.WebBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // Panel_Update
            // 
            this.Panel_Update.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Update.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Panel_Update.Controls.Add(this.Btn_Cancel);
            this.Panel_Update.Controls.Add(this.Panel_Html);
            this.Panel_Update.Controls.Add(this.Btn_Update);
            this.Panel_Update.Controls.Add(this.Label_UpdateDesc);
            this.Panel_Update.Controls.Add(this.Label_NewVersionDesc);
            this.Panel_Update.Controls.Add(this.Label_NewVersion);
            this.Panel_Update.Controls.Add(this.Label_CurVersionDesc);
            this.Panel_Update.Controls.Add(this.Label_CurVersion);
            this.Panel_Update.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Update.Location = new System.Drawing.Point(0, 0);
            this.Panel_Update.Margin = new System.Windows.Forms.Padding(0);
            this.Panel_Update.Name = "Panel_Update";
            this.Panel_Update.Size = new System.Drawing.Size(678, 382);
            this.Panel_Update.TabIndex = 4;
            // 
            // Btn_Cancel
            // 
            this.Btn_Cancel.BackgroundImage = global::Squirrel.Update.Properties.Resources.button_blue_normal;
            this.Btn_Cancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_Cancel.FlatAppearance.BorderSize = 0;
            this.Btn_Cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_Cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Cancel.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_Cancel.ForeColor = System.Drawing.Color.White;
            this.Btn_Cancel.Location = new System.Drawing.Point(354, 295);
            this.Btn_Cancel.Margin = new System.Windows.Forms.Padding(0);
            this.Btn_Cancel.Name = "Btn_Cancel";
            this.Btn_Cancel.Size = new System.Drawing.Size(185, 45);
            this.Btn_Cancel.TabIndex = 7;
            this.Btn_Cancel.Text = "暂不更新";
            this.Btn_Cancel.UseVisualStyleBackColor = true;
            this.Btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            this.Btn_Cancel.MouseEnter += new System.EventHandler(this.Btn_Cancel_MouseEnter);
            this.Btn_Cancel.MouseLeave += new System.EventHandler(this.Btn_Cancel_MouseLeave);
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.Panel_Update);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UpdateForm";
            this.Size = new System.Drawing.Size(678, 382);
            this.Load += new System.EventHandler(this.UpdateForm_Load);
            this.Panel_Html.ResumeLayout(false);
            this.Panel_Update.ResumeLayout(false);
            this.Panel_Update.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label Label_CurVersion;
        private System.Windows.Forms.Label Label_CurVersionDesc;
        private System.Windows.Forms.Label Label_NewVersion;
        private System.Windows.Forms.Label Label_NewVersionDesc;
        private System.Windows.Forms.Label Label_UpdateDesc;
        private System.Windows.Forms.Button Btn_Update;
        private System.Windows.Forms.Panel Panel_Html;
        private System.Windows.Forms.WebBrowser WebBrowser;
        private System.Windows.Forms.Panel Panel_Update;
        private System.Windows.Forms.Button Btn_Cancel;
    }
}

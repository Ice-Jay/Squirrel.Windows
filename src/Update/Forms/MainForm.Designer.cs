namespace Squirrel.Update.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Panel_Main = new System.Windows.Forms.Panel();
            this.Panel_Title = new System.Windows.Forms.Panel();
            this.Btn_Min = new System.Windows.Forms.Button();
            this.Btn_Close = new System.Windows.Forms.Button();
            this.Panel_Background = new System.Windows.Forms.Panel();
            this.Panel_Title.SuspendLayout();
            this.Panel_Background.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Main
            // 
            this.Panel_Main.AutoSize = true;
            this.Panel_Main.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Main.Location = new System.Drawing.Point(0, 0);
            this.Panel_Main.Margin = new System.Windows.Forms.Padding(0);
            this.Panel_Main.Name = "Panel_Main";
            this.Panel_Main.Size = new System.Drawing.Size(676, 380);
            this.Panel_Main.TabIndex = 1;
            // 
            // Panel_Title
            // 
            this.Panel_Title.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Title.Controls.Add(this.Btn_Min);
            this.Panel_Title.Controls.Add(this.Btn_Close);
            this.Panel_Title.Location = new System.Drawing.Point(0, 0);
            this.Panel_Title.Margin = new System.Windows.Forms.Padding(0);
            this.Panel_Title.Name = "Panel_Title";
            this.Panel_Title.Size = new System.Drawing.Size(676, 50);
            this.Panel_Title.TabIndex = 0;
            this.Panel_Title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_Title_MouseDown);
            this.Panel_Title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Panel_Title_MouseMove);
            this.Panel_Title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Panel_Title_MouseUp);
            // 
            // Btn_Min
            // 
            this.Btn_Min.BackgroundImage = global::Squirrel.Update.Properties.Resources.min_normal;
            this.Btn_Min.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.Btn_Min.FlatAppearance.BorderSize = 0;
            this.Btn_Min.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_Min.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_Min.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Min.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_Min.Location = new System.Drawing.Point(590, 12);
            this.Btn_Min.Margin = new System.Windows.Forms.Padding(0);
            this.Btn_Min.Name = "Btn_Min";
            this.Btn_Min.Size = new System.Drawing.Size(25, 25);
            this.Btn_Min.TabIndex = 2;
            this.Btn_Min.TabStop = false;
            this.Btn_Min.UseVisualStyleBackColor = true;
            this.Btn_Min.Click += new System.EventHandler(this.Btn_Min_Click);
            this.Btn_Min.MouseEnter += new System.EventHandler(this.Btn_Min_MouseEnter);
            this.Btn_Min.MouseLeave += new System.EventHandler(this.Btn_Min_MouseLeave);
            // 
            // Btn_Close
            // 
            this.Btn_Close.BackColor = System.Drawing.Color.Transparent;
            this.Btn_Close.BackgroundImage = global::Squirrel.Update.Properties.Resources.close_normal;
            this.Btn_Close.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Btn_Close.Cursor = System.Windows.Forms.Cursors.Default;
            this.Btn_Close.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.Btn_Close.FlatAppearance.BorderSize = 0;
            this.Btn_Close.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_Close.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_Close.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_Close.Location = new System.Drawing.Point(630, 16);
            this.Btn_Close.Margin = new System.Windows.Forms.Padding(0);
            this.Btn_Close.Name = "Btn_Close";
            this.Btn_Close.Size = new System.Drawing.Size(20, 20);
            this.Btn_Close.TabIndex = 3;
            this.Btn_Close.TabStop = false;
            this.Btn_Close.UseVisualStyleBackColor = false;
            this.Btn_Close.Click += new System.EventHandler(this.Btn_Close_Click);
            this.Btn_Close.MouseEnter += new System.EventHandler(this.Btn_Close_MouseEnter);
            this.Btn_Close.MouseLeave += new System.EventHandler(this.Btn_Close_MouseLeave);
            // 
            // Panel_Background
            // 
            this.Panel_Background.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Background.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Panel_Background.Controls.Add(this.Panel_Title);
            this.Panel_Background.Controls.Add(this.Panel_Main);
            this.Panel_Background.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Background.Location = new System.Drawing.Point(0, 0);
            this.Panel_Background.Margin = new System.Windows.Forms.Padding(0);
            this.Panel_Background.Name = "Panel_Background";
            this.Panel_Background.Size = new System.Drawing.Size(676, 380);
            this.Panel_Background.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(676, 380);
            this.Controls.Add(this.Panel_Background);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = string.Format("{0}更新程序", GlobalPararm.ProductName);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Panel_Title.ResumeLayout(false);
            this.Panel_Background.ResumeLayout(false);
            this.Panel_Background.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        public System.Windows.Forms.Panel Panel_Title;
        private System.Windows.Forms.Button Btn_Min;
        private System.Windows.Forms.Button Btn_Close;
        private System.Windows.Forms.Panel Panel_Background;
    }
}

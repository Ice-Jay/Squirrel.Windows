using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace Custom.Windows.Forms
{
    /// <summary>
    /// 自定义进度条
    /// </summary>
    public partial class CustomProgressBar : System.Windows.Forms.ProgressBar
    {
        public CustomProgressBar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
        }

        private float _value;
        /// <summary>
        /// 进度值
        /// </summary>
        public new float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > Maximum - Minimum) value = Maximum - Minimum;
                _value = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 是否显示进度百分比
        /// </summary>
        public bool ShowPercentage { get; set; } = false;

        //private Color _themeColor = Color.Purple;
        private Color _headColor = Color.FromArgb(54, 78, 151);//进度条首部颜色
        private Color _tailColor = Color.FromArgb(81, 174, 188);//进度条尾部颜色
        private Font _font = new Font("微软雅黑", 10f, FontStyle.Regular);//进度百分比字体
        
        /// <summary>
        /// 重绘进度条
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            ControlPaint.DrawBorder(g, ClientRectangle, Color.Gray, ButtonBorderStyle.None);
            //Image img = Squirrel.Update.Properties.Resources.progress_background;
            //e.Graphics.DrawImage(img, ClientRectangle);

            RectangleF currentRect = new RectangleF
            {
                X = 0,
                Y = 0,
                Width = Value / 100 * (Width - 0),
                Height = Height - 0
            };

            if (currentRect.Width != 0)
            {
                using (Brush brush = new LinearGradientBrush(currentRect, _headColor, _tailColor, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, currentRect);
                }
            }

            if (ShowPercentage)
            {
                StringFormat strFormat = new StringFormat();
                string percentageText = ((int)Value).ToString() + "%";
                SizeF percentageTextSize = SizeF.Empty;
                RectangleF percentageTextRect = RectangleF.Empty;

                strFormat.Alignment = StringAlignment.Near;
                strFormat.LineAlignment = StringAlignment.Center;
                strFormat.FormatFlags = StringFormatFlags.LineLimit;
                strFormat.Trimming = StringTrimming.None;

                percentageTextSize = g.MeasureString(percentageText, _font, Width, strFormat);

                percentageTextRect.X = (Width - percentageTextSize.Width) / 2;
                percentageTextRect.Y = 1;
                percentageTextRect.Width = percentageTextSize.Width;
                percentageTextRect.Height = Height - 2;

                strFormat.Alignment = StringAlignment.Center;
                using (SolidBrush objBrushText = new SolidBrush(Color.White))
                {
                    g.DrawString(percentageText, _font, objBrushText, ClientRectangle, strFormat);
                }

                //if (percentageTextRect.Left >= currentRect.Right)
                //{
                //    strFormat.Alignment = StringAlignment.Center;
                //    using (SolidBrush objBrushText = new SolidBrush(Color.FromArgb(40, 40, 40)))
                //    {
                //        g.DrawString(percentageText, GetFont(), objBrushText, ClientRectangle, strFormat);
                //    }
                //}
                //else if (currentRect.Right >= percentageTextRect.Right)
                //{
                //    strFormat.Alignment = StringAlignment.Center;
                //    using (SolidBrush objBrushText = new SolidBrush(Color.White))
                //    {
                //        g.DrawString(percentageText, GetFont(), objBrushText, ClientRectangle, strFormat);
                //    }
                //}
                //else
                //{
                //using (SolidBrush objBrushText = new SolidBrush(Color.FromArgb(40, 40, 40)))
                //{
                //    g.DrawString(percentageText, GetFont(), objBrushText, percentageTextRect, strFormat);
                //}

                //Bitmap bitmapText = new Bitmap((int)percentageTextRect.Width + 1, (int)percentageTextRect.Height);
                //Graphics gTemp = Graphics.FromImage(bitmapText);

                //gTemp.Clear(_themeColor);

                //using (SolidBrush objBrushText = new SolidBrush(Color.White))
                //{
                //    gTemp.DrawString(percentageText, GetFont(), objBrushText, new RectangleF(0, 0, bitmapText.Width, bitmapText.Height), strFormat);
                //}
                //g.DrawImage(bitmapText, new RectangleF(percentageTextRect.Left, percentageTextRect.Top, currentRect.Right - percentageTextRect.Left, Height), new RectangleF(0, 0, currentRect.Right - percentageTextRect.Left, Height), GraphicsUnit.Pixel);

                //gTemp.Dispose();
                //bitmapText.Dispose();
                //}
            }
        }
    }
}

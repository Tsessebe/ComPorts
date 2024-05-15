using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ComPort.Scanner.Watchers.EventArguments;
using ComPort.Scanner.Watchers.Models;

namespace ComPort.Scanner
{
    public partial class FormMain : Form
    {
        private const int GapSize = 2;
        protected const int GapSize2 = GapSize * 2;
        private const int MarginSize = 10;
        private const int MarginSize2 = MarginSize * 2;

        private const string NoDataText = "No Data to Display";

        private readonly Brush backgroundBrush = new SolidBrush(Color.MidnightBlue);

        // private readonly Pen boundaryLinePen = new Pen(Color.Green, 1);
        private readonly object currentSyncObject = new object();

        // private readonly Brush defaultBrush = new SolidBrush(SystemColors.HotTrack);
        // private readonly Brush errBrush = new SolidBrush(Color.Red);
        private readonly Brush headingBrush = new SolidBrush(SystemColors.ControlDark);

        private readonly Pen headingLinePen = new Pen(SystemColors.ControlDark, 2);
        private readonly Pen linePen = new Pen(SystemColors.Control, 1);

        // private readonly Brush okBrush = new SolidBrush(Color.Green);

        private readonly Brush textBrush = new SolidBrush(SystemColors.ControlDark);

        // private readonly Brush whiteBrush = new SolidBrush(Color.White);

        private Bitmap current;

        public FormMain(ICollection<ComPortModel> ports)
        {
            Ports = ports.ToList();

            InitializeComponent();

            picDisplay.BorderStyle = BorderStyle.FixedSingle;

            picDisplay.Location = new Point(0, 0);
            picDisplay.Size = new Size(ClientSize.Width, ClientSize.Height);
        }

        private List<ComPortModel> Ports { get; set; }

        private int HalfHeight { get; set; }
        private int HalfWidth { get; set; }

        public void OnComPortsChanged(object sender, ChangedEventArgs e)
        {
            Ports = e.Ports;
            Redraw();
        }

        private void DrawNoData()
        {
            Bitmap localCopy;
            lock (currentSyncObject)
            {
                if (current != null)
                {
                    current.Dispose();
                    current = null;
                }

                current = new Bitmap(picDisplay.Width, picDisplay.Height);

                using (var g = Graphics.FromImage(current))
                {
                    HalfHeight = current.Height / 2;
                    HalfWidth = current.Width / 2;

                    var background = backgroundBrush;
                    g.FillRectangle(background, 0, 0, current.Width, current.Height);

                    var textSize = g.MeasureString(NoDataText, Font, -1, StringFormat.GenericTypographic);
                    var textHalfHeight = textSize.Height / 2;
                    var textHalfWidth = textSize.Width / 2;

                    var brush = textBrush;
                    var pen = headingLinePen;

                    var p1 = new Point(MarginSize, (int)(HalfHeight - textHalfHeight) - (int)(GapSize + pen.Width));
                    var p2 = new Point(current.Width - MarginSize2,
                        (int)(HalfHeight - textHalfHeight) - (int)(GapSize + pen.Width));
                    g.DrawLine(pen, p1, p2);

                    g.DrawString(NoDataText, Font, brush, HalfWidth - textHalfWidth, HalfHeight - textHalfHeight,
                        StringFormat.GenericTypographic);

                    p1 = new Point(MarginSize, (int)(HalfHeight + textHalfHeight) + (int)(GapSize + pen.Width));
                    p2 = new Point(current.Width - MarginSize2,
                        (int)(HalfHeight + textHalfHeight) + (int)(GapSize + pen.Width));
                    g.DrawLine(pen, p1, p2);
                }

                if (current == null)
                {
                    return;
                }

                localCopy = (Bitmap)current.Clone();
            }

            IDisposable old = picDisplay.Image;
            picDisplay.Image = (Bitmap)localCopy.Clone();
            old?.Dispose();
        }

        private Font GetHeadingFont()
        {
            return new Font(Font.Name, Font.Size + 2, FontStyle.Bold);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            picDisplay.Location = new Point(0, 0);
            picDisplay.Size = new Size(ClientSize.Width, ClientSize.Height);


            if (WindowState == FormWindowState.Minimized)
                Close();
            else
                Redraw();
        }

        private void Redraw()
        {
            if (picDisplay.Width < 10 || picDisplay.Height < 10) return;

            if (Ports.Count == 0)
            {
                DrawNoData();
                return;
            }

            Bitmap localCopy;
            lock (currentSyncObject)
            {
                if (current != null)
                {
                    current.Dispose();
                    current = null;
                }

                current = new Bitmap(picDisplay.Width, picDisplay.Height);

                using (var g = Graphics.FromImage(current))
                {
                    HalfHeight = current.Height / 2;
                    HalfWidth = current.Width / 2;
                    var background = backgroundBrush;
                    g.FillRectangle(background, 0, 0, current.Width, current.Height);

                    var headingFont = GetHeadingFont();
                    var textSize = g.MeasureString("Com Ports", headingFont, -1, StringFormat.GenericTypographic);
                    // var textHalfHeight = textSize.Height / 2;
                    var textHalfWidth = textSize.Width / 2;

                    var brush = textBrush;
                    var pen = headingLinePen;

                    var curX = HalfWidth - textHalfWidth;
                    float curY = MarginSize;

                    g.DrawString("Com Ports", headingFont, headingBrush, curX, curY, StringFormat.GenericTypographic);

                    curY += textSize.Height + (int)(GapSize + pen.Width);
                    var p1 = new Point(MarginSize, (int)curY);
                    var p2 = new Point(current.Width - MarginSize2, (int)curY);
                    g.DrawLine(pen, p1, p2);

                    curX = MarginSize;
                    curY += GapSize + pen.Width;

                    pen = linePen;
                    var colWidth = p2.X - p1.X;
                    foreach (var item in Ports.OrderBy(p => p.No))
                    {
                        var text = item.DeviceName;
                        textSize = g.MeasureString(text, Font, -1, StringFormat.GenericTypographic);
                        if (textSize.Width > colWidth)
                        {
                            var takeCount = 2;
                            var words = text.Split(new[] { " " }, StringSplitOptions.None).ToList();
                            var textTmp = string.Join(" ", words.Take(takeCount));
                            textSize = g.MeasureString(textTmp, Font, -1, StringFormat.GenericTypographic);
                            while (textSize.Width < colWidth)
                            {
                                takeCount++;
                                textTmp = string.Join(" ", words.Take(takeCount));
                                textSize = g.MeasureString(textTmp, Font, -1, StringFormat.GenericTypographic);
                            }

                            takeCount--;
                            textTmp = string.Join(" ", words.Take(takeCount));
                            textSize = g.MeasureString(textTmp, Font, -1, StringFormat.GenericTypographic);
                            g.DrawString(textTmp, Font, brush, curX, curY, StringFormat.GenericTypographic);
                            
                            curY += textSize.Height + (int)(GapSize + pen.Width);
                            textTmp = string.Join(" ", words.Skip(takeCount));
                            textSize = g.MeasureString(textTmp, Font, -1, StringFormat.GenericTypographic);
                            g.DrawString(textTmp, Font, brush, curX, curY, StringFormat.GenericTypographic);
                            
                            curY += textSize.Height + (int)(GapSize + pen.Width);
                        }
                        else
                        {
                            g.DrawString(text, Font, brush, curX, curY, StringFormat.GenericTypographic);
                            curY += textSize.Height + (int)(GapSize + pen.Width);
                        }

                        p1 = new Point(MarginSize, (int)curY);
                        p2 = new Point(current.Width - MarginSize2, (int)curY);
                        g.DrawLine(pen, p1, p2);
                        curY += GapSize + pen.Width;
                    }
                }

                if (current == null)
                {
                    return;
                }

                localCopy = (Bitmap)current.Clone();
            }

            IDisposable old = picDisplay.Image;
            picDisplay.Image = (Bitmap)localCopy.Clone();
            old?.Dispose();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            formTimer.Enabled = true;

            var screen = Screen.PrimaryScreen;

            var x = screen.WorkingArea.Width - Width;
            var y = screen.WorkingArea.Height - Height;
            Location = new Point(x, y);
        }

        private void OnFormTimerTick(object sender, EventArgs e)
        {
            formTimer.Enabled = false;
        }
    }
}
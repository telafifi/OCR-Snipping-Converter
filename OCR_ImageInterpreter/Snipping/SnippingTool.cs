using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCR_ImageInterpreter.Snipping
{
    public partial class SnippingTool : Form
    {
        #region Public Members
        public static event EventHandler Cancel;
        public static EventHandler AreaSelected;
        public static Image Image { get; set; }
        public static bool clipComplete = false;
        #endregion

        #region Private members
        private static SnippingTool[] _snippingForms;
        private Rectangle _rectSelected;
        private Point _pointStart;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiate the snipping tool to allow user to select region for image
        /// </summary>
        /// <param name="screenShot"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public SnippingTool(Image screenShot, int x, int y, int width, int height)
        {
            InitializeComponent();
            BackgroundImage = screenShot;
            BackgroundImageLayout = ImageLayout.Stretch;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            SetBounds(x, y, width, height);
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
            TopMost = true;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Cancel snipping process
        /// </summary>
        /// <param name="e"></param>
        private void OnCancel(EventArgs e)
        {
            clipComplete = false;
            Cancel?.Invoke(this, e);
        }

        /// <summary>
        /// Store area selected
        /// </summary>
        /// <param name="e"></param>
        private void OnAreaSelected(EventArgs e)
        {
            AreaSelected?.Invoke(this, e);
        }

        /// <summary>
        /// Clear data in forms array
        /// </summary>
        private void CloseForms()
        {
            foreach (var form in _snippingForms)
            {
                form.Dispose();
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Create Snipping overlay and prompt user to select area
        /// </summary>
        public static void Snip()
        {
            clipComplete = false;
            var screens = ScreenHelper.GetMonitorsInfo();
            _snippingForms = new SnippingTool[screens.Count]; //create a snipping form for each screen available
            for (int i = 0; i < screens.Count; i++)
            {
                int hRes = screens[i].HorizontalResolution;
                int vRes = screens[i].VerticalResolution;
                int top = screens[i].MonitorArea.Top;
                int left = screens[i].MonitorArea.Left;
                var bmp = new Bitmap(hRes, vRes, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(left, top, 0, 0, bmp.Size);
                }
                _snippingForms[i] = new SnippingTool(bmp, left, top, hRes, vRes);
                _snippingForms[i].Show();
            }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Start the snip on mouse down
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            clipComplete = false;
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            _pointStart = e.Location;
            _rectSelected = new Rectangle(e.Location, new Size(0, 0));
            Invalidate();
        }

        /// <summary>
        /// Change the size of the snipping based on mouse movement
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            int x1 = Math.Min(e.X, _pointStart.X);
            int y1 = Math.Min(e.Y, _pointStart.Y);
            int x2 = Math.Max(e.X, _pointStart.X);
            int y2 = Math.Max(e.Y, _pointStart.Y);
            _rectSelected = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            Invalidate();
        }

        /// <summary>
        /// Complete the snip on mouse up
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            clipComplete = true;
            if (_rectSelected.Width <= 0 || _rectSelected.Height <= 0)
            {
                CloseForms();
                OnCancel(new EventArgs());
                return;
            }
            Image = new Bitmap(_rectSelected.Width, _rectSelected.Height);
            var hScale = BackgroundImage.Width / (double)Width;
            var vScale = BackgroundImage.Height / (double)Height;
            using (Graphics gr = Graphics.FromImage(Image))
            {

                gr.DrawImage(BackgroundImage,
                    new Rectangle(0, 0, Image.Width, Image.Height),
                    new Rectangle((int)(_rectSelected.X * hScale), (int)(_rectSelected.Y * vScale), (int)(_rectSelected.Width * hScale), (int)(_rectSelected.Height * vScale)),
                    GraphicsUnit.Pixel);
            }
            CloseForms();
            OnAreaSelected(new EventArgs());
        }

        /// <summary>
        /// Draw the current selection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                int x1 = _rectSelected.X;
                int x2 = _rectSelected.X + _rectSelected.Width;
                int y1 = _rectSelected.Y;
                int y2 = _rectSelected.Y + _rectSelected.Height;
                e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x2, 0, Width - x2, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, Height - y2));
            }
            using (Pen pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, _rectSelected);
            }
        }

        /// <summary>
        /// Allow cancelation of snip with the Escape key
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Image = null;
                CloseForms();
                OnCancel(new EventArgs());
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}

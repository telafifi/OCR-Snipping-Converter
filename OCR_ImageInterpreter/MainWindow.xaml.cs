using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IronOcr;

namespace OCR_ImageInterpreter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double windowHeight;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += app_Load;
            MouseDown += Window_MouseDown;
        }
        #region GUI Events
        private void app_Load(object sender, EventArgs e) //update with combo box list
        {
            this.windowHeight = 250;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        #endregion
        private async void btnSnip_Click(object sender, RoutedEventArgs e)
        {
            picImage.Visibility = Visibility.Collapsed;
            txImage.Visibility = Visibility.Collapsed;
            Application.Current.MainWindow.Height = windowHeight;
            DoEvents();
            this.WindowState = WindowState.Minimized;
            txInstruct.Text = "Processing Image...";
            txOutput.Text = "";
            ImageProcessing.SnipImage();
            Task<bool> task = new Task<bool>(() => ImageProcessing.IsClipComplete());
            task.Start();
            bool result = await task;
            this.WindowState = WindowState.Normal;
            if (result)
            {
                if (SetImageInGUI())
                {
                    Application.Current.MainWindow.Height = windowHeight + picImage.ActualHeight + txImage.Height;
                    DoEvents();
                }
                string output = OCREngine.ConvertImageToText(ImageProcessing.bmp);
                txOutput.Text = output;
                txInstruct.Text = "Conversion Complete!";
            }
            else
            {
                txInstruct.Text = "Could not process image!";
            }
        }

        /// <summary>
        /// Takes image stored in clipboard and place it in GUI
        /// </summary>
        private bool SetImageInGUI()
        {
            if (Clipboard.ContainsImage())
            {
                System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                    {
                        picImage.Visibility = Visibility.Visible;
                        txImage.Visibility = Visibility.Visible;
                        System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                        picImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        DoEvents();
                        return true;
                    }
                }
            }
            return false;
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
            DispatcherPriority.Background,
            (System.Threading.SendOrPostCallback)delegate (object arg)
            {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
            );
            Dispatcher.PushFrame(frame);
        }
    }
}

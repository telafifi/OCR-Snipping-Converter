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
        /// <summary>
        /// Click close button on UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Click minimize button on UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// Hold mouse down to move application on monitor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        #endregion

        /// <summary>
        /// Run to begin snipping an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSnip_Click(object sender, RoutedEventArgs e)
        {
            //Collapse the image and text boxes while processing and return heigh back to original
            picImage.Visibility = Visibility.Collapsed;
            txImage.Visibility = Visibility.Collapsed;
            Application.Current.MainWindow.Height = windowHeight;
            DoEvents();
            this.WindowState = WindowState.Minimized;
            txInstruct.Text = "Processing Image...";
            txOutput.Text = "";
            //Begin snipping process
            Snipping.ImageProcessing.SnipImage();
            Task<bool> task = new Task<bool>(() => Snipping.ImageProcessing.IsClipComplete()); //check if the snip is complete
            task.Start();
            bool result = await task; //wait for the task to complete
            this.WindowState = WindowState.Normal; //return window back to normal

            //If the snip is complete
            if (result)
            {
                if (SetImageInGUI()) //Add the image to the application window
                {
                    Application.Current.MainWindow.Height = windowHeight + picImage.ActualHeight + txImage.Height; //update height of application
                    DoEvents();
                }
                string output = OCREngine.ConvertImageToText(Snipping.ImageProcessing.bmp); //process image text and convert
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
                System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject(); //get image copied to clipboard
                if (clipboardData != null)
                {
                    if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
                    {
                        picImage.Visibility = Visibility.Visible;
                        txImage.Visibility = Visibility.Visible;
                        System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
                        picImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); //set that image in the application window
                        DoEvents();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Perform stored actions to GUI during run
        /// </summary>
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

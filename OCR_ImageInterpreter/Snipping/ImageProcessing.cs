using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OCR_ImageInterpreter.Snipping
{
    class ImageProcessing
    {
        public static Bitmap bmp;
        /// <summary>
        /// Copy snipped image to clipboard
        /// </summary>
        public static void SnipImage()
        {
            Snipping.SnippingTool.AreaSelected += OnAreaSelected;
            Snipping.SnippingTool.Snip();
        }

        /// <summary>
        /// Check if image clipping is complete and image has been stored
        /// </summary>
        /// <returns></returns>
        public static bool IsClipComplete()
        {
            int count = 0;
            while (!Snipping.SnippingTool.clipComplete)
            {
                Thread.Sleep(500);
                count++;
                if (count > 100) //break loop if snipping takes too long
                {
                    return false;
                }
            }
            return Snipping.SnippingTool.clipComplete;
        }

        /// <summary>
        /// Store image and copy image to clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnAreaSelected(object sender, EventArgs e)
        {
            bmp = (Bitmap)Snipping.SnippingTool.Image;
            System.Windows.Forms.Clipboard.SetImage(bmp); //Copy Image to clipboard
        }
    }
}

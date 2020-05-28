using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using IronOcr;
using System.Windows;

namespace OCR_ImageInterpreter
{
    class OCREngine
    {
        /// <summary>
        /// Create IronOCR object and convert snipped image to text
        /// </summary>
        /// <param name="image">Snipped image to convert to text</param>
        /// <returns></returns>
        public static string ConvertImageToText(Bitmap image)
        {
            var Ocr = new AdvancedOcr()
            {
                CleanBackgroundNoise = true,
                EnhanceContrast = true,
                EnhanceResolution = true,
                Language = IronOcr.Languages.English.OcrLanguagePack,
                Strategy = IronOcr.AdvancedOcr.OcrStrategy.Advanced,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                DetectWhiteTextOnDarkBackgrounds = true,
                InputImageType = AdvancedOcr.InputTypes.Document,
                RotateAndStraighten = true,
                ReadBarCodes = true,
                ColorDepth = 4
            };

            var results = Ocr.Read(image);
            return results.Text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Common;
using ZXing;
using System.Windows.Shapes;
using System.Linq.Expressions;

namespace ShopifyEasyShirtPrinting.Helpers
{
    public class QrHelper
    {
        private List<string> SplitTextForWordWrap(Graphics graphics, string text, Font font, float maxWidth)
        {
            var lines = new List<string>();

            var words = text.Trim().Split(' ');
            var currentLine = string.Empty;

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(currentLine))
                {
                    currentLine = word;
                }
                else
                {
                    string testLine = currentLine + " " + word;
                    SizeF size = graphics.MeasureString(testLine, font);

                    if (size.Width <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                }
            }

            lines.Add(currentLine);

            return lines;
        }

        public Bitmap CombineImage(Bitmap bmp1, Bitmap bmp2)
        {
            var result = new Bitmap(bmp1.Width + bmp2.Width, Math.Max(bmp1.Height, bmp2.Height));

            // Create a graphics object from the result bitmap
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.Clear(Color.White);
                // Draw the first bitmap on the left side of the result bitmap
                graphics.DrawImage(bmp1, 0, 0);
                graphics.DrawImage(bmp2, bmp1.Width, 0);
            }

            return result;
        }

        public Bitmap GenerateBitmapQr(string data)
        {
            var qrMargin = 2;
            var qrWidth = 600;
            var qrHeight = 600;

            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = qrWidth,
                    Height = qrHeight,
                    Margin = qrMargin,
                    PureBarcode = true
                }
            };

            var qrCode = barcodeWriter.Write(data);

            return qrCode;
        }

        public Bitmap DrawQrTagInfo(string text, Bitmap refImage, string orderNumber, bool hasNotes, bool? color, bool? hasBackPrint = false, bool isDtf = false)
        {
            var aspectRatio = Properties.Settings.Default.PaperWidth / Properties.Settings.Default.PaperHeight;
            var width = (int)(aspectRatio * refImage.Height) - refImage.Width;
            var height = refImage.Height;
            using var font = new Font("Microsoft Sans Serif", Properties.Settings.Default.FontSize, FontStyle.Bold);

            // Create a new bitmap with the specified width and height
            var textImage = new Bitmap(width, height);

            // Create a graphics object from the result bitmap
            using var graphics = Graphics.FromImage(textImage);

            graphics.Clear(Color.White);
            // Create a new font and brush for the text

            var brush = Brushes.Black;

            // Measure the size of the text and check if it exceeds the width of the image
            var textSize = graphics.MeasureString(text, font);
            var nextLine = 0f;
            if (textSize.Width > width)
            {
                var lines = SplitTextForWordWrap(graphics, text, font, width);

                // Calculate the total height of the text
                var totalTextHeight = graphics.MeasureString(text, font, width).Height;

                // Draw each line of text with the specified font and brush
                var y = (height - totalTextHeight) / 2; // center vertically

                foreach (string line in lines)
                {
                    // Calculate the x-coordinate to center the text horizontally
                    var x = (width - graphics.MeasureString(line, font).Width) / 2;

                    graphics.DrawString(line, font, brush, new RectangleF(x, y, width, height));
                    y += graphics.MeasureString(line, font).Height;
                    nextLine = y;
                }
            }
            else
            {
                // Calculate the y-coordinate to center the text vertically
                var y = (height - textSize.Height) / 2;
                // Calculate the x-coordinate to center the text horizontally
                var x = (width - textSize.Width) / 2;
                // Draw the entire text on a single line with the specified font and brush
                graphics.DrawString(text, font, brush, new RectangleF(x, y, width, height));
                y += graphics.MeasureString(text, font).Height;
                nextLine = y;
            }

            var asterisk = "";
            if (hasNotes)
            {
                asterisk = "*";
            }
            var tagFont = new Font(new FontFamily("Arial"), Properties.Settings.Default.FontSize + 4, FontStyle.Bold);
            graphics.DrawString($"#{orderNumber} {asterisk}".Trim(), tagFont, brush, new RectangleF(0, 20, width, height));


            var icon_size = 45; // width equals heigh for square image
            var offset = width - icon_size - 20; 
            nextLine += 16;

            if (color.HasValue)
            {
                if (color.Value == false)
                {
                    graphics.FillEllipse(brush, offset, nextLine, icon_size, icon_size);
                    offset = offset - icon_size - 5;
                }
                else
                {
                    graphics.DrawEllipse(new Pen(brush, 2), offset, nextLine, icon_size, icon_size);
                    offset = offset - icon_size - 5;
                }
            }

            if (hasBackPrint.HasValue && hasBackPrint.Value)
            {
                var image = Properties.Resources.equals_icon;
                graphics.DrawImage(image, offset, nextLine, icon_size, icon_size);
                offset = offset - icon_size - 5;
            }

            if (isDtf)
            {
                var image = Properties.Resources.section_sign;
                graphics.DrawImage(image, offset, nextLine, icon_size, icon_size);
                offset = offset - icon_size - 5;
            }

            return textImage;
        }
    }
}

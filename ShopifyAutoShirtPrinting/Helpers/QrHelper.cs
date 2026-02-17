using System;
using System.Collections.Generic;
using System.Drawing;
using ZXing;
using ZXing.Common;

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
                graphics.Clear(System.Drawing.Color.White);
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


        public Bitmap DrawQrTagInfo(
    string text,
    Bitmap refImage,
    string orderNumber,
    bool hasNotes,
    string sku,
    bool? hasBackPrint)
        {
            var paperW = Properties.Settings.Default.PaperWidth;
            var paperH = Properties.Settings.Default.PaperHeight;
            var aspectRatio = paperW / paperH;

            int width = (int)(aspectRatio * refImage.Height) - refImage.Width;
            int height = refImage.Height;

            var result = new Bitmap(width, height);

            using var graphics = Graphics.FromImage(result);
            using var mainFont = new Font("Microsoft Sans Serif", Properties.Settings.Default.FontSize, FontStyle.Bold);
            using var tagFont = new Font("Arial", Properties.Settings.Default.FontSize + 4, FontStyle.Bold);
            using var pen = new Pen(Brushes.Black, 2);

            var brush = Brushes.Black;

            graphics.Clear(Color.White);

            float nextLineY;

            // ---------- TEXT DRAWING ----------
            var textSize = graphics.MeasureString(text, mainFont);

            if (textSize.Width > width)
            {
                var lines = SplitTextForWordWrap(graphics, text, mainFont, width);
                float totalHeight = graphics.MeasureString(text, mainFont, width).Height;
                float y = (height - totalHeight) / 2;

                foreach (var line in lines)
                {
                    var lineSize = graphics.MeasureString(line, mainFont);
                    float x = (width - lineSize.Width) / 2;
                    graphics.DrawString(line, mainFont, brush, x, y);
                    y += lineSize.Height;
                }

                nextLineY = y;
            }
            else
            {
                float x = (width - textSize.Width) / 2;
                float y = (height - textSize.Height) / 2;

                graphics.DrawString(text, mainFont, brush, x, y);
                nextLineY = y + textSize.Height;
            }

            // ---------- ORDER TAG ----------
            string asterisk = hasNotes ? "*" : "";
            string tagText = $"#{orderNumber} {asterisk}".Trim();
            graphics.DrawString(tagText, tagFont, brush, new RectangleF(0, 20, width, height));

            // ---------- ICONS ----------
            const int ICON_SIZE = 45;
            const int PADDING = 5;
            int offsetX = width - ICON_SIZE - 20;
            nextLineY += 16;

            void DrawIcon(Image img)
            {
                graphics.DrawImage(img, offsetX, nextLineY, ICON_SIZE, ICON_SIZE);
                offsetX -= ICON_SIZE + PADDING;
            }

            void DrawCircle(bool filled)
            {
                if (filled)
                    graphics.FillEllipse(brush, offsetX, nextLineY, ICON_SIZE, ICON_SIZE);
                else
                    graphics.DrawEllipse(pen, offsetX, nextLineY, ICON_SIZE, ICON_SIZE);

                offsetX -= ICON_SIZE + PADDING;
            }

            // Back print icon
            if (hasBackPrint == true)
                DrawIcon(Properties.Resources.equals_icon);

            if (string.IsNullOrWhiteSpace(sku))
                return result;

            // SKU icons
            if (sku.EndsWith("-LT")) DrawCircle(false);
            else if (sku.EndsWith("-BL")) DrawCircle(true);
            else if (sku.EndsWith("-DK")) DrawIcon(Properties.Resources.half_circle);
            else if (sku.EndsWith("-DTF")) DrawIcon(Properties.Resources.X);
            else if (sku.EndsWith("-RDY")) DrawIcon(Properties.Resources.R);

            return result;
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

            graphics.Clear(System.Drawing.Color.White);
            // Create a new font and brush for the text

            var brush = System.Drawing.Brushes.Black;

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
            var tagFont = new Font(new System.Drawing.FontFamily("Arial"), Properties.Settings.Default.FontSize + 4, FontStyle.Bold);
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
                    graphics.DrawEllipse(new System.Drawing.Pen(brush, 2), offset, nextLine, icon_size, icon_size);
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

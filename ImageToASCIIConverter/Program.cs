using System;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageToASCIIConverter
{

    // Recommended font for ASII image is Courier / Courier New and font size 8 
    class Program
    {
        private static readonly char[] _chars = { '#', '+',  ' ' };
        private static readonly int _maxSize = 75;
        static void Main(string[] args)
        {
            try
            {
                StringBuilder ASCIIImage;

                using (Bitmap sourceImage = new Bitmap(Image.FromFile(args[0])))
                {
                    int resize = Math.Max(sourceImage.Width, sourceImage.Height) / _maxSize;
                    Bitmap resizedImage;

                    if (resize != 0)
                        resizedImage = ResizeImage(sourceImage, sourceImage.Width / resize, sourceImage.Height / resize);
                    else
                        resizedImage = sourceImage;

                    using (resizedImage)
                    {
                        ASCIIImage = ConvertToASCII(resizedImage);
                    }
                }

                using (StreamWriter writer = new StreamWriter(args[1]))
                {
                    writer.Write(ASCIIImage.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.GetType().FullName);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Convert image to ASCII
        /// </summary>
        /// <param name="image">image to convert</param>
        /// <param name="stringBuilder">output StringBilder containing ASCII image</param>
        public static StringBuilder ConvertToASCII(Bitmap image)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int height = 0; height < image.Height - 1; height++)
            {
                for (int width = 0; width < image.Width; width++)
                {
                    Color pixelColor1 = image.GetPixel(width, height);
                    Color pixelColor2 = image.GetPixel(width, height + 1);
                    int grayscale = (pixelColor1.B + pixelColor1.R + pixelColor1.G +
                        pixelColor2.B + pixelColor2.R + pixelColor2.G) / 6;

                    if (grayscale < 85)
                    {
                        stringBuilder.Append(_chars[0]);
                    }
                    else
                    {
                        if (grayscale < 170)
                        {
                            stringBuilder.Append(_chars[1]);
                        }
                        else
                        {
                            stringBuilder.Append(_chars[2]);
                        }
                    }
                }
                height++;
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder;
        }

        /// <summary>
        /// High quality resize image
        /// </summary>
        /// <param name="image">source image</param>
        /// <param name="width">target image width</param>
        /// <param name="height">target image height</param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}

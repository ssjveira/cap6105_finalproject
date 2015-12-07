using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Matrix = System.Windows.Media.Matrix;

namespace NutritionalInfoApp.Utils
{
    // Class that provides utility methods for converting ink strokes to nxn bitmap images.
    // Created with help from here: http://thedatafarm.com/tablet/converting-silverlight-inkpresenter-images-to-a-png-file/
    public static class InkConversionUtils
    {
        public enum ImageType
        {
            Gif,
            Png,
            Jpg
        }

        private static void ScaleStrokes(StrokeCollection strokes, double imageSideLength)
        {
            // Get stroke bounds
            var bounds = strokes.GetBounds();

            // Perform scale transform to fit inside image whose size is specified by the imageSideLength parameter
            if (bounds.Width > bounds.Height)
            {
                var scale = 0.0;

                // Scale the ink stroke bounds up or down
                if (bounds.Width > imageSideLength)
                    scale = bounds.Width / imageSideLength;
                else if (bounds.Width < imageSideLength)
                    scale = imageSideLength / bounds.Width;

                var matrix = new Matrix();
                matrix.Scale(scale, scale);

                strokes.Transform(matrix, false);
            }
            else if (bounds.Height > bounds.Width && bounds.Height > imageSideLength)
            {
                var scale = 0.0;

                // Scale the ink stroke bounds up or down
                if (bounds.Height > imageSideLength)
                    scale = bounds.Height / imageSideLength;
                else if (bounds.Height < imageSideLength)
                    scale = imageSideLength / bounds.Height;

                var matrix = new Matrix();
                matrix.Scale(scale, scale);

                strokes.Transform(matrix, false);
            }
        }

        /// <summary>
        /// Translate a stroke so that it's centered in the part of the ink canvas
        /// whose coordinates range from (0,0) to (imageSideLength,imageSideLength)
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        private static void TranslateStrokes(StrokeCollection strokes, double imageSideLength)
        {
            var bounds = strokes.GetBounds();

            // Perform translate transform to center stroke in the center of image
            var translateX = imageSideLength / 2.0 - bounds.Left - bounds.Width / 2.0;
            var translateY = imageSideLength / 2.0 - bounds.Top - bounds.Height / 2.0;

            var translateMatrix = new Matrix();
            translateMatrix.Translate(translateX, translateY);
            strokes.Transform(translateMatrix, false);
        }

        /// <summary>
        /// Scales and translates a collection of strokes so that it can fit 
        /// inside of an nxn image whose sides are of length "imageSideLength" 
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        /// <returns></returns>
        private static void TransformStrokes(StrokeCollection strokes, double imageSideLength)
        {
            ScaleStrokes(strokes, imageSideLength);
            TranslateStrokes(strokes, imageSideLength);
        }

        /// <summary>
        /// Converts a StrokeCollection to a GDI+ bitmap.
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        /// <param name="imageType"></param>
        /// <returns></returns>
        public static Bitmap StrokesToBitmap(StrokeCollection strokes, double imageSideLength, ImageType imageType = ImageType.Png)
        {
            Bitmap bitmap = null;

            // Create a temporary InkCanvas
            var inkCanvas = new InkCanvas {Strokes = strokes.Clone()};

            var maxSideLength = imageSideLength*0.8;
            ScaleStrokes(inkCanvas.Strokes, maxSideLength);
            TranslateStrokes(inkCanvas.Strokes, imageSideLength);

            // TODO: Provide a way to make PixelFormat customizable
            var renderTargetBitmap = new RenderTargetBitmap((int)imageSideLength, (int)imageSideLength, 96.0, 96.0, PixelFormats.Default);

            // Render white background to target bitmap
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(new System.Windows.Point(), new System.Windows.Size(imageSideLength, imageSideLength)));
            }
            renderTargetBitmap.Render(drawingVisual);

            // Render InkCanvas to target bitmap
            renderTargetBitmap.Render(inkCanvas);

            // Save image to memory
            BitmapEncoder encoder;
            switch (imageType)
            {
                case ImageType.Gif:
                    encoder = new GifBitmapEncoder();
                    break;
                case ImageType.Png:
                    encoder = new PngBitmapEncoder();
                    break;
                case ImageType.Jpg:
                    encoder = new JpegBitmapEncoder();
                    break;
                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }

            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }

            return bitmap;
        }

        // TODO: Create another method that can save the Png file to a stream
        // TODO: Launch this in another thread -> http://geoffwebbercross.blogspot.com/2013/06/windows-81-rendertargetbitmap.html
        /// <summary>
        /// Saves the collection of ink strokes as a Png bitmap.
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        /// <param name="filename"></param>
        /// <exception cref="ArgumentException">Thrown when the file extension is not one of the supported types (.gif, .png, .jpg)</exception>
        public static void SaveStrokesToImageFile(StrokeCollection strokes, double imageSideLength, string filename)
        {
            // Get the file extension
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension != null) 
                fileExtension = fileExtension.ToLower();

            // Save image to memory
            Bitmap bitmap;
            switch (fileExtension)
            {
                case ".gif":
                    bitmap = StrokesToBitmap(strokes, imageSideLength, ImageType.Gif);
                    break;
                case ".png":
                    bitmap = StrokesToBitmap(strokes, imageSideLength, ImageType.Png);
                    break;
                case ".jpg":
                    bitmap = StrokesToBitmap(strokes, imageSideLength, ImageType.Jpg);
                    break;
                default:
                    bitmap = StrokesToBitmap(strokes, imageSideLength);
                    break;
            }

            bitmap.Save(filename);
        }
    }
}

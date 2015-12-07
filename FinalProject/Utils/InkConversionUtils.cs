using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NutritionalInfoApp.Utils
{
    // Class that provides utility methods for converting ink strokes to nxn bitmap images.
    // Created with help from here: http://thedatafarm.com/tablet/converting-silverlight-inkpresenter-images-to-a-png-file/
    public static class InkConversionUtils
    {
        /// <summary>
        /// Scales and translates a collection of strokes so that it can fit 
        /// inside of an nxn image whose sides are of length "imageSideLength" 
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        /// <returns></returns>
        private static Rect TransformStrokes(StrokeCollection strokes, double imageSideLength)
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

            var scaledBounds = strokes.GetBounds();

            // Perform translate transform to center stroke in the center of image
            var translateX = imageSideLength / 2.0 - scaledBounds.Left - scaledBounds.Width / 2.0;
            var translateY = imageSideLength / 2.0 - scaledBounds.Top - scaledBounds.Height / 2.0;

            var translateMatrix = new Matrix();
            translateMatrix.Translate(translateX, translateY);
            strokes.Transform(translateMatrix, false);

            // Return bounds of the strokes after they've been scaled and translated
            return strokes.GetBounds();
        }

        public static BitmapSource StrokesToBitmap(StrokeCollection strokes, double imageSideLength)
        {
            // Create a temporary InkCanvas
            var inkCanvas = new InkCanvas {Strokes = strokes.Clone()};

            var bounds = TransformStrokes(inkCanvas.Strokes, imageSideLength);

            // TODO: Provide a way to make PixelFormat customizable
            var renderTargetBitmap = new RenderTargetBitmap((int)imageSideLength,
                (int)imageSideLength, 96.0, 96.0, PixelFormats.Default);

            // Render white background to target bitmap
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(new Point(), new Size(bounds.Width, bounds.Height)));
            }
            renderTargetBitmap.Render(drawingVisual);

            // Render InkCanvas to target bitmap
            renderTargetBitmap.Render(inkCanvas);

            return renderTargetBitmap;
        }

        // TODO: Create another method that can save the PNG file to a stream
        // TODO: Launch this in another thread?
        /// <summary>
        /// Saves the collection of ink strokes as a PNG bitmap.
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="imageSideLength"></param>
        /// <param name="filename"></param>
        /// <exception cref="ArgumentException">Thrown when the file extension is not one of the supported types (.gif, .png, .jpg)</exception>
        public static void SaveStrokesAsImage(StrokeCollection strokes, double imageSideLength, string filename)
        {
            // Get the file extension
            var fileExtension = Path.GetExtension(filename);
            if (fileExtension != null) 
                fileExtension = fileExtension.ToLower();

            // Save image to memory
            BitmapEncoder encoder;
            switch (fileExtension)
            {
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                default:
                    Console.WriteLine(@"Failed to save strokes as image because the file extension wasn't .gif, .png, or .jpg.");
                    return;
            }

            encoder.Frames.Add(BitmapFrame.Create(StrokesToBitmap(strokes, imageSideLength)));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Close();

                // Save stroke to png file
                File.WriteAllBytes(filename, stream.ToArray());
            }
        }
    }
}

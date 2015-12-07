using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NutritionalInfoApp.Utils
{
    // Class that provides utility methods for converting ink strokes to bitmap images.
    // Created with help from here: http://thedatafarm.com/tablet/converting-silverlight-inkpresenter-images-to-a-png-file/
    public static class InkConversionUtils
    {
        public static BitmapSource StrokesToBitmap(StrokeCollection strokes)
        {
            // Create a temporary InkCanvas
            var inkCanvas = new InkCanvas {Strokes = strokes};

            // Get stroke bounds
            var bounds = inkCanvas.Strokes.GetBounds();

            // TODO: Provide a way to make PixelFormat customizable
            var renderTargetBitmap = new RenderTargetBitmap((int) bounds.Right,
                (int) bounds.Bottom, 96.0, 96.0, PixelFormats.Default);

            // Render white background to target bitmap
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(new Point(), new Size(bounds.Right, bounds.Bottom)));
            }
            renderTargetBitmap.Render(drawingVisual);

            // Render InkCanvas to target bitmap
            renderTargetBitmap.Render(inkCanvas);

            return renderTargetBitmap;
        }

        // TODO: Create another method that can save the PNG file to a stream
        /// <summary>
        /// Saves the collection of ink strokes as a PNG bitmap.
        /// </summary>
        /// <param name="strokes"></param>
        /// <param name="filename"></param>
        /// <exception cref="ArgumentException">Thrown when the file extension is not one of the supported types (.gif, .png, .jpg)</exception>
        public static void SaveStrokesAsImage(StrokeCollection strokes, string filename)
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

            encoder.Frames.Add(BitmapFrame.Create(StrokesToBitmap(strokes)));

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

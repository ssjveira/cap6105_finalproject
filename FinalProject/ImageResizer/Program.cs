using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageResizer.Properties;
using ImageResizer.Utils;

namespace ImageResizer
{
    /// <summary>
    /// Main class created to resize sample images from our food sketch image database.
    /// </summary>
    class Program
    {
        private const int KSideLength = 256;

        private static void Main(string[] args)
        {
            var filePaths = Directory.GetFiles(Settings.Default.FoodSamplePath, "*.png", SearchOption.AllDirectories);

            Console.WriteLine("Resizing {0} files to {1} x {1} px", filePaths.Length, KSideLength);

            for(var i = 0; i < filePaths.Length; ++i)
            {
                var bitmap = new Bitmap(filePaths[i]);
                var resizedBitmap = ImageUtils.ResizeImage(bitmap, new Size(KSideLength, KSideLength));

                bitmap.Dispose();

                resizedBitmap.Save(filePaths[i]);
                resizedBitmap.Dispose();

                Console.WriteLine("Resized image {0} of {1}", i + 1, filePaths.Length);
            }
        }
    }
}

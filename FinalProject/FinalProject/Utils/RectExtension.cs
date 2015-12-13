using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NutritionalInfoApp.Utils
{
    public static class RectExtension
    {
        /// <summary>
        /// Returns the center point of the rect
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Point Center(this Rect r)
        {
            return new Point((r.Left + r.Right) / 2, (r.Top + r.Bottom / 2));
        }
    }
}

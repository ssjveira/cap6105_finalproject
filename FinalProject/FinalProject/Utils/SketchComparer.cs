using System.Collections.Generic;
using System.Windows.Ink;

namespace NutritionalInfoApp.Utils
{
    public class SketchComparer : IComparer<StrokeCollection>
    {
        public int Compare(StrokeCollection x, StrokeCollection y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the bounding box x values of the two sketches (stroke collections)
                    var boundsX = x.GetBounds();
                    var boundsY = y.GetBounds();

                    return boundsX.X.CompareTo(boundsY.X);
                }
            }
        }
    }
}

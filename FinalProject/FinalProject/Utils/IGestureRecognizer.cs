using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Shapes;

namespace NutritionalInfoApp.Utils
{
    /// <summary>
    /// Enumeration representing gestures that are recognized by classes implementing MiniJournal's IGestureRecognizer interface.
    /// </summary>
    public enum GestureType
    {
        Square,
        Rectangle,
        Triangle,
        Ellipse,
        Circle,
        Arrow,
        Erase,
        None
    }

    public interface IGestureRecognizer
    {
        /// <summary>
        /// Sets the stroke that will be used by the recognizer to find a gesture that best matches the stroke.
        /// </summary>
        /// <param name="stroke"></param>
        void SetStroke(Stroke stroke);

        /// <summary>
        /// Gets the gesture type that the recognizer says is the best match
        /// </summary>
        /// <returns></returns>
        GestureType GetRecognizedGesture();

        /// <summary>
        /// Gets the shape of the gesture that the recognizer says is the best match
        /// </summary>
        /// <returns></returns>
        Shape GetRecognizedGestureShape();

        /// <summary>
        /// Runs the implemented recognition algorithm
        /// </summary>
        void Recognize();
    }
}

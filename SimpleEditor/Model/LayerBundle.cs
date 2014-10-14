using System;
using System.Drawing;

namespace SimpleEditor.Model
{
    /// <summary>
    /// Layer bundle. Short data from Layer object to serializing.
    /// </summary>
    [Serializable]
    public class LayerBundle
    {
        public Bitmap Bitmap { get; set; }
        public bool IsVisible { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
    }
}

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace SimpleEditor.Extentions
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Converts System.Drawing.Bitmap to BitmapSource
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>WPF BitmapSource</returns>
        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                var result = new BitmapImage {CreateOptions = BitmapCreateOptions.PreservePixelFormat};
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        /// <summary>
        /// Normilize point relative to the point
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="origin">Origin point</param>
        /// <returns>Normalized point</returns>
        public static Point Normalize(this Point point, Point origin)
        {
            var normalized = new Point(
                point.X - origin.X,
                point.Y - origin.Y);
            return normalized;
        }
    }
}
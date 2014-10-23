using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEditor.Model;

namespace SimpleEditor.Tests.Model
{
    [TestClass]
    public class LayerTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var layer = new Layer();
        }

        [TestMethod]
        public void ConstructorWithBundleTest()
        {
            try
            {
                var nullBundleLayer = new Layer(null);
                Assert.Fail("Layer initiated with null bundle");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual(e.ParamName,"layerBundle");
            }

            var layerBundle = new LayerBundle()
            {
                Bitmap = new Bitmap(1, 1),
                IsVisible = true,
                Position = new Point(),
                Size = new Size(1, 1)
            };

            var layer = new Layer(layerBundle);

            Assert.AreEqual(layerBundle.Bitmap,layer.Bitmap);
            Assert.AreEqual(layerBundle.Size, layer.Size);
            Assert.AreEqual(layerBundle.Position, layer.Position);
            Assert.AreEqual(layerBundle.Size, layer.Size);
        }

        [TestMethod]
        public void OffsetTest()
        {
            var layer = new Layer();
            layer.Offset(5,5);
            Assert.AreEqual(new Point(5,5),layer.Position);
        }

        [TestMethod]
        public void DrawLinesTest()
        {
            var layer = new Layer();
            var pen = new Pen(Color.Blue,2);
            var points = new[] {new Point(0, 0), new Point(4, 4)};

            try
            {
                layer.DrawLines(null,points);
                Assert.Fail("Drawing with null pen");
            }
            catch (ArgumentNullException e)
            {
                Assert.AreEqual(e.ParamName, "pen");
            }

            layer.ChangeSizeAndPosition(new Point(0,0),new Size(5,5) );
            layer.DrawLines(pen,points);
            Assert.AreEqual(layer.BufferBitmap.GetPixel(2, 2).ToArgb(),Color.Blue.ToArgb());
            Assert.AreEqual(layer.BufferBitmap.GetPixel(3, 3).ToArgb(), Color.Blue.ToArgb());
        }
    }
}

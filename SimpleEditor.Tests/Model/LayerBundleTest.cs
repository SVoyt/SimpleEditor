using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEditor.Model;

namespace SimpleEditor.Tests.Model
{
    [TestClass]
    public class LayerBundleTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var layerBundle = new LayerBundle()
            {
                Bitmap = new Bitmap(1, 1),
                IsVisible = true,
                Position = new Point(),
                Size = new Size(1, 1)
            };
        }
    }
}

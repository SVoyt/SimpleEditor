using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEditor.Extentions;

namespace SimpleEditor.Tests.Extentions
{
    [TestClass]
    public class DrawingExtentionsTest
    {
        [TestMethod]
        public void NormalizeTest()
        {
            var p = new Point();
            var normalized = p.Normalize(new Point(1, 1));
            Assert.AreEqual(normalized,new Point(-1,-1));
        }
    }
}

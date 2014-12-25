using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEditor.Model;

namespace SimpleEditor.Tests.Model
{
    [TestClass]
    public class SceneTest
    {

        [TestMethod]
        public void EmptySceneTest()
        {
            using (var scene = new Scene())
            {
                Assert.IsFalse(scene.CanMoveSelectedLayerDown());
                Assert.IsFalse(scene.CanMoveSelectedLayerUp());
                Assert.IsFalse(scene.CanRemoveSelectedLayer());
                Assert.IsTrue(scene.HasNoLayers);
            }
        }

        [TestMethod]
        public void AddLayerTest()
        {
            using (var scene = new Scene())
            {
                scene.AddNewLayer();
                Assert.IsTrue(scene.CanRemoveSelectedLayer());
                Assert.IsFalse(scene.CanMoveSelectedLayerDown());
                Assert.IsFalse(scene.CanMoveSelectedLayerUp());
                Assert.IsFalse(scene.HasNoLayers);
                Assert.AreEqual(scene.SelectedLayerIndex, 0);
            }
        }

        [TestMethod]
        public void AddSecondLayerTest()
        {
            using (var scene = new Scene())
            {
                scene.AddNewLayer();
                scene.AddNewLayer();
                Assert.IsTrue(scene.CanRemoveSelectedLayer());
                Assert.IsTrue(scene.CanMoveSelectedLayerDown());
                Assert.IsFalse(scene.CanMoveSelectedLayerUp());
                Assert.IsFalse(scene.HasNoLayers);
                Assert.AreEqual(scene.SelectedLayerIndex, 0);
            }
        }

        [TestMethod]
        public void RemoveTest()
        {
            using (var scene = new Scene())
            {
                scene.AddNewLayer();
                scene.AddNewLayer();
                Assert.IsTrue(scene.CanRemoveSelectedLayer());
                scene.RemoveSelectedLayer();
                Assert.AreEqual(scene.SelectedLayerIndex,0);
                scene.RemoveSelectedLayer();
                Assert.AreEqual(scene.SelectedLayerIndex, -1);
                Assert.IsTrue(scene.HasNoLayers);
            }
        }

    }
}

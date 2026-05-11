using FileSystemViewer.Models;
using FileSystemViewer.Models.Tools;
using FileSystemViewer.UnitTests.Helpers;
using System.Collections.ObjectModel;
using ModernControls.Models;

namespace FileSystemViewer.UnitTests.Models.Tools
{
    public class TreemapBuilderHelperTests
    {
        [Fact]
        public void Build_FilterLowPercent_ProperCollectionGot()
        {
            // Arrange
            DirectoryNode rootNode = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null, 100000);
            DirectoryNode subDirectory = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(rootNode, 99000);
            FileNode subFileNode = FakeFileSystemNodeHelper.GetFileNodeForTest(rootNode, 5);
            rootNode.FileSystemNodes!.Add(subDirectory);
            rootNode.FileSystemNodes!.Add(subFileNode);
            List<DirectoryNode> rootNodes = new List<DirectoryNode>() { rootNode };

            // Act
            IEnumerable<TreemapNode> treemapNodes = TreemapBuilderHelper.Build(rootNodes);

            // Assert
            var rootTreemapNode = treemapNodes.First();
            Assert.Single(rootTreemapNode.Children);
        }

        [Fact]
        public void Build_Max200Nodes_ProperCollectionGot()
        {
            // Arrange
            DirectoryNode rootNode = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null);
            List<DirectoryNode> directoryNodes = GenerateNodesList(205);
            rootNode.FileSystemNodes = new ObservableCollection<FileSystemNode>(directoryNodes);

            List<DirectoryNode> rootNodes = new List<DirectoryNode>() { rootNode };

            // Act
            IEnumerable<TreemapNode> treemapNodes = TreemapBuilderHelper.Build(rootNodes);

            // Assert
            var rootTreemapNode = treemapNodes.First();
            Assert.Equal(200, rootTreemapNode.Children.Count);
        }

        [Fact]
        public void Build_FileExtensionGrouping_ProperCollectionGot()
        {
            // Arrange
            DirectoryNode rootNode = FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null, 270);
            FileNode fileNode1 = FakeFileSystemNodeHelper.GetFileNodeForTest(rootNode, 100, ".png");
            FileNode fileNode2 = FakeFileSystemNodeHelper.GetFileNodeForTest(rootNode, 50, ".png");
            FileNode fileNode3 = FakeFileSystemNodeHelper.GetFileNodeForTest(rootNode, 20, ".jpg");
            FileNode fileNode4 = FakeFileSystemNodeHelper.GetFileNodeForTest(rootNode, 100);
            rootNode.FileSystemNodes!.Add(fileNode1);
            rootNode.FileSystemNodes!.Add(fileNode2);
            rootNode.FileSystemNodes!.Add(fileNode3);
            rootNode.FileSystemNodes!.Add(fileNode4);

            List<DirectoryNode> rootNodes = new List<DirectoryNode>() { rootNode };

            // Act
            IEnumerable<TreemapNode> treemapNodes = TreemapBuilderHelper.Build(rootNodes);

            // Assert
            Assert.Single(treemapNodes);

            var rootTreemapNode = treemapNodes.First();
            Assert.Equal(3, rootTreemapNode.Children.Count); //  has 3 extension groups: .png, .jpg and No extension

            var pngGroup = rootTreemapNode.Children.FirstOrDefault(c => c.LabeledName == ".png");
            Assert.NotNull(pngGroup);
            Assert.Equal(150, pngGroup.Size);

            var noExtentionGroup = rootTreemapNode.Children.FirstOrDefault(c => c.LabeledName == "No extension");
            Assert.NotNull(noExtentionGroup);
            Assert.Equal(100, noExtentionGroup.Size);
        }

        private static List<DirectoryNode> GenerateNodesList(int count)
        {
            var nodes = new List<DirectoryNode>(count);

            for (int i = 0; i < count; i++)
            {
                nodes.Add(FakeFileSystemNodeHelper.GetDirectoryNodeForTest(null));
            }

            return nodes;
        }
    }
}

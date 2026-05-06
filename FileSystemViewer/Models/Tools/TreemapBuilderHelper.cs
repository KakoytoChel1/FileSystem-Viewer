using ModernControls.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileSystemViewer.Models.Tools
{
    public static class TreemapBuilderHelper
    {
        public static IEnumerable<TreemapNode> Build(IEnumerable<DirectoryNode> rootNodes)
        {
            var newTreemanCollection = new List<TreemapNode>();
            foreach (var rootNode in rootNodes)
            {
                var treemapLayer = CreateTreemapNodeFromDirectoryNode(rootNode);

                if (treemapLayer == null)
                    continue;

                newTreemanCollection.Add(treemapLayer);
            }
            return newTreemanCollection;
        }

        private static TreemapNode? CreateTreemapNodeFromDirectoryNode(DirectoryNode directoryNode)
        {
            const int maxSubdirectories = 200;
            const double minPercent = 0.01;

            var treemapNode = new TreemapNode
            {
                LabeledName = directoryNode.Name,
                IsContainer = true,
                Size = directoryNode.Size,
                BackgroundColor = ColorManager.DirectoryTreemapNodeColor,
                Percent = 0,
                Children = new ObservableCollection<TreemapNode>(),
                FullPath = directoryNode.FullPath
            };

            var subDirectories = directoryNode.FileSystemNodes!
                .OfType<DirectoryNode>();

            var orderedDirectories = subDirectories
                .Where(d => d.PercentProperty >= minPercent)
                .OrderByDescending(d => d.PercentProperty)
                .Take(maxSubdirectories)
                .ToList();

            var extensionGroups = directoryNode.FileSystemNodes!
                .OfType<FileNode>()
                .GroupBy(f => f.Extension);

            var sortedFilesExtensionGroups = extensionGroups
                .Where(g => g.Sum(i => i.PercentProperty) >= minPercent)
                .ToList();

            // File extension groups
            foreach (var extensionGroup in sortedFilesExtensionGroups)
            {
                string extension = extensionGroup.Key;
                long totalSizeForExtension = extensionGroup.Sum(f => f.Size);
                double totalPercent = extensionGroup.Sum(f => f.PercentProperty);

                var fileExtensionNode = new TreemapNode
                {
                    LabeledName = string.IsNullOrEmpty(extension) ? "No extension" : extension,
                    IsContainer = false,
                    Size = totalSizeForExtension,
                    BackgroundColor = ColorManager.GetColorByExtension(extension),
                    Percent = totalPercent,
                    Parent = treemapNode,
                    Children = null
                };

                treemapNode.Children.Add(fileExtensionNode);
            }

            // Directories
            foreach (var subDirectory in orderedDirectories)
            {
                var childTreemapNode = CreateTreemapNodeFromDirectoryNode(subDirectory);

                if (childTreemapNode == null)
                    continue;

                childTreemapNode.Parent = treemapNode;
                treemapNode.Children.Add(childTreemapNode);
            }

            // Calculate percentages for this node's children
            if (treemapNode.Children.Any())
            {
                long totalChildSize = treemapNode.Children.Sum(c => c.Size);
                if (totalChildSize > 0)
                {
                    foreach (var child in treemapNode.Children)
                    {
                        child.Percent = (double)child.Size / totalChildSize * 100;
                    }
                }
            }

            return treemapNode;
        }
    }
}

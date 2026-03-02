using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FileSystem_Viewer.Models;

namespace FileSystem_Viewer.Views.TSelectors
{
    public class ExplorerItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = null!;

        public DataTemplate FileTemplate { get; set; } = null!;


        protected override DataTemplate SelectTemplateCore(object item)
        {
            var fileSystemNode = (FileSystemNode)item;

            return fileSystemNode.NodeType == FileSystemNode.NodeTypes.Directory
                ? DirectoryTemplate
                : FileTemplate;
        }
    }
}

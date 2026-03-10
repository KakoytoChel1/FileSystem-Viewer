using FileSystem_Viewer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FileSystem_Viewer.Views
{
    public class ExplorerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DriveTemplate { get; set; } = null!;
        public DataTemplate DirectoryTemplate { get; set; } = null!;

        public DataTemplate FileTemplate { get; set; } = null!;
        
        protected override DataTemplate? SelectTemplateCore(object item)
        {
            if (item is TreeViewNode node)
            {
                if (node.Content is DriveNode)
                    return DriveTemplate;
                else if (node.Content is DirectoryNode)
                    return DirectoryTemplate;
                else if (node.Content is FileNode)
                    return FileTemplate;
            }
            return null;
        }
    }   
}

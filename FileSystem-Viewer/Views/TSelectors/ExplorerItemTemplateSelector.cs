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
            if(item is DirectoryNode directoryNode)
            {
                return DirectoryTemplate;
            }
            else if(item is FileNode fileNode)
            {
                return FileTemplate;
            }
            else
            {
                return base.SelectTemplateCore(item);
            }
        }
    }
}

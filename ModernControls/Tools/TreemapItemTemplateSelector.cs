using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ModernControls.Models;

namespace ModernControls.Tools
{
    public class TreemapItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ContainerTemplate { get; set; }
        public DataTemplate RegularTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TreemapNode node)
            {
                if (node.IsContainer)
                    return ContainerTemplate;
                else
                    return RegularTemplate;
            }
            return base.SelectTemplateCore(item);
        }
    }
}

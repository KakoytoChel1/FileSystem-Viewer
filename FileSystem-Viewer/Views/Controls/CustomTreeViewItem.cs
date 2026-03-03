using FileSystem_Viewer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_Viewer.Views.Controls
{
    public class CustomTreeViewItem : TreeViewItem
    {
        public CustomTreeViewItem()
        {
            // Подписываемся на момент, когда виртуализация подменяет нам данные
            this.DataContextChanged += OnDataContextChanged;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Метод вызвался 1 раз. Шаблон применен.
            // Здесь мы могли бы найти части шаблона через GetTemplateChild, если бы писали свой ControlTemplate.

            // Принудительно обновляем вид при первом создании
            UpdateVisualState(this.DataContext as FileSystemNode);
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            // Виртуализация подсунула новые данные! Реагируем.
            UpdateVisualState(args.NewValue as FileSystemNode);
        }

        private void UpdateVisualState(FileSystemNode? node)
        {
            if (node == null) return;

            // Вручную управляем логикой наличия дочерних элементов
            if (node is DirectoryNode dirNode)
            {
                // Говорим контролу: "У этого узла МОГУТ БЫТЬ дети, покажи стрелочку"
                this.HasUnrealizedChildren = true;
            }
            else if (node is FileNode)
            {
                // У файла детей быть не может. Прячем стрелочку.
                this.HasUnrealizedChildren = false;

                // Если он вдруг был развернут (осталось от прошлой папки), сворачиваем
                this.IsExpanded = false;
            }
        }
    }
}

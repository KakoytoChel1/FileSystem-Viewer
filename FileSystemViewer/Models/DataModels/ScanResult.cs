using FileSystemViewer.ViewModels;
using ModernControls.Models;
using System.Collections.Generic;

namespace FileSystemViewer.Models.DataModels
{
    public class ScanResult
    {
        public AppState.ScanningStates FinalState { get; set; }
        public List<FileExtensionItem> ExtensionItems { get; set; } = new List<FileExtensionItem>();
        public List<TreemapNode> TreemapNodes { get; set; } = new List<TreemapNode>();
    }
}

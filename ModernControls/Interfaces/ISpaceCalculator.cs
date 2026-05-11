using ModernControls.Models;
using System.Collections.Generic;
using Windows.Foundation;

namespace ModernControls.Interfaces
{
    public interface ISpaceCalculator
    {
        public void CalculateLevelSpace(List<WrappedTreemapNode> nodes, Rect freeSpace);
    }
}

using ModernControls.Interfaces;
using ModernControls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace ModernControls
{
    public class SpaceCalculator : ISpaceCalculator
    {
        public void CalculateLevelSpace(List<WrappedTreemapNode> nodes, Rect freeSpace)
        {
            if (nodes == null || nodes.Count == 0 || freeSpace.Width <= 0 || freeSpace.Height <= 0)
                return;

            var sortedNodes = nodes.OrderByDescending(n => n.AreaSize).ToList();
            PlaceAndDecide(sortedNodes, freeSpace);
        }

        private void PlaceAndDecide(List<WrappedTreemapNode> remainingNodes, Rect currentFreeSpace)
        {
            if (remainingNodes.Count == 0)
                return;

            double shortestSide = Math.Min(currentFreeSpace.Width, currentFreeSpace.Height);
            double currentRatio = double.MaxValue;

            List<WrappedTreemapNode> currentRowGroup = new List<WrappedTreemapNode>();

            while (remainingNodes.Count > 0)
            {
                WrappedTreemapNode currentNode = remainingNodes[0];

                if (currentRowGroup.Count == 0)
                {
                    currentRowGroup.Add(currentNode);
                    remainingNodes.RemoveAt(0);
                    currentRatio = CalculateWorstRatio(currentRowGroup, shortestSide);
                    continue;
                }

                var testGroup = new List<WrappedTreemapNode>(currentRowGroup) { currentNode };
                double newRatio = CalculateWorstRatio(testGroup, shortestSide);

                if (newRatio <= currentRatio)
                {
                    currentRowGroup.Add(currentNode);
                    remainingNodes.RemoveAt(0);
                    currentRatio = newRatio;
                }
                else
                {
                    break;
                }
            }

            Rect newFreeSpace = LayoutRow(currentRowGroup, currentFreeSpace);

            PlaceAndDecide(remainingNodes, newFreeSpace);
        }

        private double CalculateWorstRatio(List<WrappedTreemapNode> rowGroup, double shortestSide)
        {
            double rowArea = rowGroup.Sum(n => n.AreaSize);
            double thickness = rowArea / shortestSide;
            double worstRatio = 0;

            foreach (var node in rowGroup)
            {
                double length = node.AreaSize / thickness;
                double ratio = Math.Max(length / thickness, thickness / length);

                if (ratio > worstRatio)
                {
                    worstRatio = ratio;
                }
            }
            return worstRatio;
        }

        private Rect LayoutRow(List<WrappedTreemapNode> rowGroup, Rect spaceBounds)
        {
            double rowArea = rowGroup.Sum(n => n.AreaSize);
            bool isHorizontal = spaceBounds.Width >= spaceBounds.Height;
            double length = isHorizontal ? rowArea / spaceBounds.Height : rowArea / spaceBounds.Width;

            double currentX = spaceBounds.X;
            double currentY = spaceBounds.Y;

            foreach (var node in rowGroup)
            {
                if (isHorizontal)
                {
                    double nodeHeight = node.AreaSize / length;
                    node.Bounds = new Rect(currentX, currentY, length, nodeHeight);
                    currentY += nodeHeight;
                }
                else
                {
                    double nodeWidth = node.AreaSize / length;
                    node.Bounds = new Rect(currentX, currentY, nodeWidth, length);
                    currentX += nodeWidth;
                }
            }

            if (isHorizontal)
            {
                return new Rect(spaceBounds.X + length, spaceBounds.Y, Math.Max(0, spaceBounds.Width - length), spaceBounds.Height);
            }
            else
            {
                return new Rect(spaceBounds.X, spaceBounds.Y + length, spaceBounds.Width, Math.Max(0, spaceBounds.Height - length));
            }
        }
    }
}

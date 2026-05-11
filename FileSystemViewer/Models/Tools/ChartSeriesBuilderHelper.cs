using FileSystemViewer.Models.DataModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemViewer.Models.Tools
{
    public static class ChartSeriesBuilderHelper
    {
        public static List<ISeries> Build(IEnumerable<FileExtensionItem> fileExtensionItems)
        {
            List<ISeries> fileExtesnionSeries = new List<ISeries>();

            var validItems = new List<FileExtensionItem>();
            var otherItems = new List<FileExtensionItem>();

            foreach (var item in fileExtensionItems)
            {
                if (item.Percent <= 1)
                    otherItems.Add(item);
                else
                    validItems.Add(item);
            }

            var seriesList = validItems.Select(TransformIntoSeries);
            var otherSeries = ArrangeOtherSeries(otherItems);

            fileExtesnionSeries.AddRange(seriesList);
            fileExtesnionSeries.Add(otherSeries);

            return fileExtesnionSeries;
        }

        private static ISeries TransformIntoSeries(FileExtensionItem item)
        {
            var pieSeries = new PieSeries<long>
            {
                Values = new long[] { item.Size },
                Name = item.Extension,
                ToolTipLabelFormatter = point => $"{item.Percent:F2}%",
                InnerRadius = 0,
                HoverPushout = 5,
                Pushout = 2
            };

            var winColor = item.Color;
            pieSeries.Fill = new SolidColorPaint(new SKColor(winColor.R, winColor.G, winColor.B, winColor.A));

            return pieSeries;
        }

        private static ISeries ArrangeOtherSeries(List<FileExtensionItem> others)
        {
            PieSeries<long> pieSeries = new PieSeries<long>()
            {
                Values = new long[] { others.Sum(i => i.Size) },
                Name = "Other",
                ToolTipLabelFormatter = point => $"{others.Sum(i => i.Percent):F2}%",
                InnerRadius = 0,
                HoverPushout = 5,
                Pushout = 2
            };

            var otherColor = ColorManager.OtherColor;
            pieSeries.Fill = new SolidColorPaint(new SKColor(otherColor.R, otherColor.G, otherColor.B, otherColor.A));

            return pieSeries;
        }
    }
}

using FileSystemViewer.Models.DataModels;
using FileSystemViewer.Models.Tools;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI;

namespace FileSystemViewer.UnitTests.Models.Tools
{
    public class ChartSeriesBuilderHelperTests
    {
        // Properly means, all < 1% must be in 'Other' category
        [Fact]
        public void Build_WithMixedPercents_GroupsItemsProperly()
        {
            // Arrange
            long commonSize = 10000;

            var mp4Item = new FileExtensionItem { Extension = ".mp4", Size = 5000, FileCount = 1, Color = default };
            var txtItem = new FileExtensionItem { Extension = ".txt", Size = 50, FileCount = 1, Color = default };
            var iniItem = new FileExtensionItem { Extension = ".ini", Size = 50, FileCount = 1, Color = default };

            mp4Item.UpdateParameters(commonSize); // 5000 / 10000 = 50%
            txtItem.UpdateParameters(commonSize); // 50 / 10000 = 0.5% (Goes to Other)
            iniItem.UpdateParameters(commonSize); // 50 / 10000 = 0.5% (Goes to Other)

            var items = new List<FileExtensionItem> { mp4Item, txtItem, iniItem };

            // Act
            List<ISeries> result = ChartSeriesBuilderHelper.Build(items);

            // Assert
            Assert.Equal(2, result.Count); // 1 - .mp4, 2 - Other

            var seriesList = result.Cast<PieSeries<long>>().ToList();

            var mp4Series = seriesList.FirstOrDefault(s => s.Name == ".mp4");
            Assert.NotNull(mp4Series);
            Assert.Equal(5000, mp4Series.Values!.First()); // .mp4 size 5000

            var otherSeries = seriesList.FirstOrDefault(s => s.Name == "Other");
            Assert.NotNull(otherSeries);
            Assert.Equal(100, otherSeries.Values!.First()); // Other size 100
        }

        [Fact]
        public void Build_AllItemsAboveOnePercent_CreatesEmptyOtherSeries()
        {
            // Arrange
            long commonSize = 10000;

            var mp4Item = new FileExtensionItem { Extension = ".mp4", Size = 5000, FileCount = 1, Color = default };
            var aviItem = new FileExtensionItem { Extension = ".avi", Size = 5000, FileCount = 1, Color = default };

            mp4Item.UpdateParameters(commonSize);
            aviItem.UpdateParameters(commonSize);

            var items = new List<FileExtensionItem> { mp4Item, aviItem };

            // Act
            var result = ChartSeriesBuilderHelper.Build(items);

            // Assert
            Assert.Equal(3, result.Count);

            var seriesList = result.Cast<PieSeries<long>>().ToList();

            var otherSeries = seriesList.FirstOrDefault(s => s.Name == "Other");
            Assert.NotNull(otherSeries);
            Assert.Equal(0, otherSeries.Values!.First());
        }
    }
}

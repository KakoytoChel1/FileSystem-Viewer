using FileSystemViewer.Services;

namespace FileSystemViewer.UnitTests.Services
{
    public class FileExtentionItemServiceTests
    {
        [Fact]
        public void UpdateOrCreateFileExtensionItem_WithDifferentCases_AggregatesIntoOneItem()
        {
            // Arrange
            var service = new FileExtentionItemService();

            // Act
            service.UpdateOrCreateFileExtensionItem(".png", 100, 1);
            service.UpdateOrCreateFileExtensionItem(".PNG", 250, 1);
            var resultList = service.GetOrderedExtensionCollection();

            // Assert
            Assert.Single(resultList); // Must be one the same element
            Assert.Equal(".png", resultList[0].Extension); // Be lower
            Assert.Equal(350, resultList[0].Size); // Size amounts totaled 
        }
    }
}

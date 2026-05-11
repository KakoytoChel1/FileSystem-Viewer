using FileSystemViewer.Models;

namespace FileSystemViewer.UnitTests.Models
{
    public class UnicodeManagerTests
    {
        [Theory]
        [InlineData(".mp4")]
        [InlineData(".MP4")]
        [InlineData(".Mp4")]
        public void GetFileUnicodeByExtension_KnownExtensionDifferentCases_ReturnsSameSpecificColor(string extension)
        {
            // Arrange
            string? expectedUnicodeIcon;
            UnicodeManager.ExtensionIconPairs.TryGetValue(".mp4", out expectedUnicodeIcon);

            // Act
            string result = UnicodeManager.GetFileUnicodeByExtension(extension);

            // Assert
            Assert.NotEqual(UnicodeManager.FileIcon, result);
            Assert.Equal(expectedUnicodeIcon, result);
        }

        [Fact]
        public void GetFileUnicodeByExtension_UnknownExtension_ReturnsDefaultColor()
        {
            // Arrange
            string unknownExtension = ".404";

            // Act
            string result = UnicodeManager.GetFileUnicodeByExtension(unknownExtension);

            // Assert
            Assert.Equal(UnicodeManager.FileIcon, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetFileUnicodeByExtension_NullOrEmptyExtension_ReturnsDefaultColor(string invalidExtension)
        {
            // Arrange
            string? noExtensionColor;
            UnicodeManager.ExtensionIconPairs.TryGetValue(string.Empty, out noExtensionColor);

            // Act
            string result = UnicodeManager.GetFileUnicodeByExtension(invalidExtension);

            // Assert
            Assert.Equal(noExtensionColor, result);
        }
    }
}

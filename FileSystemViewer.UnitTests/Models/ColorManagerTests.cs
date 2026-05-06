using FileSystemViewer.Models;
using Windows.UI;

namespace FileSystemViewer.UnitTests.Models
{
    public class ColorManagerTests
    {
        [Theory]
        [InlineData(".mp4")]
        [InlineData(".MP4")]
        [InlineData(".Mp4")]
        public void GetColorByExtension_KnownExtensionDifferentCases_ReturnsSameSpecificColor(string extension)
        {
            // Arrange
            Color expectedColor;
            ColorManager.ExtensionColorPairs.TryGetValue(".mp4", out expectedColor);

            // Act
            Color result = ColorManager.GetColorByExtension(extension, false);

            // Assert
            Assert.NotEqual(ColorManager.DefaultColor, result);
            Assert.Equal(expectedColor, result);
        }

        [Fact]
        public void GetColorByExtension_UnknownExtension_ReturnsDefaultColor()
        {
            // Arrange
            string unknownExtension = ".404";

            // Act
            Color result = ColorManager.GetColorByExtension(unknownExtension, false);
            Color iconResult = ColorManager.GetColorByExtension(unknownExtension, true);

            // Assert
            Assert.Equal(ColorManager.DefaultColor, result);
            Assert.Equal(ColorManager.FileIconColor, iconResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetColorByExtension_NullOrEmptyExtension_ReturnsDefaultColor(string invalidExtension)
        {
            // Arrange
            Color noExtensionColor;
            ColorManager.ExtensionColorPairs.TryGetValue(string.Empty, out noExtensionColor);

            // Act
            Color result = ColorManager.GetColorByExtension(invalidExtension, false);

            // Assert
            Assert.Equal(noExtensionColor, result);
        }
    }
}

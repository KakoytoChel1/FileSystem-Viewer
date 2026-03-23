using CommunityToolkit.Mvvm.ComponentModel;
using Windows.UI;

namespace FileSystem_Viewer.Models.DataModels
{
    public class FileExtensionItem : ObservableObject
    {
        public FileExtensionItem() { }

        private long _commonSize = 0;

        public string Extension { get; set; } = null!;
        public Color? Color { get; set; }

        public long Size { get; set; }
        public long FileCount { get; set; }
        public double Percent
        {
            get
            {
                if (_commonSize == 0)
                    return 0;

                double result = (double)Size / _commonSize;
                return result * 100;
            }
        }

        /// <summary>
        /// Updates the internal parameters (Size, FileCount, Percent) and implement Percent property calculation based on the specified total size of all drives.
        /// </summary>
        public void UpdateParameters(long drivesSizeSum)
        {
            _commonSize = drivesSizeSum;

            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(FileCount));
            OnPropertyChanged(nameof(Percent));
        }
    }
}

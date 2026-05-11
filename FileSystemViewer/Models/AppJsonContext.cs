using FileSystemViewer.Models.DataModels;
using System.Text.Json.Serialization;

namespace FileSystemViewer.Models
{
    [JsonSerializable(typeof(AppSettings))]
    public partial class AppJsonContext : JsonSerializerContext { }
}

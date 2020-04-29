using System.Net.Http;

namespace Dnn.Modules.ResourceManager.Components.Models
{
    public class ThumbnailContent
    {
        public ByteArrayContent Content { get; set; }
        public string ContentType { get; set; }
        public int Height { get; set; }
        public string ThumbnailName { get; set; }
        public int Width { get; set; }
    }
}
using System.Linq;

namespace ARInstructionsEditor.Core.Models
{
    public enum MediaType
    {
        Image = 0,
        Video,
        Unkown
    }
    public class MediaFile
    {
        public string FileName { get; set; }
        public MediaType Type { get; set; }
    }
}
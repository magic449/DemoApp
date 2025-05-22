using Microsoft.VisualBasic.FileIO;

namespace DemoAppAPI.Model
{
    public class FileUploadModel
    {
        public IFormFile FileDetails { get; set; }
        public int FileType { get; set; }
    }
}

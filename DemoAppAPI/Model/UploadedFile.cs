using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoAppAPI.Model
{
    public class UploadedFile
    {
        
        public Guid FileId { get; set; }

     
        public string FileName { get; set; }

      
        public string ContentType { get; set; }

      
        public long FileSize { get; set; }

       
        public byte[] FileData { get; set; } // Stores the binary data

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public string userName { get; set; }
    }
}

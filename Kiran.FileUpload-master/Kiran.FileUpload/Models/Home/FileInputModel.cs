using Microsoft.AspNetCore.Http;

namespace Kiran.FileUpload.Models.Home
{
    public class FileInputModel
    {
        public IFormFile FileToUpload { get; set; }
    }
}

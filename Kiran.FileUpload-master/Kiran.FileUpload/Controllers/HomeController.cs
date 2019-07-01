using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Kiran.FileUpload.Models.Home;
using Microsoft.Extensions.FileProviders;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kiran.FileUpload.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileProvider fileProvider;

        public HomeController(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file,string filename,string expirationdate)
        {
            if (file == null)
            {
                return Content("file not selected");
            }
            else if (file.Length == 0)
            {
                return Content("file is empty");
            }
            var path = "";
            if (string.IsNullOrWhiteSpace(filename))
            {
                path = Path.Combine(
                     Directory.GetCurrentDirectory(), "wwwroot",
                     file.GetFilename());
            }
            else
            {
                path = Path.Combine(
                       Directory.GetCurrentDirectory(), "wwwroot",
                       filename);
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var newdict = new Dictionary<string, string>();

            using (StreamReader r = new StreamReader(Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        "expiration.json")))
            {
                var json = r.ReadToEnd();

                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                
                foreach (var kv in dict)
                {
                    //  Console.WriteLine(kv.Key + ":" + kv.Value);
                    if ((kv.Key != filename))
                    {
                        if ((kv.Key != file.GetFilename()))
                        {
                            newdict.Add(kv.Key, kv.Value);
                        }
                    }
                }
                if (filename == null)
                {
                    newdict.Add(file.GetFilename(), expirationdate);

                }
                else
                {
                    newdict.Add(filename, expirationdate);
                }

            }

                string jsonnew = JsonConvert.SerializeObject(newdict, Formatting.Indented);
                System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        "expiration.json"), string.Empty);
                System.IO.File.WriteAllText(Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        "expiration.json"), jsonnew);


            
            

            return RedirectToAction("Files");
        }


        [HttpPost]
        public async Task<IActionResult> DownloadLink()
        {
            
            return RedirectToAction("Files");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(string filename)
        {
            if (filename == null)
                return Content("filename not present");
            try
            { 
            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", filename);
            System.IO.File.Delete(path);
            }
            catch (Exception e)
            {
                return Content("Errorfilename", e.Message);
            }

            var newdict = new Dictionary<string, string>();

            using (StreamReader r = new StreamReader(Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        "expiration.json")))
            {
                var json = r.ReadToEnd();

                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                foreach (var kv in dict)
                {
                    Console.WriteLine(kv.Key + ":" + kv.Value);
                    if (!(kv.Key == filename))
                    {
                        newdict.Add(kv.Key, kv.Value);
                    }
                }
                
                

            }

            string jsonnew = JsonConvert.SerializeObject(newdict, Formatting.Indented);
            System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    "expiration.json"), string.Empty);
            System.IO.File.WriteAllText(Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot",
                    "expiration.json"), jsonnew);

            return RedirectToAction("Files");
        }

        public IActionResult Files()
        {
            var model = new FilesViewModel();
            foreach (var item in this.fileProvider.GetDirectoryContents(""))
            {
                model.Files.Add(
                    new FileDetails { Name = item.Name, Path = item.PhysicalPath });
            }
            return View(model);
        }

        public async Task<IActionResult> Download(string filename)
        {
            if (filename == null)
                return Content("filename not present");

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var content = GetContentType(path);
            return File(memory, content, Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == "")
            {
                return "text/plain";
            }
            else
            {
                if (types.ContainsKey(ext))
                {
                    return types[ext];
                }
                else
                {
                    return "text/plain";
                }
            }
            
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {"", "file"}
                
            };
        }
    }
}

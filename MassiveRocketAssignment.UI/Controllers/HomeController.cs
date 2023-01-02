using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Utilities;
using Microsoft.AspNetCore.Http;

namespace MassiveRocketAssignment.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClientInfo _clientInfo;
        private readonly IReader _csvReader;
        private static string FolderBasePath = System.AppDomain.CurrentDomain.BaseDirectory;
        string FolderPath = $@"{FolderBasePath}\{Constants.FolderName}\{Constants.CustomerName}";

        public FileUpload fileUpload { get; set; }

        public HomeController(ILogger<HomeController> logger, IClientInfo clientInfo, IReader reader)
        {
            _logger = logger;
            _clientInfo = clientInfo;
            _csvReader = reader;
        }

        public IActionResult Index(FileUpload postedFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if(postedFile?.FormFiles == null) 
            {
                return View();
            }

            foreach (var formFile in postedFile?.FormFiles)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Combine(FolderPath, formFile.FileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        formFile.CopyToAsync(stream);
                    }

                    var fileName = Path.GetFileName(formFile.FileName);
                    var fileExtension = Path.GetExtension(fileName);
                    var newFileName = string.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                    var documentViewModel = new DocumentViewModel()
                    {
                        Id = 0,
                        FileName = newFileName,
                        FileType = fileExtension,
                        Created = DateTime.Now,
                        Modified = DateTime.Now
                    };

                    using (var target = new MemoryStream())
                    {
                        formFile.CopyTo(target);
                        documentViewModel.FileData = target.ToArray();
                    }
                }
            }

            ViewBag.SuccessMsg = postedFile.FormFiles.Count.ToString() + " files uploaded!!";
            _logger.LogInformation("File upload successful");
            return View();
        }

        public async Task<IActionResult> Save()
        {
            var status = new List<Tuple<string, int>>();
            foreach (var filepath in Directory.GetFiles(FolderPath))
            {
                string filename = Path.GetFileName(filepath);
                var results = _csvReader.Read(filepath).Skip(1);

                status.Add(Tuple.Create(filename, results.Count()));
                await _clientInfo.AddClientsByCsv(results);

                _logger.LogInformation($"{filename} - processed successful");
            }

            ViewData["Status"] = status;
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
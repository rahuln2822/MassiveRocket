using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace MassiveRocketAssignment.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClientInfo _clientInfo;
        private readonly IReader _csvReader;
        private static string FolderBasePath = System.AppDomain.CurrentDomain.BaseDirectory;
        string FolderPath = $@"{FolderBasePath}\{Constants.FolderName}\{Constants.CustomerName}";

        private int RecordCount = 0;

        private static PaginationModel PaginationModel = new PaginationModel();

        public HomeController(ILogger<HomeController> logger, IClientInfo clientInfo, IReader reader)
        {
            _logger = logger;
            _clientInfo = clientInfo;
            _csvReader = reader;
        }

        public IActionResult Index(ViewModel postedFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if(postedFile?.FileUpload?.FormFiles == null) 
            {
                return View();
            }

            foreach (var formFile in postedFile?.FileUpload?.FormFiles)
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

            ViewBag.SuccessMsg = postedFile?.FileUpload?.FormFiles.Count.ToString() + " files uploaded!!";
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

                RecordCount = RecordCount + results.Count();

                status.Add(Tuple.Create(filename, results.Count()));
                await _clientInfo.AddClientsByCsv(results);

                _logger.LogInformation($"{filename} - processed successful");
            }

            ViewData["Status"] = status;
            return View("Index");
        }

        public async Task<IActionResult> Display(int currentPage = 1)
        {
            var vm = new ViewModel();
            //var PaginationModel = new PaginationModel(RecordCount == 0 ? _clientInfo.GetClientsCount().Result : RecordCount);
            PaginationModel.CurrentPage = currentPage;            
            PaginationModel.LastPage = currentPage - 10 < 1 ? 1 : currentPage - 10;
            var skipRecords = (currentPage - 1) * PaginationModel.PageSize;
            var results = await _clientInfo.GetAllClient(PaginationModel.PageSize, skipRecords);
            PaginationModel.ClientEntities = results.ToList();
            vm.PaginationModel = PaginationModel;
            return View("Index", vm);
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
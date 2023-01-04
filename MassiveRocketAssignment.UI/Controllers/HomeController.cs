using MassiveRocketAssignment.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Utilities;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace MassiveRocketAssignment.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClientInfo _clientInfo;
        private readonly IReader _csvReader;
        private static string FolderBasePath = AppDomain.CurrentDomain.BaseDirectory;
        string FolderPath = $@"{FolderBasePath}\{Constants.FolderName}\{Constants.CustomerName}";

        private int RecordCount = 0;

        private static PaginationModel PaginationModel = new PaginationModel();

        public HomeController(ILogger<HomeController> logger, IClientInfo clientInfo, IReader reader)
        {
            _logger = logger;
            _clientInfo = clientInfo;
            _csvReader = reader;
        }

        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
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
                        formFile.CopyTo(stream);
                    }

                    using (var target = new MemoryStream())
                    {
                        formFile.CopyTo(target);
                    }
                }
            }

            ViewBag.SuccessMsg = postedFile?.FileUpload?.FormFiles.Count.ToString() + " files uploaded!!";
            _logger.LogInformation("File upload successful");
            return View();
        }

        public async Task<IActionResult> Save()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving client data. {ex.Message}-{ex.StackTrace}");
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            finally 
            {
                // Archive files here.
                // Delete after archival.
                var dirInfo = new DirectoryInfo(FolderPath);
                foreach (var file in dirInfo.GetFiles())
                {
                    file.Delete();
                } 
            }
        }

        public async Task<IActionResult> Display(int currentPage = 1)
        {
            var vm = new ViewModel();
            PaginationModel.CurrentPage = currentPage;            
            PaginationModel.LastPage = currentPage - 10 < 1 ? 1 : currentPage - 10;
            var skipRecords = (currentPage - 1) * PaginationModel.PageSize;
            var results = await _clientInfo.GetAllClient(PaginationModel.PageSize, skipRecords);
            PaginationModel.ClientEntities = results.ToList();
            vm.PaginationModel = PaginationModel;
            return View("Index", vm);
        }

        [Route("Home/Search/")]
        [Route("Home/Search/{searchString?}/{currentPage}")]
        public async Task<IActionResult> Search(string searchString, int currentPage = 1)
        {
            if (searchString == null)
                return View();

            var vm = new ViewModel();
            vm.SearchString = searchString;
            PaginationModel.CurrentPage = currentPage;
            PaginationModel.LastPage = currentPage - 10 < 1 ? 1 : currentPage - 10;
            PaginationModel.Count= searchString.Length;
            var skipRecords = (currentPage - 1) * PaginationModel.PageSize;
            var results = await _clientInfo.GetClient(searchString, PaginationModel.PageSize, skipRecords);
            PaginationModel.ClientEntities = results.ToList();
            vm.PaginationModel = PaginationModel;
            return View("Search", vm);
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
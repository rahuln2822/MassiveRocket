using MassiveRocketAssignment.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Utilities;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment.Validation;

namespace MassiveRocketAssignment.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClientInfo _clientInfo;
        private readonly IReader _csvReader;
        private readonly IBatchProcessor<string> _batchProcessor;

        private static string FolderBasePath = AppDomain.CurrentDomain.BaseDirectory;
        string FolderPath = $@"{FolderBasePath}\{Constants.FolderName}\{Constants.CustomerName}";

        private static PaginationModel PaginationModel = new PaginationModel();

        public HomeController(ILogger<HomeController> logger, IClientInfo clientInfo, IReader reader, IBatchProcessor<string> batchProcessor)
        {
            _logger = logger;
            _clientInfo = clientInfo;
            _csvReader = reader;
            _batchProcessor = batchProcessor;
        }

        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        public IActionResult Index(ViewModel postedFile)
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (postedFile?.FileUpload?.FormFiles == null)
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
                var csvContents = new List<string>();
                foreach (var filepath in Directory.GetFiles(FolderPath))
                {
                    string filename = Path.GetFileName(filepath);
                    var results = _csvReader.Read(filepath).Skip(1);

                    csvContents.AddRange(results);

                    status.Add(Tuple.Create(filename, results.Count()));
                }

                var batches = _batchProcessor.CreateBatches(csvContents);

                var clientBatches = batches.Select(batch => ConvertBatchToClientEntity(batch));

                var task = Task.Run(() =>
                Parallel.ForEach(clientBatches, async (batch) =>
                {
                    await _clientInfo.AddClients(batch);
                }));

                await task;
                //_logger.LogInformation($"{filename} - processed successful");


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
            int skipRecords = await SetPaging(currentPage);

            var results = await _clientInfo.GetAllClient(PaginationModel.PageSize, skipRecords);
            PaginationModel.ClientEntities = results.ToList();

            var viewModel = new ViewModel();
            viewModel.PaginationModel = PaginationModel;
            return View("Index", viewModel);
        }

        public async Task<IActionResult> Search(string searchString, int currentPage = 1)
        {
            if (searchString == null)
                return View();

            int skipRecords = await SetPaging(currentPage, searchString);

            var results = await _clientInfo.GetClient(searchString, PaginationModel.PageSize, skipRecords);
            PaginationModel.ClientEntities = results.ToList();

            var viewModel = new ViewModel();
            viewModel.SearchString = searchString;
            viewModel.PaginationModel = PaginationModel;
            return View("Search", viewModel);
        }

        private async Task<int> SetPaging(int currentPage, string searchString = null)
        {
            if (currentPage == 1)
            {
                _clientInfo.TotalRecordCount = await _clientInfo.GetClientsCount(searchString);
            }

            PaginationModel.CurrentPage = currentPage;
            PaginationModel.Count = _clientInfo.TotalRecordCount;
            PaginationModel.StartPage = currentPage - 10 < 1 ? 1 : currentPage - 10;

            var endPageNumber = PaginationModel.StartPage + PaginationModel.MinimumPagesToDisplay;
            if (endPageNumber > PaginationModel.TotalPages)
            {
                PaginationModel.MinimumPagesToDisplay = (PaginationModel.MinimumPagesToDisplay - (endPageNumber - PaginationModel.TotalPages)) + 1;
            }
            else
            {
                PaginationModel.MinimumPagesToDisplay = 20;
            }
            var skipRecords = (currentPage - 1) * PaginationModel.PageSize;
            return skipRecords;
        }


        private IEnumerable<ClientEntity?> ConvertBatchToClientEntity(IEnumerable<string> csvBatch)
        {
            string customerIdentity = $"{Constants.CustomerName}";

            var result = csvBatch.Select(csv => ToClientEntity(csv, customerIdentity));

            return result.Where(entity => entity != null);
        }

        private ClientEntity? ToClientEntity(string csvLine, string partitionKey)
        {
            var clientEntity = new ClientEntity();
            try
            {
                string[] values = csvLine.Split(',');

                if (values.Length == 4)
                {
                    clientEntity.Id = $"{partitionKey}-{values[0]}-{values[3]}";
                    clientEntity.PartitionKey = partitionKey;
                    clientEntity.FirstName = values[0].ShouldNotBeNull();
                    clientEntity.LastName = values[1].ShouldNotBeNull();
                    clientEntity.Email = values[2].ShouldBeValidEmail();
                    clientEntity.ContactNumber = values[3];

                    return clientEntity;
                }
                else
                {
                    throw new ArgumentException($"Incorrect data - {csvLine}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting {csvLine} to ClientEntity - {ex.Message} : {ex.StackTrace} ");
                return null;
            }
        }
    }
}
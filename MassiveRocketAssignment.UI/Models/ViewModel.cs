using Microsoft.AspNetCore.Mvc;

namespace MassiveRocketAssignment.UI.Models
{
    [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
    public class ViewModel
    {
        public FileUpload FileUpload { get; set; }
        public PaginationModel PaginationModel { get; set; }
        public string SearchString { get; set; }
    }
}

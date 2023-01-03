using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MassiveRocketAssignment.Storage;
using System.ComponentModel.DataAnnotations;

namespace MassiveRocketAssignment.UI.Models
{
    public class PaginationModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int LastPage { get; set; }
        public int Count { get; set; }
        public int PageSize { get; set; } = 50;
        public int TotalPages => Count == 0 ? 1 : (int)Math.Ceiling(decimal.Divide(Count, PageSize));
        public int MinimumPagesToDisplay { get; set; } = 20;
        public List<ClientEntity> ClientEntities { get; set; }
    }
}

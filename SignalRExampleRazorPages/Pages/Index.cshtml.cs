using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignalRExampleRazorPages.Service;

namespace SignalRExampleRazorPages.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly  IConnectionService _connectionService;
        [BindProperty]
        public IReadOnlyList<CamaraModel> CamaraModels { get; set; } = new List<CamaraModel>();
        public IndexModel(ILogger<IndexModel> logger, IConnectionService connectionService)
        {
            _logger = logger;
            _connectionService = connectionService;
        }

        public void OnGet()
        {
            CamaraModels = _connectionService.GetCamaras();
        }
    }
}
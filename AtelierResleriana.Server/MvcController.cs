using Microsoft.AspNetCore.Mvc;

namespace AtelierResleriana.Server
{
    [Controller]
    public class MvcController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return RedirectToPage("/News");
        }
    }
}

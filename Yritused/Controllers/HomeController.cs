using Microsoft.AspNetCore.Mvc;

namespace Yritused.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}

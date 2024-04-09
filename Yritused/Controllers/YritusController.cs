using Microsoft.AspNetCore.Mvc;
using Yritused.Models;

namespace Yritused.Controllers
{
    public class YritusController : Controller
    {
        private IYritusRepository repository;

        public YritusController(IYritusRepository repo) 
        {
            repository = repo;
        }

        public IActionResult List() => View(repository.Yritused);
    }
}

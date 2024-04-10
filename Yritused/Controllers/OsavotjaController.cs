using Microsoft.AspNetCore.Mvc;
using Yritused.Models;

namespace Yritused.Controllers
{
    public class OsavotjaController : Controller
    {
        private IOsavotjaRepository repository;

        public OsavotjaController(IOsavotjaRepository repo)
        {
            repository = repo;      
        }

        public IActionResult List() => View(repository.Osavotjad);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Controllers
{
    public class ProverkaController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            Console.WriteLine("Action!");
            return View("<h1>I m here!</h1>");
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Controllers
{
    public class LogOutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

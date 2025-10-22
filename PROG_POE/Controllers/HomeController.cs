using Microsoft.AspNetCore.Mvc;

namespace PROG_POE.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

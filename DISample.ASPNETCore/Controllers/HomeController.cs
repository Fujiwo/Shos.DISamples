using DISample.ASPNETCore.Models;
using DISample.ASPNETCore.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DISample.ASPNETCore.Controllers
{
    public class HomeController : Controller
    {
        readonly ILogger<HomeController> _logger;
        readonly IMyService _myService;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        //public HomeController(IMyService myService)
        //{
        //    _myService = myService;
        //}

        public HomeController(ILogger<HomeController> logger, IMyService myService)
        {
            _logger = logger;
            _myService = myService;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("HomeController.Index() is called.");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("HomeController.Privacy() is called.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
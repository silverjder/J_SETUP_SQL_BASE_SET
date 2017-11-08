using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J_SETUP_SQL.Controllers
{
    public class HomeController : Controller
    {
        private ILogger<HomeController> _logger;
        private IConfiguration _config;
        public HomeController( ILogger<HomeController> logger , IConfiguration config)
        { 
            _logger = logger;
            _config = config; 
        }
        public IActionResult Index()
        {
            if (_config.GetSection("ConnectionStrings:DefaultConnection").Value.ToString().Equals(""))
            {
                this.Response.Redirect("/Setup/Index");
            }
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}

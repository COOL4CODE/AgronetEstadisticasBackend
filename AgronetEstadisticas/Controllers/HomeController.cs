using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Cors;
using System.Diagnostics;

using AgronetEstadisticas.Models.parametersBinding;

namespace AgronetEstadisticas.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public FileResult DownloadTable(DownloadTable model)
        {
            return File(model.content, System.Net.Mime.MediaTypeNames.Application.Octet, model.filename + "." + model.format);
        }
    }
}

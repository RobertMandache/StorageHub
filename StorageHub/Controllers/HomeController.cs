using StorageHub.Models;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace StorageHub.Controllers
{
    public class HomeController : Controller
    {     
        [Authorize]
        public ActionResult Index()
        {
            if (Session["DropBoxStatus"] == null)
                return RedirectToAction("DropboxConnect", "StorageManagement");
            if (Session["GoogleDriveStatus"] == null)
                return RedirectToAction("GoogleDriveConnect", "StorageManagement");
            if ((int)Session["DropBoxStatus"] != StorageService.ServiceStatus.NotLinked || (int)Session["GoogleDriveStatus"] != StorageService.ServiceStatus.NotLinked)
                return RedirectToAction("Index", "Files");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }       
    }
}
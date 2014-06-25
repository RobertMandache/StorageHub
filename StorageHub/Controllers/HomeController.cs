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
            string aesiv = @"!qaz2wsx#edc4rfv";
            string clearText = "Ana are mere sau poarta push-up?";
            string encrypted = Utility.Encryption.Encrypt(aesiv, clearText);
            string decrypted = Utility.Encryption.Decrypt(encrypted);
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }       
    }
}
using DropNet;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

        [Authorize]
        public async Task<ActionResult> DriveAsync(CancellationToken cancellationToken)
        {
            ViewBag.Message = "Your drive page.";

            var driveService = (DriveService)Session["GoogleDriveService"];

            List<File> results = new List<File>();
            FilesResource.ListRequest request = driveService.Files.List();
            //request.Q = "sharedWithMe != true";
            request.Q = "trashed = false";
            do
            {
                try
                {
                    FileList files = request.Execute();

                    results.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return View(results);
        }

    
    }
}
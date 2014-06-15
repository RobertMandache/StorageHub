using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using StorageHub.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Threading;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Drive.v2.Data;
using DropNet;

namespace StorageHub.Controllers
{    
    [Authorize]
    public class StorageManagementController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /StorageManagement/
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            return View(currentUser.StorageServices.ToList());
        }

        [HttpGet]
        public ActionResult Manage()
        {            
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            var curentServices = currentUser.StorageServices;
            Dictionary<int, bool> status = new Dictionary<int, bool>();
            var availableServices = StorageService.getAvailableServices();
            foreach(var x in availableServices)
            {
                status.Add(x, false);
            }
            foreach(var x in curentServices)
            {
                status[x.ServiceType] = true;
            }           
            return View(status);
        }

        [HttpGet]
        public ActionResult Delete(int? serviceType)
        {
            if (serviceType == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            var storageservice = currentUser.StorageServices.First(x => x.ServiceType == serviceType);            
            if (storageservice == null)
            {
                return HttpNotFound();
            }
            db.StorageServices.Remove(storageservice);
            db.SaveChanges();
            StorageService.ClearServiceFromSession((int)serviceType);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public async Task<ActionResult> DriveAsync(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new StorageHub.Utility.DriveFlowMetadata()).
                    AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                return new RedirectResult(result.RedirectUri);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "StorageHub"
            });

            Session["GoogleDriveStatus"] = StorageService.ServiceStatus.Connected;
            Session["GoogleDriveService"] = driveService;
            
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DriveCreate()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            StorageService driveService = new StorageService()
            {
                ServiceType = 1
            };
            currentUser.StorageServices.Add(driveService);
            db.SaveChanges();
            return RedirectToAction("DriveAsync");
        }

        public ActionResult DropboxCreate()
        {
            var client = new DropNetClient(Utility.AppCredentials.DROPBOX_APP_KEY, Utility.AppCredentials.DROPBOX_APP_SECRET);            
            client.GetToken();
            var callbackUrl = Url.Action("DropboxCallback", "StorageManagement", null, Request.Url.Scheme);
            var url = client.BuildAuthorizeUrl(callbackUrl);            
            Session["DropBoxClient"] = client;
            return Redirect(url);
        }

        public ActionResult DropboxCallback()
        {
            DropNetClient cl = (DropNetClient)Session["DropBoxClient"];
            var accessToken = cl.GetAccessToken();
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            StorageService dropboxService = new StorageService
            {
                ServiceType = StorageService.ServiceTypes.Dropbox,
                UserToken = accessToken.Token,
                UserSecret = accessToken.Secret
            };
            currentUser.StorageServices.Add(dropboxService);
            db.SaveChanges();
            Session["DropBoxStatus"] = StorageService.ServiceStatus.Connected;
            Session["DropBoxClient"] = cl;            
            return RedirectToAction("Index", "Home");
        }

        public ActionResult StorageConnect()
        {
            return RedirectToAction("DropboxConnect");
        }

        public ActionResult DropboxConnect()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            var dropboxService = currentUser.StorageServices.FirstOrDefault(x => x.ServiceType == StorageService.ServiceTypes.Dropbox);
            if (dropboxService != null)
            {
                DropNetClient cl = new DropNetClient(
                    Utility.AppCredentials.DROPBOX_APP_KEY, Utility.AppCredentials.DROPBOX_APP_SECRET, dropboxService.UserToken, dropboxService.UserSecret);
                Session["DropBoxStatus"] = StorageService.ServiceStatus.Connected;
                Session["DropBoxClient"] = cl;
            }
            else
            {
                Session["DropBoxStatus"] = StorageService.ServiceStatus.NotLinked;
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GoogleDriveConnect()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            var googledriveService = currentUser.StorageServices.FirstOrDefault(x => x.ServiceType == StorageService.ServiceTypes.GoogleDrive);
            if(googledriveService != null)
            {
                return RedirectToAction("DriveAsync");
            }
            else
            {
                Session["GoogleDriveStatus"] = StorageService.ServiceStatus.NotLinked;
            }
            return RedirectToAction("Index", "Home");
        }

    }
}

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

        public ActionResult Test()
        {
            string currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
            var dropboxService = currentUser.StorageServices.FirstOrDefault(x => x.ServiceType == StorageService.ServiceTypes.Dropbox);
            DropNetClient cl = new DropNetClient(
                Utility.AppCredentials.DROPBOX_APP_KEY, Utility.AppCredentials.DROPBOX_APP_SECRET, dropboxService.UserToken, dropboxService.UserSecret);
            var y = cl.GetMetaData();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DriveConnect(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new StorageHub.Utility.DriveFlowMetadata()).
                AuthorizeAsync(cancellationToken);
            
            if (result.Credential != null)
            {

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "StorageHub"
                });

                // YOUR CODE SHOULD BE HERE..
                // SAMPLE CODE:
                var list = await service.Files.List().ExecuteAsync();
                ViewBag.Message = "FILE COUNT IS: " + list.Items.Count();
                return View();
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        // GET: /StorageManagement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StorageService storageservice = db.StorageServices.Find(id);
            if (storageservice == null)
            {
                return HttpNotFound();
            }
            return View(storageservice);
        }

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

        // GET: /StorageManagement/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /StorageManagement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ServiceType")] StorageService storageservice)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                var currentUser = db.Users.FirstOrDefault(x => x.Id == currentUserId);
                var curentServices = currentUser.StorageServices;
                var availableServices = StorageService.getAvailableServices();
                foreach(var x in curentServices)
                {
                    int type = x.ServiceType;
                }
                currentUser.StorageServices.Add(storageservice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(storageservice);
        }

        // GET: /StorageManagement/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StorageService storageservice = db.StorageServices.Find(id);
            if (storageservice == null)
            {
                return HttpNotFound();
            }
            return View(storageservice);
        }

        // POST: /StorageManagement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ID,ServiceType,AccessToken,RefreshToken")] StorageService storageservice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(storageservice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(storageservice);
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

        // POST: /StorageManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StorageService storageservice = db.StorageServices.Find(id);
            db.StorageServices.Remove(storageservice);
            db.SaveChanges();
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
                Session["DropBoxStatus"] = "connected";
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

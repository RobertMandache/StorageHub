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

        // GET: /StorageManagement/Delete/5
        public ActionResult Delete(int? id)
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
    }
}

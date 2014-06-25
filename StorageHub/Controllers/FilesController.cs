using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using StorageHub.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using DropNet;
using System.Web;
using System.IO;

namespace StorageHub.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        public ActionResult Index()
        {
            List<StorageTree> folderTrees = new List<StorageTree>();
            if ((int)Session["GoogleDriveStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string driveCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.GoogleDrive;
                if (HttpContext.Cache[driveCacheKey] == null)
                {
                    HttpContext.Cache[driveCacheKey] = StorageTree.BuildDriveFolderTree();
                }
                StorageTree driveTree = (StorageTree)HttpContext.Cache[driveCacheKey];
                folderTrees.Add(driveTree);
            }

            if ((int)Session["DropBoxStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string dropboxCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.Dropbox;
                if (HttpContext.Cache[dropboxCacheKey] == null)
                {
                    var x = StorageTree.BuildDropboxFolderTree();
                    HttpContext.Cache[dropboxCacheKey] = x.Item1;
                    HttpContext.Cache[dropboxCacheKey + "dict"] = x.Item2;
                }
                StorageTree dropboxTree = (StorageTree)HttpContext.Cache[dropboxCacheKey];
                folderTrees.Add(dropboxTree);
            }

            ViewBag.OpenFolder = "";
            ViewBag.Header = "";
            return View(new FileTreeViewModel() 
            { 
                FolderTrees = folderTrees,
                ContextFiles = new List<FileModel>()
            });
        }
        
        [HttpGet]
        public ActionResult Drive(string folderId)
        {
            List<StorageTree> folderTrees = new List<StorageTree>();
            if ((int)Session["GoogleDriveStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string driveCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.GoogleDrive;
                if (HttpContext.Cache[driveCacheKey] == null)
                {
                    HttpContext.Cache[driveCacheKey] = StorageTree.BuildDriveFolderTree();
                }
                StorageTree driveTree = (StorageTree)HttpContext.Cache[driveCacheKey];
                folderTrees.Add(driveTree);
            }

            if ((int)Session["DropBoxStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string dropboxCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.Dropbox;
                if (HttpContext.Cache[dropboxCacheKey] == null)
                {
                    var x = StorageTree.BuildDropboxFolderTree();
                    HttpContext.Cache[dropboxCacheKey] = x.Item1;
                    HttpContext.Cache[dropboxCacheKey + "dict"] = x.Item2;
                }
                StorageTree dropboxTree = (StorageTree)HttpContext.Cache[dropboxCacheKey];
                folderTrees.Add(dropboxTree);
            } 

            var y = StorageTree.GetDriveChildren(folderId);
            ViewBag.OpenFolder = folderId;
            //ViewBag.Header = folderId;
            return View("Index", new FileTreeViewModel()
                {
                    Folder = folderId,
                    FolderTrees = folderTrees,
                    ContextFiles = y
                }
                );
        }

        

        public ActionResult Dropbox(string folderId, bool? queried, bool? getFiles)
        {
            List<StorageTree> folderTrees = new List<StorageTree>();
            if ((int)Session["GoogleDriveStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string driveCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.GoogleDrive;
                if (HttpContext.Cache[driveCacheKey] == null)
                {
                    HttpContext.Cache[driveCacheKey] = StorageTree.BuildDriveFolderTree();
                }
                StorageTree driveTree = (StorageTree)HttpContext.Cache[driveCacheKey];
                folderTrees.Add(driveTree);
            }
            string notNullId = folderId;
            if ((int)Session["DropBoxStatus"] != StorageService.ServiceStatus.NotLinked)
            {
                string dropboxCacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.Dropbox;
                if (HttpContext.Cache[dropboxCacheKey] == null)
                {
                    var x = StorageTree.BuildDropboxFolderTree();
                    HttpContext.Cache[dropboxCacheKey] = x.Item1;
                    HttpContext.Cache[dropboxCacheKey + "dict"] = x.Item2;
                }
                StorageTree dropboxTree = (StorageTree)HttpContext.Cache[dropboxCacheKey];
                folderTrees.Add(dropboxTree);

                Dictionary<string, StorageTree> dict = (Dictionary<string, StorageTree>)HttpContext.Cache[dropboxCacheKey + "dict"];
                
                if (folderId == null) notNullId = "";
                if (dict[notNullId].Queried == false)
                {
                    DropNetClient client = (DropNetClient)Session["DropBoxClient"];
                    var subtree = StorageTree.BuildDropboxSubtreeTwoLevels(client, folderId);
                    foreach (var x in subtree)
                    {
                        foreach (var y in x.Subtree)
                        {
                            dict.Add(y.Id, y);
                        }
                        dict[x.Id].Subtree = x.Subtree;
                        if (x.Subtree == null || x.Subtree.Count == 0)
                            dict[x.Id].Queried = true;
                    }
                    //dict[folderId].Subtree = subtree;
                    dict[folderId].Queried = true;
                    HttpContext.Cache[dropboxCacheKey] = dropboxTree;
                }
            }
            List<FileModel> children = new List<FileModel>();
            if (getFiles != null)
            {
                children = StorageTree.GetDropboxChildren(notNullId);
                foreach(var file in children)
                {
                    if (file.DownloadUrl != null)
                        file.DownloadUrl = Url.Action("DropboxDownload", "Files", new { fileId = file.Id }, Request.Url.Scheme);
                }
            }
            ViewBag.OpenFolder = folderId;
            ViewBag.Header = folderId;
            return View("Index", new FileTreeViewModel()
                {
                    Folder = folderId,
                    FolderTrees = folderTrees,
                    ContextFiles = children
                });
        }

        public ActionResult ClearCache()
        {
            string userId = User.Identity.GetUserId();
            Utility.Util.ClearDriveCache(userId);
            Utility.Util.ClearDropboxCache(userId);
            return RedirectToAction("Index");
        }        

        //public ActionResult Upload(string folderId, HttpPostedFileBase fileToUpload)
        //{
        //    if (fileToUpload != null)
        //    {
        //        MemoryStream target = new MemoryStream();
        //        fileToUpload.InputStream.CopyTo(target);
        //        byte[] fileBytes = target.ToArray();
                
        //        if (folderId != null)
        //            fileBytes = StorageTree.DownloadDropboxFile(folderId);
        //    }
        //    return View();
        //}

        public FileResult DropboxDownload(string fileId)
        {
            var x = StorageTree.DownloadDropboxFile(fileId);
            byte[] fileBytes = x.Item3;
            string fileName = x.Item1;
            string mime = MimeMapping.GetMimeMapping(x.Item1);
            return File(fileBytes, mime, fileName);
        }


        //public ActionResult DropboxTree(string folderId, bool? queried)
        //{
        //    string cacheKey = User.Identity.GetUserId() + StorageService.ServiceTypes.Dropbox;
        //    if (HttpContext.Cache[cacheKey] == null)
        //    {
        //        var x = StorageTree.BuildDropboxFolderTree();
        //        HttpContext.Cache[cacheKey] = x.Item1;
        //        HttpContext.Cache[cacheKey + "dict"] = x.Item2;
        //    }
        //    StorageTree tree = (StorageTree)HttpContext.Cache[cacheKey];
        //    Dictionary<string, StorageTree> dict = (Dictionary<string, StorageTree>)HttpContext.Cache[cacheKey + "dict"];
        //    if(dict[folderId].Queried == false)
        //    {
        //        DropNetClient client = (DropNetClient)Session["DropBoxClient"];
        //        var subtree = StorageTree.BuildDropboxSubtreeTwoLevels(client, folderId);
        //        foreach (var x in subtree)
        //        {
        //            foreach (var y in x.Subtree)
        //            {
        //                dict.Add(y.Id, y);
        //            }
        //            dict[x.Id].Subtree = x.Subtree;
        //        }
        //        //dict[folderId].Subtree = subtree;
        //        dict[folderId].Queried = true;
        //        HttpContext.Cache[cacheKey] = tree;
        //    }                      
        //    ViewBag.OpenFolder = folderId;
        //    return View("Dropbox", new FileTreeViewModel()
        //    {
        //        FolderTree = tree               
        //    });
        //}
	}
}
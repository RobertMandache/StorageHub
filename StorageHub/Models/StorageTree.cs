using DropNet;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class StorageTree
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Queried { get; set; }
        public int Source { get; set; }
        public List<StorageTree> Subtree { get; set; }


        public static StorageTree BuildDriveFolderTree()
        {
            var driveService = (DriveService)HttpContext.Current.Session["GoogleDriveService"];
            About about = driveService.About.Get().Execute();
            string rootId = about.RootFolderId;
            List<File> folders = GetDriveFolders();

            Dictionary<string, StorageTree> partialTrees = new Dictionary<string, StorageTree>();
            partialTrees.Add(rootId, new StorageTree()
                {
                    Id = rootId,
                    Name = "Google Drive",
                    Source = StorageService.ServiceTypes.GoogleDrive,
                    Subtree = new List<StorageTree>()
                });
            foreach(var folder in folders)
            {
                if(partialTrees.ContainsKey(folder.Id))
                {
                    partialTrees[folder.Id].Name = folder.Title;
                }
                else 
                {
                    partialTrees.Add(folder.Id, new StorageTree()
                        {
                            Id = folder.Id,
                            Name = folder.Title,
                            Source = StorageService.ServiceTypes.GoogleDrive,
                            Subtree = new List<StorageTree>()
                        });
                }
                if(folder.Parents.Count > 0)
                {
                    string parentId = folder.Parents[0].Id;
                    if(!partialTrees.ContainsKey(parentId))
                    {
                        partialTrees.Add(parentId, new StorageTree()
                            {
                                Id = parentId,
                                Source = StorageService.ServiceTypes.GoogleDrive,
                                Subtree = new List<StorageTree>()
                            });
                    }
                    partialTrees[parentId].Subtree.Add(partialTrees[folder.Id]);
                }
                else 
                {
                    partialTrees[rootId].Subtree.Add(partialTrees[folder.Id]);
                }
            }            
            return partialTrees[rootId];
        }
        public static List<File> GetDriveFolders()
        {
            var driveService = (DriveService)HttpContext.Current.Session["GoogleDriveService"];

            List<File> results = new List<File>();
            FilesResource.ListRequest request = driveService.Files.List();
            request.Q = "trashed = false and mimeType = 'application/vnd.google-apps.folder'";
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
            
            return results;
        }

        public static List<ChildReference> GetDriveFolderContents(string id)
        {
            var driveService = (DriveService)HttpContext.Current.Session["GoogleDriveService"];

            ChildrenResource.ListRequest request = driveService.Children.List(id);
            request.MaxResults = 1000;
            List<ChildReference> results = new List<ChildReference>();

            request.Q = "trashed = false";
            do
            {
                try
                {
                    ChildList children = request.Execute();

                    results.AddRange(children.Items);
                    request.PageToken = children.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return results;
        }

        public static List<FileModel> GetDriveChildren(string folderId)
        {
            var driveService = (DriveService)HttpContext.Current.Session["GoogleDriveService"];

            List<FileModel> results = new List<FileModel>();
            FilesResource.ListRequest request = driveService.Files.List();
            request.Q = "trashed = false and '" + folderId + "' in parents";
            do
            {
                try
                {
                    FileList files = request.Execute();
                    foreach(var file in files.Items)
                    {

                        results.Add(new FileModel()
                            {
                                Id = file.Id,
                                Name = file.Title,
                                Icon = file.IconLink,
                                LastModified = file.ModifiedDate,
                                DownloadUrl = file.WebContentLink,
                                ExternalUrl = file.AlternateLink,
                                Size = Utility.Util.GetBytesReadable(file.FileSize),
                                Source = StorageService.ServiceTypes.GoogleDrive
                            });
                    }                    
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return results;
        }

        public static List<StorageTree> BuildDropboxSubtreeOneLevel(DropNetClient client, string scope)
        {
            var metadata = client.GetMetaData(scope);
            List<StorageTree> subFolders = new List<StorageTree>();
            foreach(var file in metadata.Contents)
            {
                if (file.Is_Dir)
                {
                    subFolders.Add(new StorageTree()
                    {
                        Id = file.Path,
                        Name = file.Name,
                        Source = StorageService.ServiceTypes.Dropbox,
                        Queried = false

                    });
                }
            }
            return subFolders;
        }

        public static List<StorageTree> BuildDropboxSubtreeTwoLevels(DropNetClient client, string scope)
        {
            var metadata = client.GetMetaData(scope);
            List<StorageTree> subFolders = new List<StorageTree>();
            foreach (var file in metadata.Contents)
            {
                if (file.Is_Dir)
                {
                    subFolders.Add(new StorageTree()
                    {
                        Id = file.Path,
                        Name = file.Name,
                        Source = StorageService.ServiceTypes.Dropbox,
                        Queried = false,
                        Subtree = BuildDropboxSubtreeOneLevel(client, file.Path)
                    });
                }
            }
            return subFolders;
        }

        public static Tuple<StorageTree,Dictionary<string,StorageTree>> BuildDropboxFolderTree()
        {
            DropNetClient client = (DropNetClient)HttpContext.Current.Session["DropBoxClient"];
            List<StorageTree> subtree = BuildDropboxSubtreeTwoLevels(client, "");            
            StorageTree rootTree = new StorageTree()
            {
                Id = "",
                Name = "Dropbox",
                Queried = true,
                Source = StorageService.ServiceTypes.Dropbox,
                Subtree = subtree
            };  
            Dictionary<string,StorageTree> folderDict = new Dictionary<string,StorageTree>();
            folderDict.Add(rootTree.Id, rootTree);
            foreach(var x in subtree)
            {
                folderDict.Add(x.Id, x);
                foreach(var y in x.Subtree)
                {
                    folderDict.Add(y.Id, y);
                }
            }
            return Tuple.Create(rootTree, folderDict);                           
        }

        public static List<FileModel> GetDropboxChildren(string id)
        {
            if (id == null) return null;
            var client = (DropNetClient)HttpContext.Current.Session["DropBoxClient"];
            var metadata = client.GetMetaData(id);
            List<FileModel> results = new List<FileModel>();
            foreach (var file in metadata.Contents)
            {
                results.Add(new FileModel() 
                { 
                    Id = file.Path,
                    Name = file.Name,                    
                    LastModified = file.ModifiedDate,
                    DownloadUrl = file.Is_Dir ? null : file.Path,
                    Size = file.Size,
                    Source = StorageService.ServiceTypes.Dropbox
                });
            }
            return results;
        }

        public static Tuple<string, string, byte[]> DownloadDropboxFile(string id)
        {
            var client = (DropNetClient)HttpContext.Current.Session["DropBoxClient"];
            var metadata = client.GetMetaData(id);
            if (metadata.Is_Dir == false)
            {
                byte[] fileBytes = client.GetFile(id);
                return Tuple.Create(metadata.Name, metadata.Extension, fileBytes);
            }
            return null;
        }
    }
}
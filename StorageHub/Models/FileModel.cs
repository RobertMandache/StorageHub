using DropNet;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class FileModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
        public string DownloadUrl { get; set; }
        public string ExternalUrl { get; set; }
        public string Icon { get; set; }
        public string Size { get; set; }
        public int Source { get; set; }

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

        public static void DropboxUpload(string folderId, string fileName, byte[] fileContents)
        {
            var client = (DropNetClient)HttpContext.Current.Session["DropBoxClient"];
            var metadata = client.UploadFile(folderId, fileName, fileContents);
        }

        public static void DriveUpload(string folderId, string fileName, byte[] fileContents, string mimeType)
        {
            var driveService = (DriveService)HttpContext.Current.Session["GoogleDriveService"];
            File newFile = new File();
            newFile.Title = fileName;
            newFile.MimeType = mimeType;
            newFile.Parents = new List<ParentReference>() 
                { 
                    new ParentReference() { Id = folderId } 
                };
            System.IO.MemoryStream stream = new System.IO.MemoryStream(fileContents);
            try
            {
                FilesResource.InsertMediaUpload request = driveService.Files.Insert(newFile, stream, mimeType);
                request.Upload();

                File file = request.ResponseBody;               
            }
            catch (Exception e)
            {                            
            }
        }
    }
}
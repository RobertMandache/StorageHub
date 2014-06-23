using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class FileTreeViewModel
    {
        public string Folder { get; set; }
        public List<StorageTree> FolderTrees { get; set; }

        public List<FileModel> ContextFiles { get; set; }

        public FileTreeViewModel() { }
    }
}
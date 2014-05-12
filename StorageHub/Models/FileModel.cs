using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class FileModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string DownloadUrl { get; set; }
    }
}
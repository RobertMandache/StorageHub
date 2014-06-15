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
    }
}
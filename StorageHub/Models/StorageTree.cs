using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class StorageTree
    {
        public string Name { get; set; }
        public List<StorageTree> Subfolders { get; set; }
    }
}
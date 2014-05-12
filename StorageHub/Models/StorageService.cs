using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StorageHub.Models
{
    public class StorageService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ServiceType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
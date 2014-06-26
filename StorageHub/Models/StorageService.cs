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

        public List<int> getCurrentServices(string userId)
        {
            List<int> services = new List<int>();

            return services;
        }

        public static List<int> getAvailableServices()
        {
            List<int> services = new List<int>();
            services.Add(ServiceTypes.GoogleDrive);
            services.Add(ServiceTypes.Dropbox);
            return services;
        }

        public static void ClearServiceFromSession(int serviceType)
        {
            if (ServiceTypes.GoogleDrive == serviceType)
            {               
                HttpContext.Current.Session["GoogleDriveStatus"] = ServiceStatus.NotLinked;
                HttpContext.Current.Session["GoogleDriveService"] = null;
            }
            if (ServiceTypes.Dropbox == serviceType)
            {
                HttpContext.Current.Session["DropBoxStatus"] = ServiceStatus.NotLinked;
                HttpContext.Current.Session["DropBoxClient"] = null;
            }
        }

        public class ServiceTypes
        {
            public const int GoogleDrive = 1;
            public const int Dropbox = 2;
        }

        public class ServiceStatus
        {
            public const int Connected = 1;
            public const int NotLinked = 2;
        }
    }
}
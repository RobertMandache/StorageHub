using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace StorageHub.Utility
{
    public class DriveFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = Utility.AppCredentials.GOOGLEDRIVE_APP_KEY,
                    ClientSecret = Utility.AppCredentials.GOOGLEDRIVE_APP_SECRET
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore("Drive.Api.Auth.Store")
            });

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            //var user = controller.Session["user"];
            //if (user == null)
            //{
            //    user = Guid.NewGuid();
            //    controller.Session["user"] = user;
            //}
            //return user.ToString();
            return controller.User.Identity.GetUserName();
            return System.Web.HttpContext.Current.User.Identity.GetUserId();
        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }
    }
}
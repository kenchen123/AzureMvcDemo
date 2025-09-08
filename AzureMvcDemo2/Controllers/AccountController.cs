using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AzureMvcDemo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public async Task<ActionResult> Groups()
        {
            var clientId = System.Configuration.ConfigurationManager.AppSettings["AzureAd:ClientId"];
            var tenantId = System.Configuration.ConfigurationManager.AppSettings["AzureAd:TenantId"];
            var clientSecret = System.Configuration.ConfigurationManager.AppSettings["AzureAd:ClientSecret"];
            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(authority)
                .Build();

            string[] scopes = { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            //var graphClient = new GraphServiceClient(
            //    new DelegateAuthenticationProvider(req =>
            //    {
            //        req.Headers.Authorization =
            //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
            //        return Task.CompletedTask;
            //    }));

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            //var me = await graphClient.Me.Request().GetAsync();
            //var groups = await graphClient.Me.MemberOf.Request().GetAsync();
            var me = await graphClient.Me.GetAsync();
            var groups = await graphClient.Me.MemberOf.GetAsync();

            //var groupNames = new List<string>();
            //foreach (var directoryObject in groups.CurrentPage)
            //{
            //    if (directoryObject is Group g)
            //    {
            //        groupNames.Add(g.DisplayName);
            //    }
            //}

            ViewBag.UserName = me.DisplayName;
            //ViewBag.Groups = groupNames;
            return View();
        }

        public ActionResult SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
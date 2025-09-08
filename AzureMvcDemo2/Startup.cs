using Microsoft.Identity.Client;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(AzureMvcDemo.Startup))]

namespace AzureMvcDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var clientId = ConfigurationManager.AppSettings["AzureAd:ClientId"];
            var tenantId = ConfigurationManager.AppSettings["AzureAd:TenantId"];
            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
            var redirectUri = ConfigurationManager.AppSettings["AzureAd:RedirectUri"];
            var clientSecret = ConfigurationManager.AppSettings["AzureAd:ClientSecret"];

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                RedirectUri = redirectUri,
                ResponseType = "code id_token",
                Scope = "openid profile email offline_access User.Read GroupMember.Read.All",

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async context =>
                    {
                        var appConfidential = ConfidentialClientApplicationBuilder.Create(clientId)
                            .WithClientSecret(clientSecret)
                            .WithAuthority(authority)
                            .WithRedirectUri(redirectUri)
                            .Build();

                        string[] scopes = { "https://graph.microsoft.com/.default" };
                        var result = await appConfidential.AcquireTokenByAuthorizationCode(scopes, context.Code).ExecuteAsync();

                        context.HandleCodeRedemption(result.AccessToken, result.IdToken);
                    }
                }
            });
        }
    }
}
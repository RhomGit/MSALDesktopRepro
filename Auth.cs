using System;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using System.Threading.Tasks;
using System.Linq; 
using System.Diagnostics; 

namespace MSALTesting
{
     
    public class Auth
    {
        // ref: for microsoft identity client return URI https://github.com/Azure-Samples/active-directory-b2c-xamarin-native
        // ref: https://github.com/Azure-Samples/active-directory-b2c-xamarin-native/blob/master/UserDetailsClient/UserDetailsClient/MainPage.xaml.cs


        public AppPlatform appPlatform { get; private set; }
        public string clientId { get; private set; }
        public TokenCredentials creds { get; set; }
        public string authority;
        public AuthenticationResult authResult = null;
        public object parentActivity = null; // we only need this for Android, otherwise null is fine
        public IPublicClientApplication pca = null; 
        public UserFromClaims userFromClaims;
        public string[] scopes = null;
        public string RedirectUri;

        public enum AppPlatform
        {
            DesktopClient = 2,
            MobileClient = 3,
            Web = 4,
        }

        public static string MyDocumentsRoot()
        {
            string s = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\AE";
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(s);
            if (di.Exists == false)
                di.Create();
            return s;
        }

        public Auth(string Authority, AppPlatform platform, string clientId, string[] scopes, object parentActivity)
        {
            this.appPlatform = platform;
            this.clientId = clientId;
            this.parentActivity = parentActivity;
            this.scopes = scopes;

            string userDir = MyDocumentsRoot() + @"\";

            this.RedirectUri = $@"msal{this.clientId}://auth";

            this.pca = PublicClientApplicationBuilder.Create(this.clientId)
                .WithB2CAuthority(Authority)
                .WithLogging(CustomLoggingMethod, LogLevel.Info, enablePiiLogging: true, enableDefaultPlatformLogging: true)
                .WithRedirectUri(RedirectUri)
                .Build();

            Debug.WriteLine($@"Creating auth context with:
            B2C authority: {Authority}
            Client id: {this.clientId}
            Redirect uri: {RedirectUri}");
            if (this.scopes is null)
                System.Diagnostics.Debug.WriteLine("** Scopes are NULL **");
            else
            {
                foreach (var s in scopes)
                {
                    System.Diagnostics.Debug.WriteLine(s);
                }
            }

            switch (platform)
            {
                case AppPlatform.DesktopClient:
                    TokenCacheHelper.CacheFilePath = userDir + "msalcache.bin3";
                    TokenCacheHelper.EnableSerialization(this.pca.UserTokenCache);
                    break;
                default:
                    break;
            }
        } 


        public async Task<bool> Connect(Auth_VM AuthB2C, bool isSilent, string previousSignInName)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            System.Diagnostics.Debug.WriteLine($"Auth.Connect.Start ");

            var accounts = await pca.GetAccountsAsync();

            if (accounts != null && accounts.Count() > 1)
            {
                System.Windows.MessageBox.Show("Multiple cached accounts discovered");
                foreach (var item in accounts)
                {
                    System.Diagnostics.Debug.WriteLine($" - {item.ToString()}");
                }
            }
            

            var firstAccount = accounts.FirstOrDefault();
           
            try
            {
                if (isSilent)
                { 
                    authResult = await pca.AcquireTokenSilent(this.scopes, firstAccount).ExecuteAsync(); 
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Auth.Connect.SignOut @ {stopWatch.ElapsedMilliseconds / 1000}");
                    await SignOut();

                    System.Diagnostics.Debug.WriteLine($"Auth.Connect.AcquireTokenInteractive @ {stopWatch.ElapsedMilliseconds / 1000}");
                    authResult = await pca.AcquireTokenInteractive(this.scopes)
                        .WithUseEmbeddedWebView(true)
                        .WithLoginHint(previousSignInName)
                        //.WithAccount(firstAccount)
                        .WithParentActivityOrWindow(parentActivity)
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                    System.Diagnostics.Debug.WriteLine($"Auth.Connect.AcquireTokenInteractive Success @ {stopWatch.ElapsedMilliseconds / 1000}");
                }
            }
            catch (MsalUiRequiredException exMsal)
            {
                System.Diagnostics.Debug.WriteLine($"Auth.Connect.exMsal @ {stopWatch.ElapsedMilliseconds / 1000}");
                throw exMsal;
            }
            catch (Microsoft.Identity.Client.MsalServiceException exMsal2)
            {
                System.Diagnostics.Debug.WriteLine($"Auth.Connect.exMsal2 @ {stopWatch.ElapsedMilliseconds / 1000}");
                if (exMsal2.Message.Contains("AADB2C90118") == true) //The user has forgotten their password.
                    await ResetPassword(AuthB2C);
                else if (exMsal2.Message.Contains("AADB2C90091") == true) //The user has cancelled entering self-asserted information.)
                    return false;
                else
                    throw exMsal2;
            }
            catch (Microsoft.Identity.Client.MsalClientException exMsal3)
            {
                // just cancelled, ignore?
                System.Diagnostics.Debug.WriteLine($"Auth.Connect.exMsal3 @ {stopWatch.ElapsedMilliseconds / 1000}");
                Debug.WriteLine(exMsal3.ToString());
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"Auth.Connect took {stopWatch.ElapsedMilliseconds / 1000} seconds");
            }

            // if we get this far, we have validated succesfully, set up the creds and decode the user claims
            this.creds = new TokenCredentials(authResult.IdToken);
            this.userFromClaims = new UserFromClaims(authResult.IdToken);

            return true;
        }
        
        public async Task EditProfile(Auth_VM AuthB2C)
        { 
            try
            {
                var accounts = await pca.GetAccountsAsync();
                var account = Helpers.GetAccountByPolicy(accounts, AuthB2C.PolicyEditProfile);

                var ar = await pca.AcquireTokenInteractive(this.scopes)
                    .WithAccount(account)
                    .WithB2CAuthority(AuthB2C.AuthorityEditProfile)
                    .WithParentActivityOrWindow(parentActivity)
                    .WithPrompt(Prompt.NoPrompt)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    throw ex;
            }
        }
        public async Task ResetPassword(Auth_VM AuthB2C)
        {
            try
            {
                var accounts = await pca.GetAccountsAsync();
                var account = Helpers.GetAccountByPolicy(accounts, AuthB2C.AuthorityResetPassword);

                var ar = await pca.AcquireTokenInteractive(this.scopes)
                    .WithAccount(account)
                    .WithB2CAuthority(AuthB2C.AuthorityResetPassword)
                    .WithParentActivityOrWindow(parentActivity)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    throw ex;
            }
        }
        public async Task SignOut()
        { 
            var accounts = await pca.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await pca.RemoveAsync(account);
            }
            this.authResult = null; //reset this

        }

        void CustomLoggingMethod(LogLevel level, string message, bool containsPii)
        {
            Console.WriteLine($"MSAL {level} {containsPii} {message}");
        }
    }
}


﻿using System;
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

        public Auth(Auth_VM AuthB2C, AppPlatform platform, string clientId, object parentActivity)
        {
            this.appPlatform = platform;
            this.clientId = AuthB2C.clientId;
            this.parentActivity = parentActivity;
            this.scopes = Auth_VM.GetScopes(AuthB2C.api_scopes_uri);

            string userDir = MyDocumentsRoot() + @"\";
            string RedirectUri = $@"msal{this.clientId}://auth";

            this.pca = PublicClientApplicationBuilder.Create(this.clientId)
                .WithB2CAuthority(AuthB2C.Authority)
                .WithIosKeychainSecurityGroup("com.microsoft.msalrocks")
                .WithRedirectUri(RedirectUri)
                .Build();

            Debug.WriteLine($@"Creating auth context with:
            B2C authority: {AuthB2C.Authority}
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

            System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.Start ");

            var accounts = await pca.GetAccountsAsync();

            System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.FirstAccount? @ {stopWatch.ElapsedMilliseconds / 1000}");
            var firstAccount = accounts.FirstOrDefault();
           
            try
            {
                if (isSilent)
                {
                    System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.AcquireTokenSilent @ {stopWatch.ElapsedMilliseconds / 1000}");
                    authResult = await pca.AcquireTokenSilent(this.scopes, firstAccount).ExecuteAsync(); 
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.SignOut @ {stopWatch.ElapsedMilliseconds / 1000}");
                    await SignOut();

                    System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.AcquireTokenInteractive @ {stopWatch.ElapsedMilliseconds / 1000}");
                    authResult = await pca.AcquireTokenInteractive(this.scopes)
                        .WithUseEmbeddedWebView(true)
                        .WithLoginHint(previousSignInName)
                        //.WithAccount(firstAccount)
                        .WithParentActivityOrWindow(parentActivity)
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                    System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.AcquireTokenInteractive Success @ {stopWatch.ElapsedMilliseconds / 1000}");
                }
            }
            catch (MsalUiRequiredException exMsal)
            {
                System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.exMsal @ {stopWatch.ElapsedMilliseconds / 1000}");
                throw exMsal;
            }
            catch (Microsoft.Identity.Client.MsalServiceException exMsal2)
            {
                System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.exMsal2 @ {stopWatch.ElapsedMilliseconds / 1000}");
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
                System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure.exMsal3 @ {stopWatch.ElapsedMilliseconds / 1000}");
                Debug.WriteLine(exMsal3.ToString());
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"B2i.MSALInterop.Auth.ConnectToAzure took {stopWatch.ElapsedMilliseconds / 1000} seconds");
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
                // var accounts = await pca.GetAccountsAsync(AuthB2C.PolicyEditProfile);
                // var account = accounts.FirstOrDefault();

                var accounts = await pca.GetAccountsAsync();
                var account = Helpers.GetAccountByPolicy(accounts, AuthB2C.PolicyEditProfile);

                // KNOWN ISSUE:
                // User will get prompted 
                // to pick an IdP again.


                // https://github.com/Azure-Samples/active-directory-b2c-xamarin-native/issues/35
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
                // AuthenticationResult ar = await pca.AcquireTokenAsync(AuthB2C.scopes, (IAccount)null, Prompt.SelectAccount, string.Empty, null, AuthB2C.AuthorityResetPassword, this.parentActivity);
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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MSALTesting
{
    public class Auth_VM : INotifyPropertyChanged
    {
        #region Properties / Bindings
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged is null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        string _clientId;
        public string clientId
        {
            get
            {
                return _clientId;
            }
            set
            {
                _clientId = value;
                RaisePropertyChanged(nameof(clientId));
            }
        }

        string _AzureADB2CHostname;
        public string AzureADB2CHostname
        {
            get
            {
                return _AzureADB2CHostname;
            }
            set
            {
                _AzureADB2CHostname = value;
                RaisePropertyChanged(nameof(AzureADB2CHostname));
            }
        }
        string _previousSignInName;
        public string previousSignInName
        {
            get
            {
                return _previousSignInName;
            }
            set
            {
                _previousSignInName = value;
                RaisePropertyChanged(nameof(previousSignInName));
            }
        }

        string _tenant;
        public string tenant
        {
            get
            {
                return _tenant;
            }
            set
            {
                _tenant = value;
                RaisePropertyChanged(nameof(tenant));
            }
        }


        string _PolicySignUpSignIn;
        public string PolicySignUpSignIn
        {
            get
            {
                return _PolicySignUpSignIn;
            }
            set
            {
                _PolicySignUpSignIn = value;
                RaisePropertyChanged(nameof(PolicySignUpSignIn));
            }
        }
        string _PolicyResetPassword;
        public string PolicyResetPassword
        {
            get
            {
                return _PolicyResetPassword;
            }
            set
            {
                _PolicyResetPassword = value;
                RaisePropertyChanged(nameof(PolicyResetPassword));
            }
        }
        string _PolicyEditProfile;
        public string PolicyEditProfile
        {
            get
            {
                return _PolicyEditProfile;
            }
            set
            {
                _PolicyEditProfile = value;
                RaisePropertyChanged(nameof(PolicyEditProfile));
            }
        }


        public string AuthorityBase 
        { 
            get 
            { 
                return $"https://{AzureADB2CHostname }/tfp/{tenant}/"; 
            } 
        }
        public string Authority
        {
            get
            {
                return $"{AuthorityBase}{PolicySignUpSignIn}";
            }
        }
        public string AuthorityEditProfile
        {
            get
            {
                return $"{AuthorityBase}{PolicyEditProfile}";
            }
        }
        public string AuthorityResetPassword
        {
            get
            {
                return $"{AuthorityBase}{PolicyResetPassword}";
            }
        }

        string _api_string;
        public string api_string
        {
            get
            {
                return _api_string;
            }
            set
            {
                _api_string = value;
                RaisePropertyChanged(nameof(api_string));
            }
        }

        string _bearer_token;
        public string bearer_token
        {
            get
            {
                return _bearer_token;
            }
            set
            {
                _bearer_token = value;
                RaisePropertyChanged(nameof(bearer_token));
            }
        }

        string _redirect_uri;
        public string redirect_uri
        {
            get
            {
                return _redirect_uri;
            }
            set
            {
                _redirect_uri = value;
                RaisePropertyChanged(nameof(redirect_uri));
            }
        }
    
        string _scopes_string;
        public string scopes_string
        {
            get
            {
                return _scopes_string;
            }
            set
            {
                _scopes_string = value;
                RaisePropertyChanged(nameof(scopes_string));
            }
        }
        #endregion

        public Auth auth { get; private set; }

        public Auth_VM()
        {
            this.clientId = "";
            this.tenant = "MyB2CTenant.onmicrosoft.com";
            this.AzureADB2CHostname = "MyB2CTenant.b2clogin.com"; // login.microsoftonline.com is deprecated, ref: https://docs.microsoft.com/en-us/azure/active-directory-b2c/b2clogin
            this.PolicyEditProfile = "B2C_1_edit_profile2";
            this.PolicyResetPassword = "B2C_1_Reset";
            this.PolicySignUpSignIn = "B2C_1_SignUpOrIn";

            this.api_string = "";
        }


        public async Task CreateAuth()
        {
            auth = new Auth(this.Authority, Auth.AppPlatform.DesktopClient, this.clientId, GetScopes(this.tenant, this.api_string), null);

            this.bearer_token = "";
            this.redirect_uri = auth.RedirectUri;

            if (auth.scopes is null)
                this.scopes_string = "** Scopes are NULL **";
            else 
                scopes_string = string.Join(Environment.NewLine, auth.scopes);


        }
        public async Task GetAuthResult(bool silently)
        {
            this.bearer_token = "";
            await this.auth.Connect(this, silently, this.previousSignInName);
            if (auth.authResult is null)
                return;

            this.bearer_token = auth.authResult.IdToken;
        }

        public static string[] GetScopes(string tenant, string api_string) 
        {
            string api_scopes_uri = $"https://{tenant}/{api_string}/";

            return new string[] { api_scopes_uri + "read", api_scopes_uri + "write" };
        }

        
    }
}

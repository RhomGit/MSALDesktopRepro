using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MSALTesting
{
    public class Auth_VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _api_scopes_uri; // derived from tenant 
        public string api_scopes_uri
        {
            get
            {
                return _api_scopes_uri;
            }
            set
            {
                _api_scopes_uri = value;
                RaisePropertyChanged(nameof(api_scopes_uri));
            }
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
                api_scopes_uri = $"https://{tenant}/";
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


        public Auth_VM()
        {
            this.clientId = "111";
            this.tenant = "MyB2CTenant.onmicrosoft.com";
            this.AzureADB2CHostname = "MyB2CTenant.b2clogin.com";
            this.PolicyEditProfile = "B2C_1_edit_profile2";
            this.PolicyResetPassword = "B2C_1_Reset";
            this.PolicySignUpSignIn = "B2C_1_SignUpOrIn";
        }

        public static string[] GetScopes(string api_scopes_uri)
        {
            return new string[] { api_scopes_uri + "read", api_scopes_uri + "write" };
        }

        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged is null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}

using Newtonsoft.Json.Linq;

namespace MSALTesting
{
    public class UserFromClaims
    {
        public string display_name;
        public string family_name;
        public string given_name; 
        public string oid;
        public string iss;
        public string email_str; 
        public JContainer email_arr; 
        public string ipaddr;

        public UserFromClaims(string token)
        {
            JObject user = Helpers.ParseIdToken(token);

            display_name = user["name"]?.ToString();
            family_name = user["family_name"]?.ToString();  // latest version, doesnt work?
            given_name = user["given_name"]?.ToString();  // latest version, doesnt work?
            oid = user["oid"]?.ToString();
            iss = user["iss"]?.ToString();
            email_str = user["email"]?.ToString();       // latest version, doesnt work?
            email_arr = (JContainer)user["emails"];  // latest version, doesnt work?
            ipaddr = user["ipaddr"]?.ToString(); //note: i dont cuurently do anything with this, but its cool and i should store it somewhere (not available in MSAL)

            if (email_str is null && email_arr != null && email_arr.Count > 0)
                email_str = string.Join(",", email_arr);

            if (email_str is null)
                email_str = display_name;
        }

    }
}

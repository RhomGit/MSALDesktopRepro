# A simple C# WPF repro for testing MSAL Azure B2C auth and getting a bearer token.

Update the constructor in Auth_VM.cs with your B2C params.

Click Auth to create the Auth object. This will generate:
- the default scopes (if you have an API specified) 
- the redirect uri.

Click either the interactive or silent button depending on how you want to auth.
This will send the scopes/redirect uri via MSAL to get you a bearer token. 

If auth success you should see a bearer token to copy.

![alt text](https://github.com/RhomGit/MSALDesktopRepro/blob/master/Capture2.PNG?raw=true)

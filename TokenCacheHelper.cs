// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization
using Microsoft.Identity.Client;
using System.IO;
using System.Security.Cryptography;

static class TokenCacheHelper
{
    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    /// <summary>
    /// Path to the token cache
    /// </summary>
    public static string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3"; // this is default, not useable tho, need to set to users directory

    private static readonly object FileLock = new object();


    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        lock (FileLock)
        {
            args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                                              null,
                                              DataProtectionScope.CurrentUser)
                    : null);
        }
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged)
        {
            lock (FileLock)
            {
                // reflect changesgs in the persistent store
                File.WriteAllBytes(CacheFilePath,
                                    ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                            null,
                                                            DataProtectionScope.CurrentUser)
                                    );
            }
        }
    }
}


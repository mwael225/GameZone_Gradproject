
#include "Internal/UaslibExport.h"

#include <stddef.h>

#if defined(__cplusplus)
extern "C"
{
#endif

    typedef void *UAS_AUTH_CLIENT;
    typedef void (*uaslib_authsignin_callback_fp)(bool successful, const char *token, const char *playerId,
                                                  int expiresIn, void *user_context);

    UASLIB_EXPORTED_SYMBOL UAS_AUTH_CLIENT Uaslib_Create_AuthenticationClient();
    UASLIB_EXPORTED_SYMBOL void Uaslib_Destroy_AuthenticationClient(UAS_AUTH_CLIENT client);

    /// <summary>
    /// Gets the value for the last received Unity Authentication Token
    /// </summary>
    /// <returns>The Unity Authentication Token</returns>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_GetToken(UAS_AUTH_CLIENT client, char *buf, size_t bufLen);

    /// <summary>
    /// Gets the PlayerId of the last received Unity Authentication Token
    /// </summary>
    /// <returns>The PlayerId</returns>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_GetPlayerId(UAS_AUTH_CLIENT client, char *buf, size_t bufLen);

    /// <summary>
    /// Sets the ProjectId to be used for Authentication Requests
    /// NOTE: Must be set properly before issuing Authentication Requests
    /// </summary>
    /// <param name="ProjectId">The Unity Project Id, available on the Unity Dashboard</param>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_SetProjectId(UAS_AUTH_CLIENT client, char *projectId);

    /// <summary>
    /// Sets the Environment Name to be used for Authentication Requests
    /// NOTE: Must be set properly before issuing Authentication Requests
    /// </summary>
    /// <param name="EnvironmentName">The Environment Name to be used, available on the Unity Dashboard</param>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_SetEnvironmentName(UAS_AUTH_CLIENT client, char *EnvironmentName);

    /// <summary>
    /// Signs the player in, favoring session token sign in if the session token is available to maintain continuity of
    /// UserId's.
    /// </summary>
    /// <param name="signInCallback">Callback</param>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_SignInAnonymously(UAS_AUTH_CLIENT client,
                                                                    uaslib_authsignin_callback_fp signin_cb,
                                                                    void *user_context);

    /// <summary>
    /// Attempts to sign the player in using the SessionToken, if it exists.
    /// </summary>
    /// <param name="signInCallback">Callback</param>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_RefreshToken(UAS_AUTH_CLIENT client,
                                                               uaslib_authsignin_callback_fp signin_cb,
                                                               void *user_context);

    /// <summary>
    /// Begins a thread that will intermittently check if the current Authentication Token has entered the final 25% of
    /// it's expiry time, attempting a RefreshToken when it does.
    /// </summary>
    /// <param name="RefreshCallback">The callback to be called whenever the RefreshThread calls RefreshToken</param>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_BeginAutoRefresh(UAS_AUTH_CLIENT client,
                                                                   uaslib_authsignin_callback_fp signin_cb,
                                                                   void *user_context);

    /// <summary>
    /// Ends the thread that is intermittently checking for the Token Refresh, if one exists.
    /// </summary>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_EndAutoRefresh(UAS_AUTH_CLIENT client);

    /// <summary>
    /// Whether or not the session token has been successfully stored at the end of a Sign In request.
    /// </summary>
    /// <returns></returns>
    UASLIB_EXPORTED_SYMBOL bool Uaslib_AuthClient_SessionTokenExists(UAS_AUTH_CLIENT client);

    /// <summary>
    /// Clears the session token, allowing for a new truly-anonymous sign in.
    /// </summary>
    UASLIB_EXPORTED_SYMBOL void Uaslib_AuthClient_ClearSessionToken(UAS_AUTH_CLIENT client);

#if defined(__cplusplus)
};
#endif

#pragma once

#include <iostream>

#include "AuthenticationResponses.h"

#include <chrono>
#include <functional>
#include <thread>

namespace unity {
namespace openapi {
class OpenAPIPlayerAuthenticationApi;
}
namespace uaslib {

using AuthenticationSignInCallback = std::function<void(const AuthenticationSignInResponse &)>;

class AuthenticationClient
{
public:
    AuthenticationClient();
    ~AuthenticationClient();

    /// <summary>
    /// Gets the value for the last received Unity Authentication Token
    /// </summary>
    /// <returns>The Unity Authentication Token</returns>
    custom_alloc::string GetToken() const;

    /// <summary>
    /// Gets the PlayerId of the last received Unity Authentication Token
    /// </summary>
    /// <returns>The PlayerId</returns>
    custom_alloc::string GetPlayerId() const;

    /// <summary>
    /// Sets the ProjectId to be used for Authentication Requests
    /// NOTE: Must be set properly before issuing Authentication Requests
    /// </summary>
    /// <param name="ProjectId">The Unity Project Id, available on the Unity Dashboard</param>
    void SetProjectId(const std::string_view ProjectId);

    /// <summary>
    /// Sets the Environment Name to be used for Authentication Requests
    /// NOTE: Must be set properly before issuing Authentication Requests
    /// </summary>
    /// <param name="EnvironmentName">The Environment Name to be used, available on the Unity Dashboard</param>
    void SetEnvironmentName(const std::string_view EnvironmentName);

    /// <summary>
    /// Signs the player in, favoring session token sign in if the session token is available to maintain continuity of
    /// UserId's.
    /// </summary>
    /// <param name="signInCallback">Callback</param>
    void SignInAnonymously(const AuthenticationSignInCallback &AuthCallback = AuthenticationSignInCallback());

    /// <summary>
    /// Attempts to sign the player in using the SessionToken, if it exists.
    /// </summary>
    /// <param name="signInCallback">Callback</param>
    void RefreshToken(const AuthenticationSignInCallback &AuthCallback = AuthenticationSignInCallback());

    /// <summary>
    /// Begins a thread that will intermittently check if the current Authentication Token has entered the final 25% of
    /// it's expiry time, attempting a RefreshToken when it does.
    /// </summary>
    /// <param name="RefreshCallback">The callback to be called whenever the RefreshThread calls RefreshToken</param>
    void BeginAutoRefresh(const AuthenticationSignInCallback &RefreshCallback = AuthenticationSignInCallback());

    /// <summary>
    /// Ends the thread that is intermittently checking for the Token Refresh, if one exists.
    /// </summary>
    void EndAutoRefresh();

    /// <summary>
    /// Whether or not the session token has been successfully stored at the end of a Sign In request.
    /// </summary>
    /// <returns></returns>
    bool SessionTokenExists() const;

    /// <summary>
    /// Clears the session token, allowing for a new truly-anonymous sign in.
    /// </summary>
    void ClearSessionToken();

    void SetURL(const std::string_view url);

private:
    bool IsClientValid() const;

    bool refreshing{false};
    custom_alloc::string m_token;
    custom_alloc::string m_sessionToken{""};
    std::chrono::system_clock::time_point m_tokenStart;
    int m_expTime;
    custom_alloc::string m_projectId{""};
    custom_alloc::string m_environmentName{""};
    custom_alloc::string m_playerId{""};

    std::unique_ptr<unity::openapi::OpenAPIPlayerAuthenticationApi> m_api;

    AuthenticationSignInCallback refreshCallback;

    std::thread refreshThread;

    void RefreshThread();
};
} // namespace uaslib
} // namespace unity
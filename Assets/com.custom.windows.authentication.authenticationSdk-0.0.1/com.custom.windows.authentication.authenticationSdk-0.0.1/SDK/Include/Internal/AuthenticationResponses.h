#pragma once

#include <iostream>

#include "AuthenticationPlayer.h"

namespace unity::uaslib {
struct AuthenticationSignInResponse
{
public:
    /// <summary>
    /// Whether or not the Authentication SignIn was successful
    /// </summary>
    /// <returns></returns>
    bool IsSuccessful() const
    {
        return Successful;
    }
    void SetSuccessful(bool success)
    {
        Successful = success;
    }
    /// <summary>
    /// The Token returned by the SignIn
    /// </summary>
    custom_alloc::string Token;
    /// <summary>
    /// The PlayerId associated with the Token
    /// </summary>
    custom_alloc::string PlayerId;
    /// <summary>
    /// The amount of time, in seconds, that the token will be valid from when it was produced
    /// </summary>
    int ExpiresIn;
    /// <summary>
    /// (NOT IMPLEMENTED)An AuthenticationPlayer with a vector of any External Id provider Id's
    /// </summary>
    AuthenticationPlayer Player;

private:
    bool Successful;
};
} // namespace unity::uaslib

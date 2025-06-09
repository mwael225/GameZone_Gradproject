#pragma once

#include <chrono>
#include <functional>
#include <iostream>
#include <thread>

#include "uaslib.h"

class AuthenticationSample
{
public:
    /// <summary>
    /// Sets the EnvironmentName and ProjectId of the Authentication Client, and builds the callback.
    /// </summary>
    void Initialize();

    void SampleLoop();

    unity::uaslib::AuthenticationClient client;

    unity::uaslib::AuthenticationSignInCallback cb;

    bool processing{false};

    bool refreshing{false};

    std::string command{""};
};
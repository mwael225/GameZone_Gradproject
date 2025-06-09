// OpenAPI.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "AuthenticationSample.h"

#define UNITY_PROJECT_ID       "3187a530-c1c9-4f99-9907-62d61774e6a0"
#define UNITY_ENVIRONMENT_NAME "production"

using namespace std::chrono_literals;

void AuthenticationSample::Initialize()
{
    client.SetEnvironmentName(UNITY_ENVIRONMENT_NAME);
    client.SetProjectId(UNITY_PROJECT_ID);

    auto &tmpProcessing = processing;
    cb = [&tmpProcessing](const unity::uaslib::AuthenticationSignInResponse &response) {
        std::cout << "Authentication Sign In Response:\n";
        std::cout << "Token: " << response.Token << "\n";
        std::cout << "PlayerId: " << response.PlayerId << "\n";
        std::cout << "ExpiresIn: " << response.ExpiresIn << "\n";
        tmpProcessing = false;
    };
}

void AuthenticationSample::SampleLoop()
{
    processing = true;
    client.SignInAnonymously(cb);
    while (true) {
        if (!processing) {
            std::cout << "Enter\n"
                         "- \"SignIn\"  to clear the session token, and sign in with a new PlayerId\n"
                         "- \"Refresh\" to refresh the current sign in, maintaining the PlayerId\n"
                         "- \"Auto\" to toggle the Auto Refreshing process,\nwhich will automatically refresh the "
                         "Authentication Token when "
                         "it reaches 75% of the way to it's expiry time\n"
                         "- \"List\" to list the current Token and PlayerId\n"
                         "- \"Exit\" to exit the sample app\n\n";
#if NO_STD_CIN
            // Just send a test command where cin is not supported
            command = "SignIn";
#else
            std::cin >> command;
#endif
        }

        if (command == "SignIn") {
            processing = true;
            client.ClearSessionToken();
            client.SignInAnonymously(cb);
        } else if (command == "Refresh") {
            processing = true;
            client.RefreshToken(cb);
        } else if (command == "Auto") {
            if (refreshing) {
                std::cout << "Ending Auto Refresh\n";
                client.EndAutoRefresh();
                refreshing = false;
            } else {
                std::cout << "Beginning Auto Refresh\n";
                client.BeginAutoRefresh();
                refreshing = true;
            }
        } else if (command == "List") {
            std::cout << "PlayerId: " << client.GetPlayerId() << "\n";
            std::cout << "Token: " << client.GetToken() << "\n";
        } else if (command == "Exit") {
            return;
        }
        command = "";
        std::this_thread::sleep_for(100ms);
    }
}

int main()
{
    AuthenticationSample sample;

    sample.Initialize();
    sample.SampleLoop();

    return 0;
}
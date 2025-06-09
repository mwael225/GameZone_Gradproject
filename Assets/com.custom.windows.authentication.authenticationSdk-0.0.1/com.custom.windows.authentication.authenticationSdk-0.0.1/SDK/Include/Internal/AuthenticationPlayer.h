#pragma once
#include "uaslib_custom_alloc.h"

namespace unity::uaslib {
struct PlatformId
{
    custom_alloc::string ExternalId;
    custom_alloc::string ProviderId;
};

struct AuthenticationPlayer
{
    /// <summary>
    /// (NOT IMPLEMENTED) A vector containing the Id's for any external ID provider for any platform signed in
    /// externally
    /// </summary>
    custom_alloc::vector<PlatformId> externalIds;
};
} // namespace unity::uaslib

#pragma once

#include "Internal/UaslibExport.h"

#include <cstdarg>
#include <functional>
#include <string_view>

namespace unity::uaslib::logging {
enum class LOG_LEVEL
{
    LOG_DISABLED,
    LOG_ERROR,
    LOG_WARNING,
    LOG_INFO,
    LOG_DEBUG
};

using pf_log_func = std::function<void(LOG_LEVEL level, std::string_view level_str, std::string_view msg)>;
UASLIB_EXPORTED_SYMBOL void set_log_function(pf_log_func logFunc);
UASLIB_EXPORTED_SYMBOL void set_log_level(LOG_LEVEL level);
} // namespace unity::uaslib::logging

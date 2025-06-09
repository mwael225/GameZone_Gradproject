#pragma once

#include "Internal/UaslibPlatformDetection.h"
#include <stdint.h>

namespace unity::uaslib {

enum class error_code
{
    Success = 0,
    InvalidFormat,
    InvalidArgument,
    OutOfMemory,
    InternalError,
};
using result_t = error_code; // for readability
constexpr bool IsFailure(result_t r)
{
    return (r != error_code::Success);
}

class void_method_result
{
public:
    void_method_result(result_t result = error_code::Success) : m_result(result)
    {
    }
    constexpr result_t get_result() const RETURNS_ERROR
    {
        return m_result;
    }

private:
    result_t m_result;
};

template<class T>
class method_result
{
public:
    method_result(result_t result = error_code::Success) : m_result(result)
    {
    }
    method_result(const void_method_result &result) : m_result(result.get_result())
    {
    }
    method_result(const T &data, bool force = true) : m_data(data)
    {
        (void)force;
    }
    result_t get_result(T *pData = 0) const RETURNS_ERROR
    {
        if (m_result == error_code::Success && pData != nullptr) {
            *pData = m_data;
        }
        return m_result;
    }
    void set_result(result_t value)
    {
        m_result = value;
    }

private:
    T m_data;
    result_t m_result = error_code::Success;
};
} // namespace unity::uaslib

namespace uaslib = unity::uaslib;

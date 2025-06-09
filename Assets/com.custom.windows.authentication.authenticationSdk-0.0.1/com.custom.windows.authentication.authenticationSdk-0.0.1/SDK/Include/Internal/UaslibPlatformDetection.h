#pragma once

// Detect UASLIB_PLATFORM_X define.
//
// Note that PLATFORM_X defines in Unity code base may refer to one or more platforms defined by UASLIB_PLATFORM_X
// Platforms here are very loosely defined on the set of available system apis.
// They have closest relation with the platform toolchains defined in Bee.

#if defined(__NX__)
    #define UASLIB_PLATFORM_SWITCH 1
#elif defined __ORBIS__
    #define UASLIB_PLATFORM_PS4 1
#elif defined __PROSPERO__
    #define UASLIB_PLATFORM_PS5 1
#elif defined __EMSCRIPTEN__
    #define UASLIB_PLATFORM_EMSCRIPTEN 1
#elif defined __wasi__
    #define UASLIB_PLATFORM_WASI 1
#elif defined(__APPLE__)
    #include <TargetConditionals.h>
    #if !defined(TARGET_OS_XR)
        #define TARGET_OS_XR 0
    #endif
    #if !defined(TARGET_OS_VISION)
        #define TARGET_OS_VISION 0
    #endif
    #if TARGET_OS_XR || TARGET_OS_VISION
        #define UASLIB_PLATFORM_VISIONOS 1
    #elif TARGET_OS_IOS
        #define UASLIB_PLATFORM_IOS 1
    #elif TARGET_OS_TV
        #define UASLIB_PLATFORM_TVOS 1
    #elif TARGET_OS_OSX || TARGET_OS_MAC
        #define UASLIB_PLATFORM_MACOS 1
    #endif
#elif defined(__NetBSD__)
    #define UASLIB_PLATFORM_NETBSD 1
#elif defined(linux) || defined(__linux__)
    #if defined(ANDROID) || defined(__ANDROID__)
        #define UASLIB_PLATFORM_ANDROID 1
    #elif defined(EMBEDDED_LINUX)
        #define UASLIB_PLATFORM_EMBEDDED_LINUX 1
    #else
        #define UASLIB_PLATFORM_LINUX 1
    #endif
#elif defined(_WIN32) || defined(__WIN32__)
    #include <winapifamily.h>
    #if (defined(WINAPI_FAMILY_GAMES) && (WINAPI_FAMILY == WINAPI_FAMILY_GAMES))
        #define UASLIB_PLATFORM_WINDOWSGAMES 1
    #elif WINAPI_FAMILY == WINAPI_FAMILY_APP
        #define UASLIB_PLATFORM_WINRT 1
    #else
        #define UASLIB_PLATFORM_WINDOWS 1
    #endif
#elif defined(__QNX__)
    #define UASLIB_PLATFORM_QNX 1
#endif

#if __GNUC__ > 4 || (__GNUC__ == 4 && (__GNUC_MINOR__ >= 1))
    #define RETURNS_ERROR __attribute__((warn_unused_result))
#else
    #define RETURNS_ERROR
#endif

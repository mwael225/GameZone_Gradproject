#ifdef _MSC_VER
    #ifdef UASLIB_DYNAMICLIBRARY
        #ifdef BUILDING_UASLIB
            #define UASLIB_EXPORTED_SYMBOL __declspec(dllexport)
        #else
            #define UASLIB_EXPORTED_SYMBOL __declspec(dllimport)
        #endif
    #else
        #define UASLIB_EXPORTED_SYMBOL
    #endif
#else
    #define UASLIB_EXPORTED_SYMBOL __attribute__((visibility("default")))
#endif

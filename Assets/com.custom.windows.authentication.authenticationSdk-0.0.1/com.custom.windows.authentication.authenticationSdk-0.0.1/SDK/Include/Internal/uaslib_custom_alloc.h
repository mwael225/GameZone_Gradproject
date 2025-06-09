#pragma once

#include "Internal/UaslibExport.h"

#include <array>
#include <charconv>
#include <deque>
#include <limits>
#include <list>
#include <map>
#include <memory>
#include <queue>
#include <set>
#include <sstream>
#include <stack>
#include <stddef.h>
#include <stdint.h>
#include <string>
#include <type_traits>
#include <unordered_map>
#include <unordered_set>
#include <utility>
#include <vector>

#ifdef _WIN32
    #ifdef max
        #undef max // If Windows.h has already defined this, get rid of it so that std::max will work correctly
    #endif
    #ifndef NOMINMAX
        #define NOMINMAX // In case Windows.h is included later, prevent max macro from being defined
    #endif
#endif

namespace unity::uaslib {
namespace custom_alloc {
using pf_malloc_func = std::add_pointer_t<void *(size_t)>;
using pf_malloc_aligned_func = std::add_pointer_t<void *(size_t alignment, size_t size)>;
using pf_realloc_func = std::add_pointer_t<void *(void *ptr, size_t new_size)>;
using pf_calloc_func = std::add_pointer_t<void *(size_t num, size_t size)>;
using pf_free_func = std::add_pointer_t<void(void *)>;
void set_alloc_funcs(pf_malloc_func p_malloc, pf_malloc_aligned_func p_aligned_malloc, pf_realloc_func p_realloc,
                     pf_calloc_func p_calloc, pf_free_func p_free, pf_free_func p_aligned_free);

UASLIB_EXPORTED_SYMBOL void *alloc(size_t size);
UASLIB_EXPORTED_SYMBOL void *alloc_aligned(size_t alignment, size_t size);
template<class T>
T *alloc(size_t count = 1)
{
    return static_cast<T *>(alloc(sizeof(T) * count));
}

UASLIB_EXPORTED_SYMBOL void *realloc(void *ptr, size_t new_size);
UASLIB_EXPORTED_SYMBOL void *calloc(size_t num, size_t size);

UASLIB_EXPORTED_SYMBOL void free(void *p);
UASLIB_EXPORTED_SYMBOL void free_aligned(void *p);

UASLIB_EXPORTED_SYMBOL char *strdup(const char *str);

class object
{
public:
    static void *operator new(size_t size, void *ref);
    static void *operator new(size_t size);
    static void operator delete(void *p);
    static void *operator new[](size_t size);
    static void operator delete[](void *p);
};

// Allocator to be used with STL classes that satisfies the C++ Allocator named requirements
template<typename T>
struct allocator
{
    typedef T value_type;
    typedef T *pointer;
    typedef const T *const_pointer;
    typedef T &reference;
    typedef const T &const_reference;
    typedef size_t size_type;
    typedef ptrdiff_t difference_type;

    template<typename U>
    struct rebind
    {
        typedef allocator<U> other;
    };

    allocator()
    {
    }

    template<typename U>
    allocator(const allocator<U> &other)
    {
        (void)other;
    }

    // Allocates storage for n objects of type T but do not construct
    T *allocate(size_t n)
    {
        return static_cast<T *>(uaslib::custom_alloc::alloc(n * sizeof(T)));
    }

    // Deallocates storage for n objects of type T but do not destruct
    void deallocate(T *p, size_t n)
    {
        uaslib::custom_alloc::free(p);
        (void)n;
    }

    static size_type max_size()
    {
        return std::numeric_limits<size_type>::max() / sizeof(T);
    }

    // Returns true iff the storage allocated by this can be deallocated by other
    bool operator==(const allocator &other) const
    {
        (void)other;
        return true;
    }

    bool operator!=(const allocator &other) const
    {
        return !(*this == other);
    }
};

template<typename T, typename... Args>
std::shared_ptr<T> make_shared(Args &&...args)
{
    return std::allocate_shared<T>(allocator<T>(), std::forward<Args>(args)...);
}

template<typename T>
std::unique_ptr<T> make_unique()
{
    return std::unique_ptr<T>(new T);
}

template<typename T, typename... Args>
std::unique_ptr<T> make_unique(Args &&...args)
{
    return std::unique_ptr<T>(new T(std::forward<Args>(args)...));
}

template<class T>
using deque = std::deque<T, allocator<T>>;

template<class K, class T>
using map = std::map<K, T, std::less<K>, allocator<std::pair<const K, T>>>;

template<class K, class T>
using multimap = std::multimap<K, T, std::less<K>, allocator<std::pair<const K, T>>>;

template<class T>
using queue = std::queue<T, deque<T>>;

template<class K, class T>
using unordered_map = std::unordered_map<K, T, std::hash<K>, std::equal_to<K>, allocator<std::pair<const K, T>>>;

template<class K>
using unordered_set = std::unordered_set<K, std::hash<K>, std::equal_to<K>, allocator<K>>;

template<class T>
using set = std::set<T, std::less<T>, allocator<T>>;

template<class T>
using stack = std::stack<T, deque<T>>;

template<class T>
using set = std::set<T, std::less<T>, allocator<T>>;

template<class T>
using list = std::list<T, allocator<T>>;

using string = std::basic_string<char, std::char_traits<char>, allocator<char>>;

using stringstream = std::basic_stringstream<char, std::char_traits<char>, allocator<char>>;

using istringstream = std::basic_istringstream<char, std::char_traits<char>, allocator<char>>;

using ostringstream = std::basic_ostringstream<char, std::char_traits<char>, allocator<char>>;

template<class T>
using vector = std::vector<T, allocator<T>>;
} // namespace custom_alloc
} // namespace unity::uaslib

namespace uaslib = unity::uaslib;

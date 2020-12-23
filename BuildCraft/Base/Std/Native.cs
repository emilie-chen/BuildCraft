using System.Runtime.InteropServices;
using System.Security;

namespace BuildCraft.Base.Std
{
    public static unsafe class Native
    {
        [DllImport("libSystem.dylib", EntryPoint = "malloc"), SuppressUnmanagedCodeSecurity]
        public static extern void* malloc(size_t size);
        
        [DllImport("libSystem.dylib", EntryPoint = "calloc"), SuppressUnmanagedCodeSecurity]
        public static extern void* calloc(size_t count, size_t size);
        
        [DllImport("libSystem.dylib", EntryPoint = "realloc"), SuppressUnmanagedCodeSecurity]
        public static extern void* realloc(void* ptr, size_t size);
        
        [DllImport("libSystem.dylib", EntryPoint = "free"), SuppressUnmanagedCodeSecurity]
        public static extern void free(void* ptr);
        
        [DllImport("libSystem.dylib", EntryPoint = "memset"), SuppressUnmanagedCodeSecurity]
        public static extern void memset(void* ptr, int value, size_t num);
        
        [DllImport("libSystem.dylib", EntryPoint = "memcpy"), SuppressUnmanagedCodeSecurity]
        public static extern void memcpy(void* dest, void* src, size_t size);
    }
}
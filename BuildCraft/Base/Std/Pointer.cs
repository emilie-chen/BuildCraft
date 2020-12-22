using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BuildCraft.Base.Std;

namespace BuildCraft.Base.Std
{
    public struct size_t
    {
        public ulong Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public size_t(ulong value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(ulong value)
        {
            return new size_t(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(size_t value)
        {
            return value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(nuint value)
        {
            return new size_t(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nuint(size_t value)
        {
            return (nuint) value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(int value)
        {
            return new size_t((ulong) value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(long value)
        {
            return new size_t((ulong) value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(size_t value)
        {
            return (int) value.Value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator uint(size_t value)
        {
            return (uint) value.Value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator size_t(uint value)
        {
            return new size_t(value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static size_t operator -(size_t lhs, size_t rhs)
        {
            return lhs.Value - rhs.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static size_t operator +(size_t lhs, size_t rhs)
        {
            return lhs.Value + rhs.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long operator -(size_t lhs)
        {
            return -(long) lhs.Value;
        }
    }

    // USE WITH CARE
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public unsafe struct VoidPointer
    {
        public void* Value;
        public static readonly void* NULL = (void*) 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public VoidPointer(void* ptr)
        {
            Value = ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator void*(VoidPointer boxedPtr)
        {
            return boxedPtr.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static implicit operator VoidPointer(void* ptr)
        {
            return new(ptr);
        }
    }
}
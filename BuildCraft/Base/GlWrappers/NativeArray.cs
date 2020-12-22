using System;
using System.Runtime.CompilerServices;
using BuildCraft.Base.Std;
using static BuildCraft.Base.Std.Native;

namespace BuildCraft.Base.GlWrappers
{
    public unsafe class NativeArray<T> : IDisposable where T : unmanaged
    {
        private readonly size_t m_Count;
        private T* m_Data;

        public size_t Size => m_Count;
        public T* Data => m_Data;
        
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get => m_Data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set => m_Data[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ref T At(int index)
        {
            return ref m_Data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public NativeArray(size_t count)
        {
            m_Count = count;
            m_Data = null;
            while (m_Data == null)
            {
                m_Data = (T*) malloc(m_Count * sizeof(T));
            }
        }

        private void ReleaseUnmanagedResources()
        {
            if (m_Data == null) return;
            free(m_Data);
            m_Data = null;
        }
        
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~NativeArray()
        {
            ReleaseUnmanagedResources();
        }
    }
}
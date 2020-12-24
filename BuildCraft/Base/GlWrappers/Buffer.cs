using System;
using System.Collections.Generic;
using System.Diagnostics;
using BuildCraft.Base.Std;
using Silk.NET.OpenGL;
using static BuildCraft.Base.OpenGLContext;

namespace BuildCraft.Base.GlWrappers
{
    public enum ShaderDataType : uint
    {
        None = 0,
        Float,
        Float2,
        Float3,
        Float4,
        Mat3,
        Mat4,
        Int,
        Int2,
        Int3,
        Int4,
        Bool
    }

    public static class ShaderDataTypeHelper
    {
        public static uint ShaderDataTypeSize(ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType.Float:
                case ShaderDataType.Int: return 4;
                case ShaderDataType.Float2:
                case ShaderDataType.Int2: return 4 * 2;
                case ShaderDataType.Float3:
                case ShaderDataType.Int3: return 4 * 3;
                case ShaderDataType.Float4:
                case ShaderDataType.Int4: return 4 * 4;
                case ShaderDataType.Mat3: return 4 * 3 * 3;
                case ShaderDataType.Mat4: return 4 * 4 * 4;
                case ShaderDataType.Bool: return 1;
                default:
                    Debug.Assert(false, "Unknown shader data type");
                    break;
            }

            return 0;
        }
    }

    public struct BufferElement
    {
        public string Name;
        public ShaderDataType Type;
        public uint Size;
        public uint Offset;
        public bool Normalized;

        public BufferElement(ShaderDataType type, string name, bool normalized = false)
        {
            Name = name;
            Type = type;
            Size = ShaderDataTypeHelper.ShaderDataTypeSize(type);
            Offset = 0;
            Normalized = normalized;
        }

        public uint GetComponentCount()
        {
            switch (Type)
            {
                case ShaderDataType.Float:
                case ShaderDataType.Int: return 1;
                case ShaderDataType.Float2:
                case ShaderDataType.Int2: return 2;
                case ShaderDataType.Float3:
                case ShaderDataType.Int3: return 3;
                case ShaderDataType.Float4:
                case ShaderDataType.Int4: return 4;
                case ShaderDataType.Mat3: return 3 * 3;
                case ShaderDataType.Mat4: return 4 * 4;
                case ShaderDataType.Bool: return 1;
            }

            Debug.Assert(false);
            return 0;
        }
    }

    public struct BufferLayout
    {
        public BufferLayout(IEnumerable<BufferElement> elements)
        {
            m_Elements = new List<BufferElement>(elements);
            m_Stride = 0;
            CalculateOffsetsAndStride();
        }

        public IList<BufferElement> GetElements()
        {
            return m_Elements;
        }

        public uint GetStride()
        {
            return m_Stride;
        }

        public IEnumerator<BufferElement> GetEnumerator()
        {
            return m_Elements.GetEnumerator();
        }

        private void CalculateOffsetsAndStride()
        {
            uint offset = 0;
            m_Stride = 0;

            for (int i = 0; i < m_Elements.Count; i++)
            {
                BufferElement element = m_Elements[i];
                element.Offset = offset;
                m_Elements[i] = element;
                offset += m_Elements[i].Size;
                m_Stride += m_Elements[i].Size;
            }
        }

        private readonly IList<BufferElement> m_Elements;
        private uint m_Stride;
    }

    public class VertexBuffer : IDisposable, IBindable
    {
        private uint m_RendererID;
        private BufferLayout m_BufferLayout;

        public BufferLayout Layout
        {
            get => m_BufferLayout;
            set => m_BufferLayout = value;
        }

        public uint RendererID => m_RendererID;

        public VertexBuffer()
        {
            m_RendererID = Gl.GenBuffer();
        } 

        public unsafe VertexBuffer(size_t size)
        {
            m_RendererID = Gl.GenBuffer();
            Bind();
            Gl.BufferData(BufferTargetARB.ArrayBuffer, size, null, BufferUsageARB.DynamicDraw);
        }

        public unsafe VertexBuffer(float* vertices, size_t size)
        {
            m_RendererID = Gl.GenBuffer();
            Bind();
            Gl.BufferData(BufferTargetARB.ArrayBuffer, size, vertices, BufferUsageARB.StaticDraw);
        }

        ~VertexBuffer()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            Gl.DeleteBuffer(m_RendererID);
            m_RendererID = 0; 
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void Bind()
        {
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, m_RendererID);
        }

        public void Unbind()
        {
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public unsafe void SetData(void* data, size_t size)
        {
            Bind();
            Gl.BufferSubData(BufferTargetARB.ArrayBuffer, (nint) 0, (nuint) size, data);
        }
    }

    public class IndexBuffer : IDisposable, IBindable
    {
        private uint m_RendererID;
        private uint m_Count;

        public uint Count => m_Count;

        /// <summary>
        /// Creating an index buffer with the data and count
        /// </summary>
        /// <param name="indices">data</param>
        /// <param name="count">NUMBER OF ELEMENTS, NOT SIZE</param>
        public unsafe IndexBuffer(uint* indices, size_t count)
        {
            m_Count = count;
            m_RendererID = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_RendererID);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (size_t) (count * sizeof(uint)), indices,
                GLEnum.StaticDraw);
        }

        public void Bind()
        {
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, m_RendererID);
        }

        public void Unbind()
        {
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        private void ReleaseUnmanagedResources()
        {
            Gl.DeleteBuffer(m_RendererID);
            m_RendererID = 0;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~IndexBuffer()
        {
            ReleaseUnmanagedResources();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Silk.NET.OpenGL;
using static BuildCraft.Base.OpenGLContext;

namespace BuildCraft.Base.GlWrappers
{
    public class VertexArray : IDisposable, IBindable
    {
        private uint m_RendererID;
        private IList<VertexBuffer> m_VertexBuffers;
        private IndexBuffer m_IndexBuffer;

        public static GLEnum ShaderDataTypeToOpenGLBaseType(ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType.Float:
                case ShaderDataType.Float2:
                case ShaderDataType.Float3:
                case ShaderDataType.Float4:
                case ShaderDataType.Mat3:
                case ShaderDataType.Mat4:
                    return GLEnum.Float;
                case ShaderDataType.Int:
                case ShaderDataType.Int2:
                case ShaderDataType.Int3:
                case ShaderDataType.Int4:
                    return GLEnum.Int;
                case ShaderDataType.Bool:
                    return GLEnum.Bool;
                default:
                    break;
            }

            return 0;
        }

        public unsafe VertexArray()
        {
            m_RendererID = Gl.GenVertexArray();
            m_VertexBuffers = new List<VertexBuffer>();
        }

        public void Bind()
        {
            Gl.BindVertexArray(m_RendererID);
        }

        public void Unbind()
        {
            Gl.BindVertexArray(0);
        }

        private void ReleaseUnmanagedResources()
        {
            Gl.DeleteVertexArray(m_RendererID);
            m_RendererID = 0;
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (!disposing) return;
            m_IndexBuffer?.Dispose();
            foreach (VertexBuffer buffer in m_VertexBuffers)
            {
                buffer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~VertexArray()
        {
            Dispose(false);
        }

        public unsafe void AddVertexBuffer(VertexBuffer vertexBuffer)
        {
            Debug.Assert(vertexBuffer.Layout.GetElements().Count != 0, "Vertex Buffer has no layout!");

            Bind();
            vertexBuffer.Bind();

            uint index = 0;
            BufferLayout layout = vertexBuffer.Layout;
            foreach (BufferElement element in layout)
            {
                Gl.EnableVertexAttribArray(index);
                Gl.VertexAttribPointer(
                    index,
                    (int) element.GetComponentCount(),
                    ShaderDataTypeToOpenGLBaseType(element.Type),
                    element.Normalized,
                    layout.GetStride(),
                    (void*) element.Offset);
                index++;
            }

            m_VertexBuffers.Add(vertexBuffer);
        }

        public void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            Bind();
            indexBuffer.Bind();
            m_IndexBuffer = indexBuffer;
        }
    }
}
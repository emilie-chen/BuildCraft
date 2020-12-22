using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static BuildCraft.Base.OpenGLContext;

namespace BuildCraft.Base.GlWrappers
{
    using Mat4 = Matrix4X4<float>;
    using Mat3 = Matrix3X3<float>;
    using Vec4 = Vector4D<float>;
    using Vec3 = Vector3D<float>;
    using Vec2 = Vector2D<float>;

    public class Shader : IBindable, IDisposable
    {
        public string Name => m_Name;

        private uint m_RendererID;
        private string m_Name;

        public Shader(string name, string vertexSrc, string fragmentSrc)
        {
            m_Name = name;
            CompileShaders(vertexSrc, fragmentSrc);
        }

        private void CompileShaders(string vertexSrc, string fragmentSrc)
        {
            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, vertexSrc);
            Gl.CompileShader(vertexShader);

            string infoLog = Gl.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }

            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, fragmentSrc);
            Gl.CompileShader(fragmentShader);

            infoLog = Gl.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling fragment shader {infoLog}");
            }

            m_RendererID = Gl.CreateProgram();
            Bind();
            Gl.AttachShader(m_RendererID, vertexShader);
            Gl.AttachShader(m_RendererID, fragmentShader);
            Gl.LinkProgram(m_RendererID);

            Gl.GetProgram(m_RendererID, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(m_RendererID)}");
            }

            Gl.DetachShader(m_RendererID, vertexShader);
            Gl.DetachShader(m_RendererID, fragmentShader);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);
        }

        public void Bind()
        {
            Gl.UseProgram(m_RendererID);
        }

        public void Unbind()
        {
            Gl.UseProgram(0);
        }

        public unsafe void UploadUniformMat4(string name, Mat4 matrix)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.UniformMatrix4(location, 1, true, (float*) &matrix);
        }

        public unsafe void UploadUniformMat3(string name, Mat3 matrix)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.UniformMatrix3(location, 1, true, (float*) &matrix);
        }

        public void UploadUniformFloat4(string name, Vec4 values)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform4(location, values.X, values.Y, values.Z, values.W);
        }

        public void UploadUniformFloat3(string name, Vec3 values)
        {            
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform3(location, values.X, values.Y, values.Z);
        }

        public void UploadUniformFloat2(string name, Vec2 values)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform2(location, values.X, values.Y);
        }

        public void UploadUniformFloat(string name, float value)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform1(location, value);
        }

        public void UploadUniformInt(string name, int value)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform1(location, value);
        }

        public unsafe void UploadUniformIntArray(string name, int* values, uint count)
        {
            Bind();
            int location = Gl.GetUniformLocation(m_RendererID, name);
            Debug.Assert(location != -1, "Uniform not found");
            Gl.Uniform1(location, count, values);
        }

        private void ReleaseUnmanagedResources()
        {
            Gl.DeleteProgram(m_RendererID);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~Shader()
        {
            ReleaseUnmanagedResources();
        }
    }
}
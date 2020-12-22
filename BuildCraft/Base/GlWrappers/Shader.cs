using System;
using System.Collections.Generic;
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

        // void UploadUniformMat4(string name, const glm::mat4& matrix);
        // void UploadUniformMat3(string name, const glm::mat3& matrix);
        // void UploadUniformFloat4(string name, const glm::vec4& values);
        // void UploadUniformFloat3(string name, const glm::vec3& values);
        // void UploadUniformFloat2(string name, const glm::vec2& values);
        // void UploadUniformFloat(string name, float value);
        // void UploadUniformInt(string name, int value);
        // void UploadUniformIntArray(string name, int* values, unsigned count);

        // void SetMat4(string name, const glm::mat4& values) override;
        // void SetFloat4(string name, const glm::vec4& values) override;
        // void SetFloat3(string name, const glm::vec3& values) override;
        // void SetInt(string name, int value) override;
        // void SetIntArray(string name, int* values, unsigned count) override;
        // void SetFloat(string name, float value) override;
        
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
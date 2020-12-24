using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using Silk.NET.OpenGL;
using static BuildCraft.Base.OpenGLContext;
using static BuildCraft.Base.Std.Native;

namespace BuildCraft.Game.Renderer
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;
    
    public class CubeRenderer : IDisposable
    {
        private static readonly string VertexShaderSource = File.ReadAllText("Assets/Shaders/Test/Shader.vert");
        private static readonly string FragmentShaderSource = File.ReadAllText("Assets/Shaders/Test/Shader.frag");
        
        private VertexArray m_VertexArray;
        private VertexBuffer m_VertexBuffer;
        private IndexBuffer m_IndexBuffer;
        private Shader m_Shader;
        private Camera m_Camera;

        public unsafe CubeRenderer()
        {
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            m_VertexArray = new VertexArray();
            m_VertexArray.Bind();

            float* vertices = stackalloc float[]
            {
                // X    Y      Z     U   V
                // back
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,

                // front
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,

                // left
                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

                // right
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

                // bottom
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

                // top
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            };
            m_VertexBuffer = new VertexBuffer(vertices, (3 + 2) * 4 * 6 * sizeof(float));
            m_VertexBuffer.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "a_Pos"),
                new(ShaderDataType.Float2, "a_TexCoord")
            });
            
            uint* standardDrawingOrder = stackalloc uint[6]
            {
                0, 1, 2,
                2, 3, 0
            };

            uint* indices = stackalloc uint[36];
            for (uint i = 0; i < 36; i += 6)
            {
                memcpy(indices + i, standardDrawingOrder, 6 * sizeof(uint));
                for (int j = 0; j < 6; j++)
                {
                    standardDrawingOrder[j] += 4;
                }
            }

            m_IndexBuffer = new IndexBuffer(indices, 36 * sizeof(uint));
            
            m_VertexArray.AddVertexBuffer(m_VertexBuffer);
            m_VertexArray.SetIndexBuffer(m_IndexBuffer);
            m_VertexArray.Unbind();
            m_VertexBuffer.Unbind();
            m_IndexBuffer.Unbind();
            
            m_Shader = new Shader("MainShader", VertexShaderSource, FragmentShaderSource);
            
            m_Shader.Unbind();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void BeginScene(Camera camera)
        {
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            m_Camera = camera;
            m_VertexArray.Bind();
            m_Shader.Bind();
            Mat4 viewMatrix = m_Camera.CalculateViewMatrix();
            Mat4 projectionMatrix = Mat4.CreatePerspectiveFieldOfView(
                MathF.PI / 4.0f,
                (float) GlWindow.Size.X / GlWindow.Size.Y,
                0.1f,
                100.0f
            );
            m_Shader.UploadUniformMat4("u_View", viewMatrix);
            m_Shader.UploadUniformMat4("u_Projection", projectionMatrix);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void RenderCube(Vec3 coordinates, Texture texture)
        {
            m_Shader.UploadUniformMat4("u_Model", Mat4.CreateTranslation(coordinates));
            texture.Bind();
            m_Shader.UploadUniformInt("u_Texture", 0);
            Gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, null);
        }

        public void EndScene()
        {
            m_Camera = null;
        }

        public void Dispose()
        {
            m_VertexArray?.Dispose();
            m_VertexBuffer?.Dispose();
            m_IndexBuffer?.Dispose();
            m_Shader?.Dispose();
        }
    }
}
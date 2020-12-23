using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Base.Std;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static BuildCraft.Base.OpenGLContext;
using static BuildCraft.Base.Std.Native;

namespace BuildCraft.Game
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;


    public unsafe class Application
    {
        // TODO move to renderer
        private static readonly string VertexShaderSource = File.ReadAllText("Assets/Shaders/Test/Shader.vert");
        private static readonly string FragmentShaderSource = File.ReadAllText("Assets/Shaders/Test/Shader.frag");

        private static VertexArray vao;
        private static VertexBuffer vbo;
        private static IndexBuffer ibo;
        private static Shader shader;
        private static Texture tex;


        private static void Main(string[] args)
        {
            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            Init(options, OnLoad, OnUpdate, OnRender, OnClose);
            RunWindow();
        }

        private static void OnLoad()
        {
            IInputContext input = GlWindow.CreateInput();
            foreach (IKeyboard t in input.Keyboards)
            {
                t.KeyDown += KeyDown;
            }

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            vao = new VertexArray();
            vao.Bind();

            float* vertices = stackalloc float[]
            {
                // //X    Y      Z
                // -1.0f, -1.0f, -1.0f, 0.0f, 1.0f, // bottom left away from me 0
                // -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, // bottom left towards me    1
                // 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, // bottom right away from me 2
                // 1.0f, -1.0f, 1.0f, 1.0f, 0.0f, // bottom right towards me    3
                // -1.0f, 1.0f, -1.0f, 1.0f, 1.0f, // top left away from me     4
                // -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, // top left towards me        5
                // 1.0f, 1.0f, -1.0f, 0.0f, 1.0f, // top right away from me     6
                // 1.0f, 1.0f, 1.0f, 0.0f, 0.0f // top right towards me         7
                
                //X    Y      Z     U   V
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
            vbo = new VertexBuffer(vertices, (3 + 2) * 4 * 6 * sizeof(float));


            vbo.Layout = new BufferLayout(new BufferElement[]
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

            ibo = new IndexBuffer(indices, 36 * sizeof(uint));
            // ibo = new IndexBuffer(indicies, 2 * 3 * sizeof(uint));
            
            vao.AddVertexBuffer(vbo);
            vao.SetIndexBuffer(ibo);
            vao.Unbind();
            vbo.Unbind();
            ibo.Unbind();

            shader = new Shader("MainShader", VertexShaderSource, FragmentShaderSource);
            // Mat4 projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathF.PI / 4.0f, 8.0f / 6.0f, 0.1f, 100.0f);
            // shader.UploadUniformMat4("u_ViewProjection", new (
            //     1.0f, 0.0f, 0.0f, 0.0f,
            //     0.0f, 2.0f, 0.0f, 0.0f,
            //     0.0f, 0.0f, 1.0f, 0.0f,
            //     0.0f, 0.0f, 0.0f, 1.0f
            // ));
            // shader.UploadUniformFloat4("u_Color", new(1.0f, 0.0f, 0.0f, 1.0f));

            shader.Unbind();

            tex = new Texture("Assets/Textures/testimg.png");
            shader.Bind();
            tex.Bind();
            shader.UploadUniformInt("u_Texture", 0);
            shader.Unbind();
        }

        private static unsafe void OnRender(double obj)
        {
            //Clear the color channel.
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            //Bind the geometry and shader.
            vao.Bind();
            shader.Bind();
            //Draw the geometry.
            Gl.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, null);
        }

        private static void OnUpdate(double ts)
        {
            MainCamera.Update((float) ts);
            Mat4 viewMatrix = MainCamera.CalculateViewMatrix();
            Mat4 projectionMatrix = Mat4.CreatePerspectiveFieldOfView(
                MathF.PI / 4.0f,
                (float) GlWindow.Size.X / GlWindow.Size.Y,
                0.1f,
                100.0f
            );
            shader.UploadUniformMat4("u_View", viewMatrix);
            shader.UploadUniformMat4("u_Model", Mat4.Identity);
            shader.UploadUniformMat4("u_Projection", projectionMatrix);
        }

        private static void OnClose()
        {
            //Remember to delete the buffers.
            vao.Dispose();
            shader.Dispose();
            tex.Dispose();
        }

        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                GlWindow.Close();
            }
        }
    }
}
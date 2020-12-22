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
        //Vertex shaders are run on each vertex.
        private static readonly string VertexShaderSource = @"
        #version 330 core //Using version GLSL version 3.3
uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;
layout (location = 0) in vec3 a_Pos;
out vec4 v_Pos;
out vec2 v_TexCoord;
void main() {
    gl_Position = u_Projection * u_View * u_Model * vec4(a_Pos, 1.0f);
    v_Pos = vec4(clamp(a_Pos.xyz, 0.0f, 1.0f), 1.0f);
    //v_TexCoord = a_TexCoord;
}
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
        #version 330 core

uniform sampler2D u_Texture;
uniform vec4 u_Color;

in vec4 v_Pos;
in vec2 v_TexCoord;
out vec4 color;

void main() {
    //color = texture(u_Texture, v_TexCoord);
    color = vec4(v_Pos.xyz, 1.0f) + u_Color * 0.001f + vec4(0.2f, 0.2f, 0.2f, 1.0f);
}
        ";

        private static VertexArray vao;
        private static VertexBuffer vbo;
        private static IndexBuffer ibo;
        private static Shader shader;


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


            vao = new VertexArray();
            vao.Bind();

            float* vertices = stackalloc float[]
            {
                //X    Y      Z
                -0.5f, -0.5f, -0.5f, // bottom left away from me 0
                -0.5f, -0.5f, 0.5f, // bottom left towards me    1
                0.5f, -0.5f, -0.5f, // bottom right away from me 2
                0.5f, -0.5f, 0.5f, // bottom right towards me    3
                -0.5f, 0.5f, -0.5f, // top left away from me     4
                -0.5f, 0.5f, 0.5f, // top left towards me        5
                0.5f, 0.5f, -0.5f, // top right away from me     6
                0.5f, 0.5f, 0.5f // top right towards me         7
            };
            vbo = new VertexBuffer(vertices, (3) * 8 * sizeof(float));


            vbo.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "a_Pos")
            });

            uint* indicies = stackalloc uint[]
            {
                1, 0, 3,
                3, 0, 2,
                3, 5, 1,
                3, 7, 5,
                7, 6, 4,
                7, 4, 5,
                2, 4, 6,
                2, 0, 4,
                2, 6, 7,
                3, 2, 7,
                5, 4, 0,
                0, 1, 5
            };
            ibo = new IndexBuffer(indicies, 12 * 3 * sizeof(uint));
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
            shader.UploadUniformFloat4("u_Color", new(1.0f, 0.0f, 0.0f, 1.0f));

            shader.Unbind();
        }

        private static unsafe void OnRender(double obj) //Method needs to be unsafe due to draw elements.
        {
            //Clear the color channel.
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));


            //Bind the geometry and shader.
            vao.Bind();
            shader.Bind();
            //Draw the geometry.
            Gl.DrawElements(PrimitiveType.Triangles, 12 * 3, DrawElementsType.UnsignedInt, null);
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
        }

        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                GlWindow.Close();
            }
        }


        //         private static uint Shader;
        //
        //     //Vertex shaders are run on each vertex.
        //     private static readonly string VertexShaderSource = @"
        //     #version 330 core //Using version GLSL version 3.3
        //     layout (location = 0) in vec4 vPos;
        //     
        //     void main()
        //     {
        //         gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        //     }
        //     ";
        //
        //     //Fragment shaders are run on each fragment/pixel of the geometry.
        //     private static readonly string FragmentShaderSource = @"
        //     #version 330 core
        //     out vec4 FragColor;
        //     void main()
        //     {
        //         FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        //     }
        //     ";
        //     static void Main(string[] args)
        //     {
        //         WindowOptions options = WindowOptions.Default;
        //
        //         options.Title = "BuildCraft";
        //         options.Size = new(800, 600);
        //
        //         // VertexArray vao;
        //         // VertexBuffer vbo;
        //         // IndexBuffer ibo;
        //
        //         uint vao = 0, vbo, ibo;
        //         
        //         OpenGLContext.Init(options, () =>
        //         {
        //             // vao = new VertexArray();
        //             Pointer<float> vertices = stackalloc float[]
        //             {
        //                 0.0f, 0.0f, 0.0f,
        //                 1.0f, 0.0f, 0.0f,
        //                 0.0f, 1.0f, 0.0f
        //             };
        //             // vbo = new VertexBuffer((3 + 2) * 3 * sizeof(float));
        //             // vbo.Layout = new BufferLayout(new BufferElement[]
        //             // {
        //             //     new(ShaderDataType.Float3, "a_Position"), 
        //             //     new(ShaderDataType.Float2, "a_TexCoords")
        //             // });
        //             // vbo.SetData(vertices, (3 + 2) * 3 * sizeof(float));
        //
        //             Pointer<uint> indices = stackalloc uint[3]
        //             {
        //                 0, 1, 2
        //             };
        //             // ibo = new IndexBuffer(indices, 3 * sizeof(uint));
        //             // vao.AddVertexBuffer(vbo);
        //             // vao.SetIndexBuffer(ibo);
        //             vao = Gl.GenVertexArray();
        //             Gl.BindVertexArray(vao);
        //             vbo = Gl.GenBuffer();
        //             Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        //             Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) ((3) * 3 * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        //             Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        //             Gl.EnableVertexAttribArray(0);
        //             ibo = Gl.GenBuffer();
        //             Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ibo);
        //             Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (3 * sizeof(uint)), indices, BufferUsageARB.StaticDraw);
        //             
        //             //Creating a vertex shader.
        //             uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        //             Gl.ShaderSource(vertexShader, VertexShaderSource);
        //             Gl.CompileShader(vertexShader);
        //
        //             //Checking the shader for compilation errors.
        //             string infoLog = Gl.GetShaderInfoLog(vertexShader);
        //             if (!string.IsNullOrWhiteSpace(infoLog))
        //             {
        //                 Console.WriteLine($"Error compiling vertex shader {infoLog}");
        //             }
        //
        //             //Creating a fragment shader.
        //             uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        //             Gl.ShaderSource(fragmentShader, FragmentShaderSource);
        //             Gl.CompileShader(fragmentShader);
        //
        //             //Checking the shader for compilation errors.
        //             infoLog = Gl.GetShaderInfoLog(fragmentShader);
        //             if (!string.IsNullOrWhiteSpace(infoLog))
        //             {
        //                 Console.WriteLine($"Error compiling fragment shader {infoLog}");
        //             }
        //
        //             //Combining the shaders under one shader program.
        //             Shader = Gl.CreateProgram();
        //             Gl.AttachShader(Shader, vertexShader);
        //             Gl.AttachShader(Shader, fragmentShader);
        //             Gl.LinkProgram(Shader);
        //
        //             //Checking the linking for errors.
        //             Gl.GetProgram(Shader, GLEnum.LinkStatus, out var status);
        //             if (status == 0)
        //             {
        //                 Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(Shader)}");
        //             }
        //
        //             //Delete the no longer useful individual shaders;
        //             Gl.DetachShader(Shader, vertexShader);
        //             Gl.DetachShader(Shader, fragmentShader);
        //             Gl.DeleteShader(vertexShader);
        //             Gl.DeleteShader(fragmentShader);
        //
        //         }, _ =>
        //         {
        //             Gl.UseProgram(Shader);
        //             Gl.BindVertexArray(vao);
        //             Gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, null);
        //         }, EmptyAction<double>(), EmptyVoidAction());
        //         OpenGLContext.RunWindow();
        //     }
    }
}
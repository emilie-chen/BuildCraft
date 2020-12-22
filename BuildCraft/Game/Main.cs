using System;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Base.Std;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using static BuildCraft.Base.OpenGLContext;

namespace BuildCraft.Game
{
    using Mat4 = Matrix4X4<float>;
    using Mat3 = Matrix3X3<float>;
    using Vec4 = Vector4D<float>;
    using Vec3 = Vector3D<float>;
    using Vec2 = Vector2D<float>;

    public unsafe class Application
    {
        // private static uint Shader;

        //Vertex shaders are run on each vertex.
        private static readonly string VertexShaderSource = @"
        #version 330 core //Using version GLSL version 3.3
        layout (location = 0) in vec4 vPos;
        
        void main()
        {
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        
        uniform vec4 u_Color;

        void main()
        {
            FragColor = u_Color;
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

            Gl.Enable(GLEnum.Blend);

            vao = new VertexArray();
            vao.Bind();

            float* vertices = stackalloc float[]
            {
                //X    Y      Z
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.5f
            };
            vbo = new VertexBuffer(vertices, (3) * 4 * sizeof(float));


            vbo.Layout = new BufferLayout(new BufferElement[]
            {
                new(ShaderDataType.Float3, "vPos")
            });

            uint* indicies = stackalloc uint[]
            {
                0, 1, 3,
                1, 2, 3
            };
            ibo = new IndexBuffer(indicies, 6 * sizeof(uint));


            vao.AddVertexBuffer(vbo);
            vao.SetIndexBuffer(ibo);
            vao.Unbind();
            vbo.Unbind();
            ibo.Unbind();

            shader = new Shader("MainShader", VertexShaderSource, FragmentShaderSource);
            
            shader.UploadUniformFloat4("u_Color", new(1.0f, 0.0f, 0.0f, 1.0f));
            
            shader.Unbind();
        }

        private static unsafe void OnRender(double obj) //Method needs to be unsafe due to draw elements.
        {
            //Clear the color channel.
            Gl.Clear((uint) ClearBufferMask.ColorBufferBit);

            //Bind the geometry and shader.
            vao.Bind();
            shader.Bind();
            //Draw the geometry.
            Gl.DrawElements(PrimitiveType.Triangles, 6U, DrawElementsType.UnsignedInt, null);
        }

        private static void OnUpdate(double obj)
        {
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
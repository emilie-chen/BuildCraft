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
    public unsafe class Application
    {
        private static uint Shader;

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
        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
        ";

        //Vertex data, uploaded to the VBO.
        private static readonly float[] Vertices =
        {
            //X    Y      Z
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.5f
        };

        //Index data, uploaded to the EBO.
        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private static VertexArray vao;
        private static VertexBuffer vbo;
        private static IndexBuffer ibo;


        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            Init(options, OnLoad, OnUpdate, OnRender, OnClose);
            RunWindow();
        }

        private static void OnLoad()
        {
            IInputContext input = GlWindow.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
            }
            
            vao = new VertexArray();
            vao.Bind();
            
            fixed (float* v = Vertices)
            {
                vbo = new VertexBuffer(v, Vertices.Length * sizeof(float));
            }

            fixed (uint* i = &Indices[0])
            {
                ibo = new IndexBuffer(i, Indices.Length * sizeof(uint));
            }

            //Creating a vertex shader.
            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, VertexShaderSource);
            Gl.CompileShader(vertexShader);

            //Checking the shader for compilation errors.
            string infoLog = Gl.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling vertex shader {infoLog}");
            }

            //Creating a fragment shader.
            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, FragmentShaderSource);
            Gl.CompileShader(fragmentShader);

            //Checking the shader for compilation errors.
            infoLog = Gl.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                Console.WriteLine($"Error compiling fragment shader {infoLog}");
            }

            //Combining the shaders under one shader program.
            Shader = Gl.CreateProgram();
            Gl.AttachShader(Shader, vertexShader);
            Gl.AttachShader(Shader, fragmentShader);
            Gl.LinkProgram(Shader);

            //Checking the linking for errors.
            Gl.GetProgram(Shader, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(Shader)}");
            }

            //Delete the no longer useful individual shaders;
            Gl.DetachShader(Shader, vertexShader);
            Gl.DetachShader(Shader, fragmentShader);
            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);

            //Tell opengl how to give the data to the shaders.
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
            Gl.EnableVertexAttribArray(0);
        }

        private static unsafe void OnRender(double obj) //Method needs to be unsafe due to draw elements.
        {
            //Clear the color channel.
            Gl.Clear((uint) ClearBufferMask.ColorBufferBit);

            //Bind the geometry and shader.
            vao.Bind();
            Gl.UseProgram(Shader);

            //Draw the geometry.
            Gl.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private static void OnUpdate(double obj)
        {
        }

        private static void OnClose()
        {
            //Remember to delete the buffers.
            vao.Dispose();
            Gl.DeleteProgram(Shader);
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
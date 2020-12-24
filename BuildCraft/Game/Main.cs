using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using BuildCraft.Base;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Base.Std;
using BuildCraft.Game.Renderer;
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


    public static unsafe class Application
    {
        private static Texture tex;

        private static CubeRenderer renderer;


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

            tex = new Texture("Assets/Textures/testimg.png");
            renderer = new CubeRenderer();
        }

        private static unsafe void OnRender(double obj)
        {
            renderer.BeginScene(MainCamera);
            renderer.RenderCube(new (), tex);
            renderer.EndScene();
        }

        private static void OnUpdate(double ts)
        {
            MainCamera.Update((float) ts);
        }

        private static void OnClose()
        {
            //Remember to delete the buffers.
            tex.Dispose();
            renderer.Dispose();
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
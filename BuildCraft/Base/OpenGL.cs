using System;
using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BuildCraft.Base
{
    using Mat4 = Matrix4x4;
    using Vec4 = Vector4;
    using Vec3 = Vector3;
    using Vec2 = Vector2;

    public static class OpenGLContext
    {
        public static GL Gl;
        public static IWindow GlWindow;
        public static IInputContext InputContext;
        public static Camera MainCamera;

        public static Action EmptyVoidAction()
        {
            return () => { };
        }

        public static Action<T> EmptyAction<T>()
        {
            return _ => { };
        }
        
        public static void Init(WindowOptions options, Action load, Action<double> update, Action<double> render,
            Action closing)
        {
            GlWindow = Window.Create(options);
            GlWindow.Load += () =>
            {
                MainCamera = new Camera(GlWindow, Vec3.Zero + Vec3.UnitZ, Vec3.UnitY, 0.0f, 0.0f, 5.0f, 0.01f);
                InputContext = GlWindow.CreateInput();
                Gl = GL.GetApi(GlWindow);
                IInputContext input = InputContext;
                foreach (IMouse mouse in input.Mice)
                {
                    mouse.Cursor.CursorMode = CursorMode.Raw;
                    mouse.MouseMove += MainCamera.OnMouseMove;
                }
            };
            

            GlWindow.Load += load;
            GlWindow.Update += update;
            GlWindow.Render += render;
            GlWindow.Closing += closing;
        }

        public static void RunWindow()
        {
            GlWindow.Run();
        }
    }
}
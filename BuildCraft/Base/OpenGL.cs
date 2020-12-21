using System;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BuildCraft.Base
{
    public static class OpenGLContext
    {
        public static GL Gl;
        public static IWindow GlWindow;

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
                Gl = GL.GetApi(GlWindow);
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
using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static BuildCraft.Base.Std.Native;
using static BuildCraft.Base.OpenGLContext;

namespace BuildCraft.Base.GlWrappers
{
    public class Texture : IDisposable
    {
        private uint m_RendererID;
        
        public unsafe Texture(string path)
        {
            using Image<Rgba32> img = (Image<Rgba32>) Image.Load(path);
            //We need to flip our image as image sharps coordinates has origin (0, 0) in the top-left corner,
            //where as openGL has origin in the bottom-left corner.
            img.Mutate(x => x.Flip(FlipMode.Vertical));

            fixed (void* data = &MemoryMarshal.GetReference(img.GetPixelRowSpan(0)))
            {
                Load(data, (uint) img.Width, (uint) img.Height);
            }

        }
        
        public unsafe Texture(void* data, uint width, uint height)
        {
            Load(data, width, height);
        }
        
        private unsafe void Load(void* data, uint width, uint height)
        {
            m_RendererID = Gl.GenTexture();
            Bind();

            Gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.Repeat);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.Repeat);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
            Gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(uint textureSlot = 0)
        {
            Gl.ActiveTexture((TextureUnit) ((uint) TextureUnit.Texture0 + textureSlot));
            Gl.BindTexture(TextureTarget.Texture2D, m_RendererID);
        }

        private void ReleaseUnmanagedResources()
        {
            Gl.DeleteTexture(m_RendererID);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~Texture()
        {
            ReleaseUnmanagedResources();
        }
    }
}
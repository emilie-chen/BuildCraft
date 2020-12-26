using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Game.World;
using Silk.NET.GLFW;

namespace BuildCraft.Game.Resource
{
    public class TextureManager
    {
        private static ISet<Texture> m_Textures;

        //each face of a block is allowed to have its own texture (refer to OptiFine)
        private static IDictionary<(BlockType, BlockFace), Texture> m_BlockTypeToTexture;

        /// <summary>
        /// Call Init only after opengl has been initialized
        /// </summary>
        public static void Init()
        {
            m_Textures = new HashSet<Texture>();
            m_BlockTypeToTexture = new Dictionary<(BlockType, BlockFace), Texture>();

            // load textures from disk
            // air maps to null
            for (int i = 0; i < 6; i++)
            {
                m_BlockTypeToTexture[(BlockType.Air, (BlockFace) i)] = null;
            }

            // loop through all block types except air, which has no texture
            for (int i = 1; i <= (int) Enum.GetValues(typeof(BlockType)).Cast<BlockType>().Last(); i++)
            {
                // loop through the 6 faces of each block
                for (int j = 0; j < 6; j++)
                {
                    string expectedFileName =
                        $"BlockTexture-{i}-{(BlockType) i}-{(BlockFace) j}.png";
                    Texture tex = new Texture($"Assets/Textures/{expectedFileName}");
                    m_Textures.Add(tex);
                    m_BlockTypeToTexture[((BlockType)i, (BlockFace)j)] = tex;
                    Console.WriteLine($"Loading texture: {expectedFileName}");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Texture GetTextureFromBlockType((BlockType, BlockFace) typeAndFace)
        {
            m_BlockTypeToTexture.TryGetValue(typeAndFace, out Texture texture);
            return texture;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Texture GetTextureFromBlockType(BlockType type, BlockFace face)
        {
            return GetTextureFromBlockType((type, face));
        }

        /// <summary>
        /// Doesn't matter if you call it or not. The textures will in any case live during the entire lifetime
        /// of the program
        /// </summary>
        public static void CleanUp()
        {
            foreach (Texture texture in m_Textures)
            {
                texture.Dispose();
            }
        }
    }
}
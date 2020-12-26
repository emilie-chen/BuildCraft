using System;
using System.Numerics;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Base.Std;
using static BuildCraft.Base.Std.Native;

namespace BuildCraft.Game.World
{
    public class Chunk
    {
        public const int NUM_OF_BLOCKS_IN_CHUNK = 16 * 256 * 16;

        /// <summary>
        /// Array layout: all x's, then all y's, then all z's
        /// To access through all the blocks in the correct order, you need to:
        /// for z in 0..15 then y in 0..255 then x in 0..15
        /// </summary>
        private readonly NativeArray<Block> m_Blocks;
        public unsafe Block* BlockData => m_Blocks.Data;

        private Vector2 m_BasePosition;
        public Vector3 BasePosition
        {
            get => new(m_BasePosition.X, 0.0f, m_BasePosition.Y);
            set => m_BasePosition = new(value.X, value.Z);
        }

        /// <summary>
        /// Creates a chunk with a base position and initialize it will air blocks
        /// </summary>
        /// <param name="basePosition"></param>
        public unsafe Chunk(Vector2 basePosition)
        {
            m_Blocks = new NativeArray<Block>(16 * 256 * 16);
            memset(m_Blocks.Data, 0, 16 * 256 * 16 * sizeof(Block));
            
            m_BasePosition = basePosition;
        }

        /// <summary>
        /// obtain block reference with coordinates RELATIVE to the base position of the chunk
        /// E.g. If the chunk starts at 32, y, 32, and you want to obtain a reference to a block at world coordinates
        /// 33, 10, 34, you will need to call chunk[1, 10, 2]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public ref Block this[int x, int y, int z] => ref m_Blocks.At(x + y * 16 + z * 16 * 256);

        ~Chunk()
        {
            // save chunk data before destroying

            m_Blocks.Dispose();
        }
    }
}
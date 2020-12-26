namespace BuildCraft.Game.World
{
    public enum BlockType : uint
    {
        Air = 0,
        Dirt = 1,
        Cobblestone = 2
    }

    public enum BlockFace : byte
    {
        Back = 0,
        Front = 1,
        Left = 2,
        Right = 3,
        Bottom = 4,
        Top = 5
    }
    
    public unsafe struct Block
    {
        public BlockType Type;
        public BlockProperties Props;
        public void* ExternalData;
    }
}
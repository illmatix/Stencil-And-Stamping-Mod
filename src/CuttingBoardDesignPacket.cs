using ProtoBuf;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    [ProtoContract]
    public class CuttingBoardDesignPacket
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        [ProtoMember(4)]
        public int GridSize;

        [ProtoMember(5)]
        public bool[] CellPattern;

        [ProtoMember(6)]
        public bool CellBorders;

        [ProtoMember(7)]
        public bool EdgeBorder;

        public BlockPos BoardPos
        {
            get => new BlockPos(X, Y, Z);
            set { X = value.X; Y = value.Y; Z = value.Z; }
        }
    }
}

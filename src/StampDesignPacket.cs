using ProtoBuf;
using Vintagestory.API.MathTools;

namespace StencilAndStamping
{
    [ProtoContract]
    public class StampDesignPacket
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        [ProtoMember(4)]
        public string Face;

        [ProtoMember(5)]
        public int GridSize;

        [ProtoMember(6)]
        public string[] CellColors;

        [ProtoMember(7)]
        public bool CellBorders;

        [ProtoMember(8)]
        public bool EdgeBorder;

        public BlockPos TargetPos
        {
            get => new BlockPos(X, Y, Z);
            set { X = value.X; Y = value.Y; Z = value.Z; }
        }
    }
}

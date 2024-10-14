using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;

namespace SLC_LayoutEditor.Core.PathFinding
{
    class Node
    {
        private readonly CabinSlot slot;

        #region Node base properties
        public int X { get; set; }

        public int Y { get; set; }

        public bool IsObstacle { get; set; }

        public bool? IsObstacleOverride { get; set; }

        public Node Parent { get; set; }

        public CabinSlot Slot => slot;
        #endregion

        #region A* properties
        public int G { get; set; }

        public int H { get; set; }

        public int F => G + H;
        #endregion

        #region Dijkstra properties
        public int Cost { get; set; } = int.MaxValue;
        #endregion

        public Node(CabinSlot slot)
        {
            this.slot = slot;

            X = slot.Row;
            Y = slot.Column;
            IsObstacle = slot.Type != CabinSlotType.Aisle && !slot.IsDoor &&
                    slot.Type != CabinSlotType.ServiceEndPoint && slot.Type != CabinSlotType.ServiceStartPoint;
        }
    }
}

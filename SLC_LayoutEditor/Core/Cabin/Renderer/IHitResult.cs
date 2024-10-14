using System.Windows;

namespace SLC_LayoutEditor.Core.Cabin.Renderer
{
    internal interface IHitResult
    {
        Rect Rect { get; }

        bool IsSlot { get; }

        string Tooltip { get; }

        bool IsRemoved { get; set; }

        int Row { get; }

        int Column { get; }

        int TargetRow { get; }

        int TargetColumn { get; }
    }
}

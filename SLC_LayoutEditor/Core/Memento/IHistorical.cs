namespace SLC_LayoutEditor.Core.Memento
{
    public interface IHistorical
    {
        string Message { get; }

        string Guid { get; }
    }
}

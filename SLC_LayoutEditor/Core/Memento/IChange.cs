namespace SLC_LayoutEditor.Core.Memento
{
    public interface IChange<T>
    {
        T Data { get; }

        T PreviousData { get; }
    }
}

namespace SLC_LayoutEditor.Core.Memento
{
    public class Change<T> : IChange<T>
    {
        //private readonly CabinChangeCategory changeType;

        private readonly T previousData;
        private readonly T data;

        //public CabinChangeCategory ChangeType => changeType;

        public T Data => data;

        public T PreviousData => previousData;

        public Change(T data, T previousData)
        {
            this.data = data;
            this.previousData = previousData;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" >> \"{1}\"", previousData.ToString(), data.ToString());
        }
    }
}

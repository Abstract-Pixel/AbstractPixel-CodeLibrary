namespace AbstractPixel.Core
{
    public interface IInitializable<TData>
    {
        void Initialize(TData _data);
    }
}
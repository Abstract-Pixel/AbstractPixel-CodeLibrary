namespace AbstractPixel.SaveSystem
{
    /// <summary>
    /// Picks the save file for a class whose saved data is its <see cref="SaveAttribute"/> fields. With neither this
    /// nor <see cref="ISavable{TData, TFile}"/>, the data goes in <c>SaveFiles.GameProfile</c>.
    /// Example: <c>class Chest : MonoBehaviour, ISaveIn&lt;SaveFiles.Scene&gt; { [Save] bool isOpen; }</c>
    /// </summary>
    public interface ISaveIn<TFile> where TFile : ISaveFile
    {
    }
}

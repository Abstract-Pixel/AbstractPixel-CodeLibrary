using System.Threading.Tasks;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    public interface IDataStorageService
    {
        bool SaveFile(string _data, string fullpath);
        string LoadFile(string fullpath);
        bool FileExists(string _filePath);
        bool DeleteFile(string _filePath);

        bool CreateDirectory(string _directoryPath);
        string[] GetDirectories(string _directoryPath);
        bool DeleteDirectory(string _directoryPath);
        bool DirectoryExists(string _directoryPath);
    }
}

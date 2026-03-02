using UnityEngine;
using System.IO;

namespace AbstractPixel.SaveSystem
{
    public sealed class FileDataStorageService : IDataStorageService
    {
        static readonly string temporaryFileExtension = $".{FileExtension.tmp.ToString().ToLower()}";
        public bool SaveFile(string _data, string _fullpath)
        {
            if (string.IsNullOrEmpty(_fullpath))
            {
                Debug.LogError("FileDataStorageService: Save failed. File path is null or empty.");
                return false;
            }

            if (string.IsNullOrEmpty(_data))
            {
                Debug.LogError("FileDataStorageService: Save failed. Serialized Data to save is null or empty.");
                return false;
            }

            try
            {
                string directoryPath = Path.GetDirectoryName(_fullpath);
                if (!CreateDirectory(directoryPath))
                {
                    Debug.LogWarning($"FileDataStorageService: [Save Warning] Directory does not exist at path: {directoryPath}. Creating directory.");
                    Directory.CreateDirectory(directoryPath);
                }
                string tempFilePath = _fullpath + temporaryFileExtension;
                if (!DeleteFile(tempFilePath))
                {
                    return false;
                }
                File.WriteAllText(tempFilePath, _data);

                if (!DeleteFile(_fullpath))
                {
                    return false;
                }
                File.Move(tempFilePath, _fullpath);
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] Save failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] Save failed. Exception: {e}");
                return false;
            }
        }
        public string LoadFile(string _fullpath)
        {
            if (string.IsNullOrEmpty(_fullpath))
            {
                Debug.LogError("FileDataStorageService: Load failed. File path is null or empty.");
                return null;
            }
            try
            {
                if (!File.Exists(_fullpath))
                {
                    Debug.LogError($"FileDataStorageService: Load failed. File does not exist at path: {_fullpath}");
                    return null;
                }
                string data = File.ReadAllText(_fullpath);
                return data;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] Load failed. Exception: {e}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] Load failed. Exception: {e}");
                return null;
            }
        }
      
        public bool CreateDirectory(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("FileDataStorageService: CreateDirectory failed. Directory path is null or empty.");
                return false;
            }
            try
            {
                if (Directory.Exists(_directoryPath))
                {
                    return true;
                }
                Directory.CreateDirectory(_directoryPath);
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] CreateDirectory failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] CreateDirectory failed. Exception: {e}");
                return false;
            }
        }

        public string[] GetDirectories(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("FileDataStorageService: GetSubDirectories failed. Directory path is null or empty.");
                return null;
            }
            try
            {
                if (!Directory.Exists(_directoryPath))
                {
                    Debug.LogError($"FileDataStorageService: GetSubDirectories failed. Directory does not exist at path: {_directoryPath}");
                    return null;
                }
                string[] subDirectories = Directory.GetDirectories(_directoryPath);
                return subDirectories;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] GetSubDirectories failed. Exception: {e}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] GetSubDirectories failed. Exception: {e}");
                return null;
            }
        }

        public bool DeleteDirectory(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("FileDataStorageService: DeleteDirectory failed. Directory path is null or empty.");
                return false;
            }
            try
            {
                if (!Directory.Exists(_directoryPath))
                {
                    return true;
                }
                Directory.Delete(_directoryPath, true);
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] DeleteDirectory failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] DeleteDirectory failed. Exception: {e}");
                return false;
            }

        }

        public bool DeleteFile(string _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                Debug.LogError("FileDataStorageService: DeleteFile failed. File path is null or empty.");
                return false;
            }
            try
            {
                if (!File.Exists(_filePath))
                {
                    return true;
                }
                File.Delete(_filePath);
                return true;
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] DeleteFile failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] DeleteFile failed. Exception: {e}");
                return false;
            }
        }

        public bool FileExists(string _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                Debug.LogError("FileDataStorageService: FileExists check failed. File path is null or empty.");
                return false;
            }
            try
            {
                return File.Exists(_filePath);
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] FileExists check failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] FileExists check failed. Exception: {e}");
                return false;
            }

        }

        public bool DirectoryExists(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("FileDataStorageService: DirectoryExists check failed. Directory path is null or empty.");
                return false;
            }
            try
            {
                return Directory.Exists(_directoryPath);
            }
            catch (IOException e)
            {
                Debug.LogError($"FileDataStorageService:[I/O Exception Error] DirectoryExists check failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FileDataStorageService:[Unexpected Error] DirectoryExists check failed. Exception: {e}");
                return false;
            }
        }
    }
}
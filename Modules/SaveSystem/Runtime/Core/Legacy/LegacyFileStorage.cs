using System.IO;
using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    // Temporary: the v1 path-based file storage, kept only so the old SaveManager keeps working until T7 removes it.
    // New code uses IDataStorageService and DiskStorageService.
    internal sealed class LegacyFileStorage
    {
        static readonly string temporaryFileExtension = $".{FileExtension.tmp.ToString().ToLower()}";
        public bool SaveFile(string _data, string _fullpath)
        {
            if (string.IsNullOrEmpty(_fullpath))
            {
                Debug.LogError("LegacyFileStorage: Save failed. File path is null or empty.");
                return false;
            }

            if (string.IsNullOrEmpty(_data))
            {
                Debug.LogError("LegacyFileStorage: Save failed. Serialized Data to save is null or empty.");
                return false;
            }

            try
            {
                string directoryPath = Path.GetDirectoryName(_fullpath);
                if (!CreateDirectory(directoryPath))
                {
                    Debug.LogWarning($"LegacyFileStorage: [Save Warning] Directory does not exist at path: {directoryPath}. Creating directory.");
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
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] Save failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] Save failed. Exception: {e}");
                return false;
            }
        }
        public string LoadFile(string _fullpath)
        {
            if (string.IsNullOrEmpty(_fullpath))
            {
                Debug.LogError("LegacyFileStorage: Load failed. File path is null or empty.");
                return null;
            }
            try
            {
                if (!File.Exists(_fullpath))
                {
                    Debug.LogError($"LegacyFileStorage: Load failed. File does not exist at path: {_fullpath}");
                    return null;
                }
                string data = File.ReadAllText(_fullpath);
                return data;
            }
            catch (IOException e)
            {
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] Load failed. Exception: {e}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] Load failed. Exception: {e}");
                return null;
            }
        }

        public bool CreateDirectory(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("LegacyFileStorage: CreateDirectory failed. Directory path is null or empty.");
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
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] CreateDirectory failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] CreateDirectory failed. Exception: {e}");
                return false;
            }
        }

        public string[] GetDirectories(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("LegacyFileStorage: GetSubDirectories failed. Directory path is null or empty.");
                return null;
            }
            try
            {
                if (!Directory.Exists(_directoryPath))
                {
                    Debug.LogError($"LegacyFileStorage: GetSubDirectories failed. Directory does not exist at path: {_directoryPath}");
                    return null;
                }
                string[] subDirectories = Directory.GetDirectories(_directoryPath);
                return subDirectories;
            }
            catch (IOException e)
            {
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] GetSubDirectories failed. Exception: {e}");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] GetSubDirectories failed. Exception: {e}");
                return null;
            }
        }

        public bool DeleteDirectory(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("LegacyFileStorage: DeleteDirectory failed. Directory path is null or empty.");
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
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] DeleteDirectory failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] DeleteDirectory failed. Exception: {e}");
                return false;
            }

        }

        public bool DeleteFile(string _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                Debug.LogError("LegacyFileStorage: DeleteFile failed. File path is null or empty.");
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
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] DeleteFile failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] DeleteFile failed. Exception: {e}");
                return false;
            }
        }

        public bool FileExists(string _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                Debug.LogError("LegacyFileStorage: FileExists check failed. File path is null or empty.");
                return false;
            }
            try
            {
                return File.Exists(_filePath);
            }
            catch (IOException e)
            {
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] FileExists check failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] FileExists check failed. Exception: {e}");
                return false;
            }

        }

        public bool DirectoryExists(string _directoryPath)
        {
            if (string.IsNullOrEmpty(_directoryPath))
            {
                Debug.LogError("LegacyFileStorage: DirectoryExists check failed. Directory path is null or empty.");
                return false;
            }
            try
            {
                return Directory.Exists(_directoryPath);
            }
            catch (IOException e)
            {
                Debug.LogError($"LegacyFileStorage:[I/O Exception Error] DirectoryExists check failed. Exception: {e}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LegacyFileStorage:[Unexpected Error] DirectoryExists check failed. Exception: {e}");
                return false;
            }
        }
    }
}
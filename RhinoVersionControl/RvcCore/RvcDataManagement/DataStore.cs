using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Rhino.FileIO;
using Rhino.DocObjects;

namespace RvcCore.RvcDataManagement
{
    /// <summary>
    /// Stores all the raw data from all versions of a file, to be used by the version management as neccessary
    /// </summary>
    public class DataStore: IDisposable
    {
        #region fields
        private File3dm _storeFile = null;
        private static string storeFileExtension = ".rvcstore";
        #endregion

        #region properties
        public string StoreFileName { get; private set; }
        public string StoreDirectory { get; internal set; }
        public string StoreFilePath { get => Path.Combine(StoreDirectory, StoreFileName); }

        internal static List<PropertyInfo> TablePropInfos = new List<PropertyInfo>();
        #endregion

        #region constructors
        static DataStore()
        {
            TablePropInfos = typeof(File3dm).GetProperties().Where((t) => IsTableType(t.PropertyType)).ToList();
        }
        public DataStore(string rhinoFilePath, Action<string> storeFileNameSetter, Guid rvcTetherId)
        {
            string rhDir = Path.GetDirectoryName(rhinoFilePath);
            string rvcDirPath = Path.Combine(rhDir, ".rvc");

            if (!Directory.Exists(rvcDirPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(rvcDirPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            string archivePath = Path.Combine(rvcDirPath, rvcTetherId.ToString());
            if (!Directory.Exists(archivePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(archivePath);
            }

            StoreDirectory = archivePath;
            StoreFileName = rvcTetherId.ToString() + storeFileExtension;
            storeFileNameSetter.Invoke(StoreFileName);

            _storeFile?.Dispose();
            if (File.Exists(StoreFilePath))
            {
                _storeFile = File3dm.Read(StoreFilePath);
            }
            else
            {
                _storeFile = new File3dm();
                _storeFile.Write(StoreFilePath, 0);
            }
        }
        #endregion

        #region methods
        public T ObjectLookup<T>(Guid id)
            where T : ModelComponent
        {
            T obj = null;
            foreach(var tableProp in TablePropInfos)
            {
                Type tableOfType = tableProp.PropertyType.GetGenericArguments().Single();
                if (!typeof(T).IsAssignableFrom(tableOfType)) { continue; }
                File3dmCommonComponentTable<T> table = (File3dmCommonComponentTable<T>)tableProp.GetValue(_storeFile);
                obj = table.FindId(id);
                if(obj != null) { break; }
            }
            return obj;
        }
        public void Dispose()
        {
            _storeFile.Dispose();
        }

        public static bool IsTableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(File3dmCommonComponentTable<>);
        }
        #endregion
    }
}

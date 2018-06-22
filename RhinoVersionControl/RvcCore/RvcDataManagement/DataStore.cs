using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Rhino.FileIO;
using Rhino.DocObjects;
using RvcCore.Util;

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
            TablePropInfos = typeof(File3dm).GetProperties().Where((t) => TableUtil.IsFile3dmTableType(t.PropertyType)).ToList();
        }
        public DataStore(string rhinoFilePath, Guid rvcTetherId)
        {
            string rhDir = Path.GetDirectoryName(rhinoFilePath);
            string rvcDirPath = Path.Combine(rhDir, RvcRhinoFileTether.RvcArchiveDirectoryName);

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
        public T ObjectLookupOfType<T>(Guid id)
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

        public ModelComponent ObjectLookup(Guid id, Type objT)
        {
            if (!typeof(ModelComponent).IsAssignableFrom(objT))
            {
                throw new NotImplementedException("Invalid member data type for the table");
            }

            MethodInfo genericMethod = GetType().GetMethod("ObjectLookupOfType");
            MethodInfo method = genericMethod.MakeGenericMethod(objT);
            return (ModelComponent)method.Invoke(this, new object[] { id });
        }

        /// <summary>
        /// Adds a new object to the store, to the appropriate table and returns the guid of the newly added object
        /// </summary>
        /// <typeparam name="T">Type of the object being added</typeparam>
        /// <param name="comp">The object to be added</param>
        /// <returns>Guid of the newly added object</returns>
        public Guid AddObjectOfType<T>(T comp, bool assignNewGuid = false)
            where T : ModelComponent
        {
            foreach (var tableProp in TablePropInfos)
            {
                Type tableOfType = tableProp.PropertyType.GetGenericArguments().Single();
                if (!typeof(T).IsAssignableFrom(tableOfType)) { continue; }
                File3dmCommonComponentTable<T> table = (File3dmCommonComponentTable<T>)tableProp.GetValue(_storeFile);
                T match = table.FindId(comp.Id);
                if (assignNewGuid || match != null)
                {
                    Guid addedGuid = Guid.NewGuid();
                    comp.Id = addedGuid;
                }
                table.Add(comp);
            }
            return comp.Id;
        }

        public Guid AddObject(ModelComponent comp, bool assignedNewGuid = false)
        {
            MethodInfo genericMethod = GetType().GetMethod("AddObjectOfType");
            MethodInfo method = genericMethod.MakeGenericMethod(comp.GetType());
            return (Guid)method.Invoke(this, new object[] { comp, assignedNewGuid });
        }

        public void Dispose()
        {
            _storeFile.Dispose();
        }
        public bool Save()
        {
            if (File.Exists(StoreFilePath))
            {
                File.Delete(StoreFilePath);
            }
            return _storeFile.Write(StoreFilePath, 0);
        }
        #endregion
    }
}

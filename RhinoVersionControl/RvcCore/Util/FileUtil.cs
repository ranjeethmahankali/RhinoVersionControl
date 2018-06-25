using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using Rhino.FileIO;
using Rhino.DocObjects;
using RvcCore.RvcDataManagement;
using RvcCore.VersionManagement;

namespace RvcCore.Util
{
    /// <summary>
    /// This class has the top level methods to process rhino files
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        /// Makes sure its a 3dm file and that it exists. Returns false if not.
        /// </summary>
        /// <param name="filePath">The path to the rhino file</param>
        /// <returns>True if its a rhino file and it exists, else false.</returns>
        public static bool IsValidRhinoFile(string filePath)
        {
            return Path.GetFileName(filePath).EndsWith(".3dm") && File.Exists(filePath);
        }

        private static FileState ParseFile(File3dm file, DataStore store, RvcVersion version, out ChangeSet changes)
        {
            FileState state = null;
            List<IFileDataTable> parsedTableList = new List<IFileDataTable>();
            //opening the store, and the file and comparing the contents
            List<Type> memberTypes;
            List<string> tableNames;
            List<IEnumerable> tables3dm = GetAllTables(file, out memberTypes, out tableNames);
            ChangeSet fileChanges = new ChangeSet();
            for (int i = 0; i < tables3dm.Count; i++)
            {
                ChangeSet tableChanges;
                IFileDataTable parsedTable = TableUtil.ParseTableData(tables3dm[i], tableNames[i], memberTypes[i], version, store, out tableChanges);
                parsedTableList.Add(parsedTable);
                fileChanges = ChangeSet.Merge(fileChanges, tableChanges);
            }

            //populating the file state
            state = new FileState(store, version.AddDownStreamVersion(fileChanges));
            foreach (var table in parsedTableList)
            {
                state.AddTable(table);
            }
            changes = fileChanges;
            return state;
        }

        public static FileState ParseFile(string filePath, out ChangeSet changes)
        {
            if (!IsValidRhinoFile(filePath))
            {
                changes = null;
                return null;
            }
            //opening the store, and the file and comparing the contents
            using (var file = File3dm.Read(filePath))
            {
                string tetherJson = file.Strings.GetValue(RvcRhinoFileTether.RvcTetherKey);
                string dirPath = Path.GetDirectoryName(filePath);
                RvcRhinoFileTether tether;
                RvcVersion version;
                FileState state;
                bool newArchive = tetherJson == null;
                if (newArchive)
                {
                    tether = StartTrackingFile(file, filePath, out version, out state, out changes);
                }
                else
                {
                    tether = JsonConvert.DeserializeObject<RvcRhinoFileTether>(tetherJson);
                    version = RvcVersion.ReadRootVersion(Path.Combine(dirPath, RvcRhinoFileTether.RvcArchiveDirectoryName, tether.RvcId.ToString()));

                    RvcArchive archive = new RvcArchive(tether, version, Path.GetDirectoryName(filePath));

                    using (var store = new DataStore(filePath, tether.RvcId))
                    {
                        state = ParseFile(file, store, version, out changes);
                    }
                }

                return state;
            }
        }

        private static RvcRhinoFileTether StartTrackingFile(File3dm file, string filePath, out RvcVersion rootVersion, out FileState state, 
            out ChangeSet changes)
        {
            rootVersion = new RvcVersion();
            RvcRhinoFileTether tether = new RvcRhinoFileTether(rootVersion.Id);
            file.Strings.SetString(RvcRhinoFileTether.RvcTetherKey, JsonConvert.SerializeObject(tether));
            using(var store = new DataStore(filePath, tether.RvcId))
            {
                CloneFileToDataStore(file, store);
                store.Save();
            }

            using (var store = new DataStore(filePath, tether.RvcId))
            {
                state = ParseFile(file, store, rootVersion, out changes);
            }
            rootVersion.State = state;
            string dirPath = Path.GetDirectoryName(filePath);
            //RvcVersion.WriteRootVersion(rootVersion, Path.Combine(dirPath, RvcRhinoFileTether.RvcArchiveDirectoryName, tether.RvcId.ToString()));
            //file.Write(filePath, 0);
            return tether;
        }

        public static List<IEnumerable> GetAllTables(File3dm file, out List<Type> memberTypes, out List<string> names)
        {
            var props = typeof(File3dm).GetProperties();
            List<IEnumerable> tables = new List<IEnumerable>();
            memberTypes = new List<Type>();
            names = new List<string>();
            foreach (var p in props)
            {
                Type memberType;
                var match = TableUtil.IsFile3dmTableType(p.PropertyType, out memberType);
                if (!match) { continue; }
                memberTypes.Add(memberType);
                names.Add(p.Name);
                var table = p.GetValue(file);
                tables.Add((IEnumerable)table);
            }

            return tables;
        }

        private static void CloneFileToDataStore(File3dm file, DataStore store)
        {
            var props = typeof(File3dm).GetProperties();
            foreach (var p in props)
            {
                Type memberType;
                var match = TableUtil.IsFile3dmTableType(p.PropertyType, out memberType);
                if (!match) { continue; }
                var table = p.GetValue(file);
                var sourceTable = (IEnumerable)table;

                var i = sourceTable.GetEnumerator();
                while (i.MoveNext())
                {
                    store.AddObject((ModelComponent)i.Current);
                }
            }
        }

        public static List<IEnumerable> GetAllTables(File3dm file)
        {
            List<Type> memTypes;
            List<string> names;
            return GetAllTables(file, out memTypes, out names);
        }
    }
}

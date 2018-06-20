using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Rhino.FileIO;
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

        public static void ParseFile(string filePath, ref RvcRhinoFileTether tether)
        {
            if (!IsValidRhinoFile(filePath)) { return; }
            RvcVersion version = RvcVersion.GetVersionById(tether.VersionId);

            //opening the store, and the file and comparing the contents
            using (var store = new DataStore(filePath, tether.RvcId))
            using (var file = File3dm.Read(filePath))
            {
                List<IEnumerable> tables = GetAllTables(file);
                //incomplete
            }
            //incomplete
            throw new NotImplementedException();
        }

        public static List<IEnumerable> GetAllTables(File3dm file)
        {
            var props = typeof(File3dm).GetProperties();
            List<IEnumerable> tables = new List<IEnumerable>();
            foreach (var p in props)
            {
                Type memberType;
                var match = TableUtil.IsFile3dmTableType(p.PropertyType, out memberType);
                if (!match) { continue; }
                var table = p.GetValue(file);
                tables.Add((IEnumerable)table);
            }

            return tables;
        }
    }
}

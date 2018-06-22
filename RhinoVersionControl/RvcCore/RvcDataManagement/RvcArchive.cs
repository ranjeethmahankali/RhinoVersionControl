using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using RvcCore.VersionManagement;

namespace RvcCore.RvcDataManagement
{
    public class RvcArchive
    {
        #region fields
        internal static string HistoryFileName = "history.rvc";
        #endregion

        #region properties
        public Guid RvcId { get; private set; }
        public string RhinoFileDirectory { get; set; }
        public string ArchivePath
        {
            get => Path.Combine(RhinoFileDirectory, RvcRhinoFileTether.RvcArchiveDirectoryName, RvcId.ToString());
        }
        public RvcVersion RootVersion { get; private set; }
        public string VersionHistoryPath { get => Path.Combine(ArchivePath, HistoryFileName); }
        #endregion

        #region constructors
        internal RvcArchive(Guid rvcId, RvcVersion rootVersion, string rhinoFileDir)
        {
            RvcId = rvcId;
            RhinoFileDirectory = rhinoFileDir;
            RootVersion = rootVersion;
            if (!Directory.Exists(ArchivePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(ArchivePath);
                //di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }
        internal RvcArchive(RvcRhinoFileTether tether, RvcVersion version, string rhinoFileDir) : this (tether.RvcId, version, rhinoFileDir) { }
        #endregion

        #region methods
        public void Save()
        {
            WriteVersionHistory();
        }

        private void WriteVersionHistory()
        {
            using(StreamWriter writer = new StreamWriter(VersionHistoryPath, false))
            {
                writer.Write(JsonConvert.SerializeObject(RootVersion));
            }
        }

        public RvcVersion GetVersionById(Guid id)
        {
            return RvcVersion.GetVersionById(id);
        }
        #endregion
    }
}

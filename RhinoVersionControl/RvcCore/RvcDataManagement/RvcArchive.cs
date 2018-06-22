using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RvcCore.VersionManagement;

namespace RvcCore.RvcDataManagement
{
    public class RvcArchive
    {
        #region fields

        #endregion

        #region properties
        public Guid RvcId { get; private set; }
        public string RhinoFileDirectory { get; set; }
        public string ArchivePath
        {
            get => Path.Combine(RhinoFileDirectory, RvcRhinoFileTether.RvcArchiveDirectoryName, RvcId.ToString());
        }
        public RvcVersion RootVersion { get; private set; }
        #endregion

        #region constructors
        public RvcArchive(Guid rvcId, RvcVersion rootVersion, string rhinoFileDir)
        {
            RvcId = rvcId;
            RhinoFileDirectory = rhinoFileDir;
            RootVersion = rootVersion;
            if (!Directory.Exists(ArchivePath))
            {
                Directory.CreateDirectory(ArchivePath);
            }
        }
        public RvcArchive(RvcRhinoFileTether tether, RvcVersion version, string rhinoFileDir) : this (tether.RvcId, version, rhinoFileDir) { }
        #endregion

        #region methods
        public void Save()
        {
            //incomplete
            throw new NotImplementedException();
        }

        private void SerializeVersionHistory(Action<RvcVersion> versionWriter = null, Action<ChangeSet> changeWriter = null)
        {
            if(versionWriter == null)
            {
                versionWriter = (version) => {

                };
            }
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

using RvcCore.RvcDataManagement;

namespace RvcCore.VersionManagement
{
    //[JsonConverter(typeof(Serialization.RvcVersionSerializer))]
    public class RvcVersion: Entity, IEquatable<RvcVersion>
    {
        #region fields
        private FileState _state = null;
        private static Dictionary<Guid, RvcVersion> _versionDict = new Dictionary<Guid, RvcVersion>();
        #endregion

        #region properties
        public ChangeSet UpstreamChangeSet { get; internal set; }
        public Dictionary<Guid, ChangeSet> DownstreamChangeSets { get; }
        //[JsonIgnore]
        public FileState State
        {
            get
            {
                if(_state == null)
                {
                    EvaluateState();
                }
                return _state;
            }
            internal set { _state = value; }
        }
        [JsonIgnore]
        public RvcVersion RootVersion { get => UpstreamChangeSet == null ? this : UpstreamChangeSet.VersionBefore.RootVersion; }
        #endregion

        #region constructors
        internal RvcVersion()
        {
            DownstreamChangeSets = new Dictionary<Guid, ChangeSet>();
            UpstreamChangeSet = null;
            Id = Guid.NewGuid();
            _versionDict.Add(Id, this);
        }
        #endregion

        #region methods
        public ChangeSet GetDownstreamChangeSet(Guid id)
        {
            ChangeSet set;
            if(!DownstreamChangeSets.TryGetValue(id, out set))
            {
                set = null;
            }
            return set;
        }

        public void AddDownstreamChangeSet(ChangeSet set)
        {
            if (DownstreamChangeSets.ContainsKey(set.Id))
            {
                DownstreamChangeSets[set.Id] = set;
            }
            else
            {
                DownstreamChangeSets.Add(set.Id, set);
            }
        }

        public RvcVersion AddDownStreamVersion(ChangeSet changes)
        {
            RvcVersion newVersion = new RvcVersion();
            changes.VersionBefore = this;
            changes.VersionAfter = newVersion;
            AddDownstreamChangeSet(changes);
            newVersion.UpstreamChangeSet = changes;

            return newVersion;
        }

        /// <summary>
        /// This function evaluates the file state for this version, by traversing the version tree and applying changesets
        /// and caches it in the State property.
        /// </summary>
        /// <returns></returns>
        public void EvaluateState()
        {
            //FileState prevState = 
            _state = UpstreamChangeSet?.VersionBefore?.State.ApplyChangeSet(UpstreamChangeSet) ?? new FileState() ;
            _state.Version = this;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public bool Equals(RvcVersion other)
        {
            throw new NotImplementedException();
        }
        public static RvcVersion GetVersionById(Guid id)
        {
            RvcVersion version;
            if(!_versionDict.TryGetValue(id, out version))
            {
                version = null;
            }
            return version;
        }



        public static RvcVersion ReadRootVersion(string archiveDir)
        {
            string historyPath = Path.Combine(archiveDir, RvcArchive.HistoryFileName);
            RvcVersion rootVersion = null;
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.Objects;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            using (StreamReader reader = new StreamReader(historyPath))
            {
                string jsonStr = reader.ReadToEnd();
                rootVersion = (RvcVersion)serializer.Deserialize(reader, typeof(RvcVersion));
            }

            return rootVersion;
        }
        public static void WriteRootVersion(RvcVersion version, string archiveDir)
        {
            string historyPath = Path.Combine(archiveDir, RvcArchive.HistoryFileName);
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.Objects;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            using (StreamWriter writer = new StreamWriter(historyPath))
            {
                serializer.Serialize(writer, version);
            }
        }

        #endregion
    }
}

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
        public FileState State
        {
            get
            {
                if(_state == null)
                {
                    _state = EvaluateState();
                }
                return _state;
            }
        }
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
        public FileState EvaluateState()
        {
            FileState state;
            FileState prevState = 
            state = UpstreamChangeSet.VersionBefore.State.ApplyChangeSet(UpstreamChangeSet);
            state.Version = this;
            return state;
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
            using(StreamReader reader = new StreamReader(historyPath))
            {
                rootVersion = JsonConvert.DeserializeObject<RvcVersion>(reader.ReadToEnd());
            }

            return rootVersion;
        }
        #endregion
    }
}

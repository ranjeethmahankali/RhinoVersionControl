using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RvcCore.RvcDataManagement;

namespace RvcCore.VersionManagement
{
    public class Version: Entity
    {
        #region fields
        private FileState _state = null;
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
        #endregion

        #region constructors
        private Version()
        {
            DownstreamChangeSets = new Dictionary<Guid, ChangeSet>();
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
            return state;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

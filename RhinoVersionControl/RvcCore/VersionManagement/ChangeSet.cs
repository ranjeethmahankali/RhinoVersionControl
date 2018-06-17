using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore.VersionManagement
{
    public class ChangeSet: Entity
    {
        #region fields
        private Dictionary<Guid, IChange> _changeDict = null;
        #endregion

        #region properties
        public RvcVersion VersionBefore { get; internal set; }
        public RvcVersion VersionAfter { get; internal set; }
        public Dictionary<Guid, IChange> ChangesMap
        {
            get
            {
                if(_changeDict == null) { _changeDict = new Dictionary<Guid, IChange>(); }
                return _changeDict;
            }
        }
        #endregion

        #region methods
        public void AddChange(IChange change)
        {
            if (_changeDict.ContainsKey(change.Id))
            {
                _changeDict[change.Id] = change;
            }
            else
            {
                _changeDict.Add(change.Id, change);
            }
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

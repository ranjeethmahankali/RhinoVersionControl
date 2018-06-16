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
        private Dictionary<Guid, IChange> _changeDict = new Dictionary<Guid, IChange>();
        #endregion

        #region properties
        public Version VersionBefore { get; internal set; }
        public Version VersionAfter { get; internal set; }
        #endregion

        #region methods
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

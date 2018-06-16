using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore.VersionManagement
{
    public class Addition<SetT> : Change<SetT>
    {
        #region fields

        #endregion

        #region constructors
        protected Addition(Func<SetT, SetT> applyChange, Func<SetT, SetT> rollbackChange) : base(applyChange, rollbackChange)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region methods

        #endregion
    }
}

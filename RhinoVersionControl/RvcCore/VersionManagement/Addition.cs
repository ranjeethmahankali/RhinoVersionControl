using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.DocObjects;

namespace RvcCore.VersionManagement
{
    public class Addition<T, SetT> : Change<T, SetT>
        where SetT: File3dmCommonComponentTable<T>
        where T : ModelComponent
    {
        #region fields
        #endregion

        #region constructors
        protected Addition(Guid addedGuid) : base(addedGuid, 
            (set) => {
                //return 
                //incomplete
                throw new NotImplementedException();
            }, 
            (set) => {
                //incomplete
                throw new NotImplementedException();
            })
        {
            throw new NotImplementedException();
        }
        #endregion

        #region methods
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvcCore.VersionManagement;

namespace RvcCore.RvcDataManagement
{
    /// <summary>
    /// This class captures the state of a rhino file. This is used in the version management to track changes, perform diffs etc.
    /// </summary>
    public class FileState: Entity
    {
        #region fields
        #endregion

        #region properties
        public DataStore DataStore { get; internal set; }
        #endregion

        #region methods
        public FileState ApplyChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            //apply the change set to the result state before returning it
            //incomplete
            throw new NotImplementedException();
            //return result;
        }

        public override object Clone()
        {
            //incomplete
            throw new NotImplementedException();
        }

        public FileState RollbackChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            //apply the change set to the result state before returning it
            //incomplete
            throw new NotImplementedException();
            //return result;
        }

        public bool ApplyChange(IChange change)
        {
            //incomplete
            throw new NotImplementedException();
        }

        public bool RollbackChange(IChange change)
        {
            //incomplete
            throw new NotImplementedException();
        }

        public T ObjectLookup<T>(Guid id)
        {
            return DataStore.ObjectLookup<T>(id);
        }
        #endregion
    }
}

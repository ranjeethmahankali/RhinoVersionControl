using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RvcCore.VersionManagement;
using Rhino.DocObjects;

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
        public DataStore Store { get; internal set; }
        public RvcVersion Version { get; set; }
        #endregion

        #region methods
        public FileState ApplyChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            foreach (var id in changes.ChangesMap.Keys)
            {
                result.RollbackChange(changes.ChangesMap[id]);
            }
            return result;
        }

        public FileState RollbackChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            //apply the change set to the result state before returning it
            foreach(var id in changes.ChangesMap.Keys)
            {
                result.ApplyChange(changes.ChangesMap[id]);
            }
            return result;
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
            where T : ModelComponent
        {
            return Store.ObjectLookup<T>(id);
        }

        public override object Clone()
        {
            //incomplete
            throw new NotImplementedException();
        }

        public static ChangeSet EvaluateDiff(FileState state1, FileState state2)
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}

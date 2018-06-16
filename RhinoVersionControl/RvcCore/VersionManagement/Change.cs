using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.DocObjects;

namespace RvcCore.VersionManagement
{
    /// <summary>
    /// This class represents a single change made to a collection of objects being tracked. This is the base class for all types of changes.
    /// </summary>
    /// <typeparam name="SetT">The type of the collection that is being tracked by this change</typeparam>
    public abstract class Change<T, SetT>: Entity, IChange
        where SetT: File3dmCommonComponentTable<T>
        where T: ModelComponent
    {
        #region fields
        private Func<SetT, SetT> _applyChangeLambda, _rollbackChangeLambda;
        private Guid _changedObjGuid;
        #endregion

        #region properties
        public Type ObjectType { get => typeof(T); }
        public Type CollectionType { get => typeof(SetT); }
        public ChangeSet ContainingSet { get; internal set; }
        //public Version VersionBefore { get => ContainingSet.VersionBefore; }
        #endregion

        #region constructors
        protected Change(Guid changedGuid, Func<SetT, SetT> applyChange, Func<SetT, SetT> rollbackChange)
        {
            _applyChangeLambda = applyChange;
            _rollbackChangeLambda = rollbackChange;
            _changedObjGuid = changedGuid;
        }
        #endregion

        #region methods
        public virtual SetT ApplyTo(SetT set)
        {
            if(_applyChangeLambda == null)
            {
                return set;
            }
            return _applyChangeLambda.Invoke(set);
        }

        public virtual SetT RollbackFrom(SetT set)
        {
            if(_rollbackChangeLambda == null)
            {
                return set;
            }
            return _rollbackChangeLambda.Invoke(set);
        }
        #endregion
    }
}

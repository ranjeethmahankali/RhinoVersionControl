using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.DocObjects;

namespace RvcCore.VersionManagement
{
    public enum ChangeType
    {
        Addition,
        Deletion,
        Modification
    }

    public interface IChange : IEntity
    {
        Type ObjectType { get; }
        Type CollectionType { get; }
        ChangeType Type { get; }
    }

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
        private Guid _cachedObjGuid = Guid.Empty;
        #endregion

        #region properties
        public Type ObjectType { get => typeof(T); }
        public Type CollectionType { get => typeof(SetT); }
        public ChangeSet ContainingSet { get; internal set; }
        public Guid AffectedObjectGuid { get; private set; }
        public ChangeType Type { get; internal set; }
        /// <summary>
        /// If the change type is modification, this object will contain the replacement
        /// </summary>
        public Guid CachedObjectGuid
        {
            get
            {
                if(_cachedObjGuid == Guid.Empty)
                {
                    return AffectedObjectGuid;
                }
                return _cachedObjGuid;
            }
        }
        //public Version VersionBefore { get => ContainingSet.VersionBefore; }
        #endregion

        #region constructors
        private Change(Version version, Guid changedGuid, Func<SetT, SetT> applyChange, Func<SetT, SetT> rollbackChange)
        {
            _applyChangeLambda = applyChange;
            _rollbackChangeLambda = rollbackChange;
            AffectedObjectGuid = changedGuid;
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

        //static constructors
        public static Change<Q, SetQ> CreateAddition<Q, SetQ>(Version affectedVersion, Guid addedObjGuid)
            where SetQ : File3dmCommonComponentTable<Q>
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }

        public static Change<Q, SetQ> CreateDeletion<Q, SetQ>(Version affectedVersion, Guid deletedObjGuid)
            where SetQ : File3dmCommonComponentTable<Q>
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }

        public static Change<Q, SetQ> CreateModification<Q, SetQ>(Version affectedVersion, Guid originalObjGuid, Guid modifiedObjGuid)
            where SetQ : File3dmCommonComponentTable<Q>
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}

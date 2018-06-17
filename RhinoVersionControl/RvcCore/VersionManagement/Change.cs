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
        None,
        Addition,
        Deletion,
        Modification
    }

    public interface IChange : IEntity
    {
        Type ObjectType { get; }
        ChangeType Type { get; }
    }

    /// <summary>
    /// This class represents a single change made to a collection of objects being tracked. This is the base class for all types of changes.
    /// </summary>
    /// <typeparam name="SetT">The type of the collection that is being tracked by this change</typeparam>
    public abstract class Change<T>: Entity, IChange
        where T: ModelComponent
    {
        #region fields
        //if this change type happens to be a modification, then the below two objects will store the initial and final versions of the modification
        private Guid _modifiedInitialVersion = Guid.Empty;
        private Guid _modifiedFinalVersion = Guid.Empty;
        #endregion

        #region properties
        public Type ObjectType { get => typeof(T); }
        public ChangeSet ContainingSet { get; internal set; }
        public Guid AffectedObjectGuid { get; private set; }
        public ChangeType Type { get; internal set; }
        /// <summary>
        /// If the change type is modification, this object will contain the initial version of the modification
        /// </summary>
        public Guid ModificationInitialVersion
        {
            get
            {
                if(_modifiedInitialVersion == Guid.Empty)
                {
                    return AffectedObjectGuid;
                }
                return _modifiedInitialVersion;
            }
        }
        /// <summary>
        /// If the change type is a modification, this will contain the final version of the modification
        /// </summary>
        public Guid ModificationFinalVersion
        {
            get
            {
                if (_modifiedFinalVersion == Guid.Empty)
                {
                    return AffectedObjectGuid;
                }
                return _modifiedFinalVersion;
            }
        }
        //public Version VersionBefore { get => ContainingSet.VersionBefore; }
        #endregion

        #region constructors
        private Change(RvcVersion version, Guid changedGuid)
        {
            AffectedObjectGuid = changedGuid;
        }
        #endregion

        #region methods
        //static constructors
        public static Change<Q> CreateAddition<Q>(RvcVersion affectedVersion, Guid addedObjGuid)
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }

        public static Change<Q> CreateDeletion<Q>(RvcVersion affectedVersion, Guid deletedObjGuid)
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }

        public static Change<Q> CreateModification<Q>(RvcVersion affectedVersion, Guid affectedObject, Guid initialVersion,
            Guid finalVersion)
            where Q : ModelComponent
        {
            //incomplete
            throw new NotImplementedException();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.DocObjects;
using RvcCore.Serialization;
using Newtonsoft.Json;

namespace RvcCore.VersionManagement
{
    public enum ChangeType
    {
        None,
        Addition,
        Deletion,
        Modification
    }

    [JsonConverter(typeof(ChangeSerializer))]
    public interface IChange : IEntity
    {
        Type ObjectType { get; }
        ChangeType Type { get; }
        Guid ModificationInitialVersion { get; }
        Guid ModificationFinalVersion { get; }
        ChangeSet ContainingSet { get; set; }
        Guid AffectedObjectGuid { get; set; }
        //Guid AffectedObjectAliasId { get; set; }
    }

    /// <summary>
    /// This class represents a single change made to a collection of objects being tracked. This is the base class for all types of changes.
    /// </summary>
    /// <typeparam name="SetT">The type of the collection that is being tracked by this change</typeparam>
    [JsonConverter(typeof(ChangeSerializer))]
    public class Change<T>: Entity, IChange
        where T: ModelComponent
    {
        #region fields
        //if this change type happens to be a modification, then the below two objects will store the initial and final versions of the modification
        private Guid _modifiedInitialVersion = Guid.Empty;
        private Guid _modifiedFinalVersion = Guid.Empty;
        #endregion

        #region properties
        public Type ObjectType { get => typeof(T); }
        public ChangeSet ContainingSet { get; set; }
        public Guid AffectedObjectGuid { get; set; }
        //public Guid AffectedObjectAliasId { get; set; }
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
            set { _modifiedInitialVersion = value; }
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
            set { _modifiedFinalVersion = value; }
        }
        //public Version VersionBefore { get => ContainingSet.VersionBefore; }
        #endregion

        #region constructors
        private Change(Guid changedGuid)
        {
            AffectedObjectGuid = changedGuid;
        }
        #endregion

        #region methods
        //static constructors
        public static Change<Q> CreateAddition<Q>(Guid addedObjGuid)
            where Q : ModelComponent
        {
            Change<Q> change = new Change<Q>(addedObjGuid);
            change.Type = ChangeType.Addition;
            return change;
        }

        public static Change<Q> CreateDeletion<Q>(Guid deletedObjGuid)
            where Q : ModelComponent
        {
            Change<Q> change = new Change<Q>(deletedObjGuid);
            change.Type = ChangeType.Deletion;
            return change;
        }

        public static Change<Q> CreateModification<Q>(Guid affectedObject, Guid initialVersion,
            Guid finalVersion)
            where Q : ModelComponent
        {
            Change<Q> change = new Change<Q>(affectedObject);
            change.Type = ChangeType.Modification;
            change.ModificationInitialVersion = initialVersion;
            change.ModificationFinalVersion = finalVersion;
            return change;
        }

        public override object Clone()
        {
            Change<T> clone = new Change<T>(AffectedObjectGuid);
            clone.ModificationInitialVersion = ModificationInitialVersion;
            clone.ModificationFinalVersion = ModificationFinalVersion;
            clone.Type = Type;
            return clone;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.FileIO;
using Rhino.DocObjects;
using RvcCore.Util;
using RvcCore.VersionManagement;

namespace RvcCore.RvcDataManagement
{
    public interface IFileDataTable: ICloneable
    {
        HashSet<Guid> Objects { get; }
        FileState State { get; set; }
        DataStore Store { get; }
        ModelComponent GetModelComponent(Guid id, bool getOriginalVersion = false);
        bool ObjectHasAlias(Guid id, out Guid alias);
        FileDataTable<T> AsTableInstance<T>() where T: ModelComponent;
        ChangeSet EvaluateDiff(IFileDataTable table);
        Type MemberType { get; }
        string Name { get; set; }
        bool ApplyChange(IChange change);
        bool RollbackChange(IChange change);
        IFileDataTable ApplyChangeSet(ChangeSet changes);
        IFileDataTable RollbackChangeSet(ChangeSet changes);
        bool Contains(Guid id);
        bool Delete(Guid id);
        void Add(Guid id);
        void Add(Guid id, Guid alias);
    }
    /// <summary>
    /// A FileState is a collection of tables imported from the 3dm file. This is the class that represents the individual tables in the file
    /// </summary>
    public class FileDataTable<T>: IFileDataTable
        where T: ModelComponent
    {
        #region fields
        private HashSet<Guid> _objects = null;
        /// <summary>
        /// When objects are edited their original versions and the edited version both have to be saved in the same data store. To avoid 
        /// guid duplication in the data store, a new guid (alias) is assigned to the edited object, and then added to the datastore. The relationship
        /// between the original guid and the alias is stored in this dictionary.
        /// </summary>
        private Dictionary<Guid, Guid> _aliases = null;
        #endregion

        #region properties
        public HashSet<Guid> Objects
        {
            get
            {
                if(_objects == null) { _objects = new HashSet<Guid>(); }
                return _objects;
            }
        }
        public FileState State { get; set; }
        public DataStore Store { get => State.Store; }
        public Type MemberType { get => typeof(T); }
        public string Name { get; set; }
        #endregion

        #region constructors
        public FileDataTable()
        {
            _objects = new HashSet<Guid>();
            _aliases = new Dictionary<Guid, Guid>();
        }
        public FileDataTable(FileState state, string name): this()
        {
            State = state;
            Name = name;
        }
        public FileDataTable(FileState state, File3dmCommonComponentTable<T> rhTable, string name): this(state, name)
        {
            List<T> comps = TableUtil.ToList(rhTable);
            _objects = new HashSet<Guid>(comps.Select((mc) => mc.Id));
        }
        #endregion

        #region methods
        public ModelComponent GetModelComponent(Guid id, bool getOriginalversion = false)
        {
            return GetObject(id, getOriginalversion);
        }
        public T GetObject(Guid id, bool getOriginalVersion = false)
        {
            if (!_objects.Contains(id)) { return null; }
            Guid objId;
            if (!(getOriginalVersion && ObjectHasAlias(id, out objId)))
            {
                objId = id;
            }
            return Store.ObjectLookupOfType<T>(objId);
        }
        public bool ObjectHasAlias(Guid id, out Guid alias)
        {
            var hasAlias = _aliases.TryGetValue(id, out alias);
            if (!hasAlias) { alias = id; }
            return hasAlias;
        }
        public void AddObjectAlias(Guid id, Guid alias)
        {
            if (_aliases.ContainsKey(id))
            {
                _aliases[id] = alias;
            }
            else
            {
                _aliases.Add(id, alias);
            }
        }

        public bool Contains(Guid id)
        {
            return Objects.Contains(id);
        }
        public bool Delete(Guid id)
        {
            if (Contains(id)) { Objects.Remove(id); }
            else { return false; }
            if (_aliases.ContainsKey(id)) { _aliases.Remove(id); }
            return true;
        }
        public void Add(Guid id)
        {
            Objects.Add(id);
        }
        public void Add(Guid id, Guid alias)
        {
            Objects.Add(id);
            AddObjectAlias(id, alias);
        }

        public FileDataTable<Q> AsTableInstance<Q>() where Q: ModelComponent
        {
            if(typeof(T) == typeof(Q)) { return this as FileDataTable<Q>; }
            else { return null; }
        }

        public static ChangeSet EvaluateDiff(IFileDataTable table1, IFileDataTable table2)
        {
            var memberType1 = table1.MemberType;
            var memberType2 = table2.MemberType;
            if(table1.GetType().GetGenericTypeDefinition() != typeof(FileDataTable<>) || 
                table2.GetType().GetGenericTypeDefinition() != typeof(FileDataTable<>) || memberType1 != memberType2 ||
                typeof(T) != memberType1)
            {
                throw new InvalidOperationException("Diff operation cannot be performed on tables of these types");
            }
            return EvaluateDiff(table1.AsTableInstance<T>(), table2.AsTableInstance<T>());
        }

        public ChangeSet EvaluateDiff(IFileDataTable table2)
        {
            return EvaluateDiff(this, table2);
        }

        /// <summary>
        /// Evaluates the diff between the two tables and returns the changeset that would transform the first table into the second
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static ChangeSet EvaluateDiff(FileDataTable<T> t1, FileDataTable<T> t2)
        {
            Dictionary<Guid, ChangeType> changeMap = new Dictionary<Guid, ChangeType>();
            Dictionary<Guid, Guid> aliasMap1 = new Dictionary<Guid, Guid>(), aliasMap2 = new Dictionary<Guid, Guid>();
            //we start by assuming the worst case of changes - i.e. maximum changes
            //we assume that all the old objects are deleted
            foreach(var id1 in t1.Objects)
            {
                changeMap.Add(id1, ChangeType.Deletion);
            }
            //we assume that all the objects in teh new file are newly added
            foreach (var id2 in t2.Objects)
            {
                if (changeMap.ContainsKey(id2))
                {
                    //both tables contain this object
                    //next we check to see if they have the same version of that object or has it been edited
                    Guid alias1, alias2;
                    t1.ObjectHasAlias(id2, out alias1);
                    t2.ObjectHasAlias(id2, out alias2);
                    if (alias1.Equals(alias2))
                    {
                        //they both contain the same version of the same object
                        changeMap[id2] = ChangeType.None;
                    }
                    else
                    {
                        //they have different versions of that object
                        changeMap[id2] = ChangeType.Modification;
                        aliasMap1.Add(id2, alias1);
                        aliasMap2.Add(id2, alias2);
                    }
                }
                else
                {
                    //this object is new, and has been added to t2
                    changeMap.Add(id2, ChangeType.Addition);
                }
            }

            //now creating the changeset from the dictionary
            ChangeSet changeSet = new ChangeSet();
            foreach(var id in changeMap.Keys)
            {
                ChangeType changeType = changeMap[id];
                if (changeType == ChangeType.None) { continue; }
                Change<T> change = null;
                if(changeType == ChangeType.Addition)
                {
                    change = Change<T>.CreateAddition<T>(id);
                }
                else if(changeType == ChangeType.Deletion)
                {
                    change = Change<T>.CreateDeletion<T>(id);
                }
                else if(changeType == ChangeType.Modification)
                {
                    change = Change<T>.CreateModification<T>(id, aliasMap1[id], aliasMap2[id]);
                }
                if (change != null) { changeSet.AddChange(change); }
            }

            return changeSet;
        }

        public object Clone()
        {
            FileDataTable<T> clone = new FileDataTable<T>(State, Name);
            foreach(var id in _objects)
            {
                clone.Objects.Add(id);
            }
            foreach(var key in _aliases.Keys)
            {
                clone.AddObjectAlias(key, _aliases[key]);
            }
            return clone;
        }

        public bool ApplyChange(IChange change)
        {
            if(MemberType != change.ObjectType) { return false; }

            if(change.Type == ChangeType.Addition)
            {
                Add(change.AffectedObjectGuid);
                return true;
            }
            if (change.Type == ChangeType.Deletion)
            {
                return Delete(change.AffectedObjectGuid);
            }
            if (Contains(change.AffectedObjectGuid) && change.Type == ChangeType.Modification)
            {
                AddObjectAlias(change.AffectedObjectGuid, change.ModificationFinalVersion);
                return true;
            }

            return false;
        }

        public bool RollbackChange(IChange change)
        {
            if (MemberType != change.ObjectType) { return false; }

            if (change.Type == ChangeType.Addition)
            {
                return Delete(change.AffectedObjectGuid);
            }
            if (change.Type == ChangeType.Deletion)
            {
                Add(change.AffectedObjectGuid);
                return true;
            }
            if (Contains(change.AffectedObjectGuid) && change.Type == ChangeType.Modification)
            {
                AddObjectAlias(change.AffectedObjectGuid, change.ModificationInitialVersion);
                return true;
            }
            return false;
        }

        public IFileDataTable ApplyChangeSet(ChangeSet changes)
        {
            IFileDataTable copy = (IFileDataTable)Clone();
            foreach(var key in changes.ChangesMap.Keys)
            {
                copy.ApplyChange(changes.ChangesMap[key]);
            }
            return copy;
        }

        public IFileDataTable RollbackChangeSet(ChangeSet changes)
        {
            IFileDataTable copy = (IFileDataTable)Clone();
            foreach (var key in changes.ChangesMap.Keys)
            {
                copy.RollbackChange(changes.ChangesMap[key]);
            }
            return copy;
        }
        #endregion
    }
}

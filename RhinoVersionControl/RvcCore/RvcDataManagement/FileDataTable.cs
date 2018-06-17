using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.FileIO;
using Rhino.DocObjects;
using RvcCore.Util;
using RvcCore.VersionManagement;

namespace RvcCore.RvcDataManagement
{
    /// <summary>
    /// A FileState is a collection of tables imported from the 3dm file. This is the class that represents the individual tables in the file
    /// </summary>
    public class FileDataTable<T> where T: ModelComponent
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
        public DataStore DataStore { get => State.DataStore; }
        #endregion

        #region constructors
        public FileDataTable()
        {
            _objects = new HashSet<Guid>();
        }
        public FileDataTable(FileState state, File3dmCommonComponentTable<T> rhTable)
        {
            List<T> comps = TableUtil.ToList(rhTable);
            _objects = new HashSet<Guid>(comps.Select((mc) => mc.Id));
            State = state;
        }
        #endregion

        #region methods
        public T GetObject(Guid id, bool getOriginalVersion = false)
        {
            if (!_objects.Contains(id)) { return null; }
            Guid objId;
            if (!(getOriginalVersion && ObjectHasAlias(id, out objId)))
            {
                objId = id;
            }
            return DataStore.ObjectLookup<T>(objId);
        }
        private bool ObjectHasAlias(Guid id, out Guid alias)
        {
            var hasAlias = _aliases.TryGetValue(id, out alias);
            if (!hasAlias) { alias = id; }
            return hasAlias;
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
                    change = Change<T>.CreateAddition<T>(t1.State.Version, id);
                }
                else if(changeType == ChangeType.Deletion)
                {
                    change = Change<T>.CreateDeletion<T>(t1.State.Version, id);
                }
                else if(changeType == ChangeType.Modification)
                {
                    change = Change<T>.CreateModification<T>(t1.State.Version, id, aliasMap1[id], aliasMap2[id]);
                }
                if (change != null) { changeSet.AddChange(change); }
            }

            return changeSet;
        }
        #endregion
    }
}

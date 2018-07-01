using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
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
        private HashSet<IFileDataTable> _tables { get; }
        #endregion

        #region properties
        [JsonIgnore]
        public DataStore Store { get; internal set; }
        [JsonIgnore]
        public RvcVersion Version { get; set; }
        #endregion

        #region constructors
        internal FileState()
        {
            _tables = new HashSet<IFileDataTable>();
        }
        public FileState(DataStore store, RvcVersion version): this()
        {
            Store = store;
            Version = version;
        }
        public FileState(FileState old, ChangeSet changes)
        {
            FileState changed = old.ApplyChangeSet(changes);
            foreach (var table in _tables)
            {
                _tables.Add((IFileDataTable)table.Clone());
            }
            Store = Store;
        }
        #endregion

        #region methods
        public void AddTable(IFileDataTable table)
        {
            table.State = this;
            _tables.Add(table);
        }
        public FileState ApplyChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            foreach (var id in changes.ChangesMap.Keys)
            {
                result.ApplyChange(changes.ChangesMap[id]);
            }
            return result;
        }

        public FileState RollbackChangeSet(ChangeSet changes)
        {
            FileState result = (FileState)Clone();
            //apply the change set to the result state before returning it
            foreach(var id in changes.ChangesMap.Keys)
            {
                result.RollbackChange(changes.ChangesMap[id]);
            }
            return result;
        }

        public bool ApplyChange(IChange change)
        {
            IFileDataTable matchingTable = GetMatchingTable(change.ObjectType);
            if(matchingTable == null) { return false; }
            return matchingTable.ApplyChange(change);
        }

        public bool RollbackChange(IChange change)
        {
            IFileDataTable matchingTable = GetMatchingTable(change.ObjectType);
            if (matchingTable == null) { return false; }
            return matchingTable.RollbackChange(change);
        }

        public T ObjectLookup<T>(Guid id)
            where T : ModelComponent
        {
            return Store.ObjectLookupOfType<T>(id);
        }

        public override object Clone()
        {
            FileState clone = new FileState();
            foreach(var table in _tables)
            {
                clone._tables.Add((IFileDataTable)table.Clone());
            }
            clone.Store = Store;
            clone.Version = Version;
            return clone;
        }

        public IFileDataTable GetMatchingTable(IFileDataTable table)
        {
            return GetMatchingTable(table.MemberType, table.Name);
        }
        public IFileDataTable GetMatchingTable(Type memberType, string name = null)
        {
            List<IFileDataTable> matches = new List<IFileDataTable>();
            foreach (var t in _tables)
            {
                if (t.MemberType == memberType) { matches.Add(t); }
                if(name != null && t.Name == name) { return matches.Last(); }
            }

            var preferredMatches = matches.Where((t) => t.Name.Contains("All")).ToList();
            if(preferredMatches.Count > 0) { return preferredMatches.FirstOrDefault(); }
            else { return matches.FirstOrDefault(); }
        }

        public static ChangeSet EvaluateDiff(FileState state1, FileState state2)
        {
            ChangeSet changeSet = new ChangeSet();
            foreach(var table1 in state1._tables)
            {
                Type memberType = table1.MemberType;
                var table2 = state2.GetMatchingTable(table1);
                if(table2 == null)
                {
                    throw new InvalidDataException("Could not find a matching table. Aborting the diff operation.");
                }
                changeSet = ChangeSet.Merge(changeSet, table1.EvaluateDiff(table2));
            }
            return changeSet;
        }
        #endregion
    }
}

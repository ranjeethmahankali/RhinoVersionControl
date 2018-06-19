using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        public HashSet<IFileDataTable> Tables { get; }
        #endregion

        #region properties
        public DataStore Store { get; internal set; }
        public RvcVersion Version { get; set; }
        #endregion

        #region constructors
        private FileState()
        {
            Tables = new HashSet<IFileDataTable>();
        }
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
            return Store.ObjectLookup<T>(id);
        }

        public override object Clone()
        {
            FileState clone = new FileState();
            foreach(var table in Tables)
            {
                clone.Tables.Add((IFileDataTable)table.Clone());
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
            foreach (var t in Tables)
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
            foreach(var table1 in state1.Tables)
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

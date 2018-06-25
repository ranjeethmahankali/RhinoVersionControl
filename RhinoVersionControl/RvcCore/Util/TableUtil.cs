using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.Runtime;
using Rhino.DocObjects;
using RvcCore.RvcDataManagement;
using RvcCore.VersionManagement;

namespace RvcCore.Util
{
    public class TableUtil
    {
        /// <summary>
        /// For some reason Rhino3dmIO tables don't implement the linq methods, so we need this to manually convert stuff
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(File3dmCommonComponentTable<T> table)
            where T : ModelComponent
            //where TableT: IEnumerable<T>
        {
            List<T> list = new List<T>();
            foreach (var obj in table)
            {
                T copy = obj;
                copy.EnsurePrivateCopy();
                list.Add(copy);
            }
            return list;
        }

        public static IFileDataTable ParseTableData(IEnumerable table3dm, string tableName, Type dataType, RvcVersion version, DataStore store,
            out ChangeSet changes)
        {
            IFileDataTable refTable = version.State.GetMatchingTable(dataType, tableName);
            if(refTable == null)
            {
                refTable = CreateTableInstance(dataType, tableName);
                version.State.AddTable(refTable);
                version.State.Store = store;
            }
            changes = EvaluateDiffWith3dmData(refTable, table3dm, ref store);
            return refTable.ApplyChangeSet(changes);
        }

        public static IFileDataTable CreateTableInstance(Type memberT, string tableName = null)
        {
            if (!typeof(ModelComponent).IsAssignableFrom(memberT))
            {
                throw new NotImplementedException("Invalid member data type for creating the table");
            }
            Type generic = typeof(FileDataTable<>);
            Type[] typeArgs = new Type[] { memberT };
            Type combined = generic.MakeGenericType(typeArgs);

            IFileDataTable table = (IFileDataTable)Activator.CreateInstance(combined);
            table.Name = tableName;
            return table;
        }

        public static ChangeSet EvaluateDiffWith3dmData(IFileDataTable table, IEnumerable table3dm, ref DataStore store)
        {
            Type tableObjectType = null;
            ChangeSet changes = new ChangeSet();
            Dictionary<Guid, ModelComponent> table3dmCached = new Dictionary<Guid, ModelComponent>();
            HashSet<Guid> objGuidsWithMatches = new HashSet<Guid>();
            var i = table3dm.GetEnumerator();
            while (i.MoveNext())
            {
                ModelComponent comp = (ModelComponent)i.Current;
                if(tableObjectType == null) { tableObjectType = comp.GetType(); }
                //caching this for future use
                table3dmCached.Add(comp.Id, comp);
                //now doing the comparison
                if (table.Contains(comp.Id))
                {
                    objGuidsWithMatches.Add(comp.Id);
                    ModelComponent obj = table.GetModelComponent(comp.Id);
                    if(!obj.Equals(comp))
                    {
                        //object has been modified
                        Guid initialVersion;
                        if (!table.ObjectHasAlias(comp.Id, out initialVersion))
                        {
                            initialVersion = comp.Id;
                        }
                        //adding the new version of the object to the store
                        Guid finalVersion = store.AddObject(comp, true);
                        //create modification type change from initial version to final version
                        changes.AddChange(ChangeType.Modification, tableObjectType, comp.Id, initialVersion, finalVersion);
                    }
                }
                else
                {
                    //create addition change
                    changes.AddChange(ChangeType.Addition, tableObjectType, comp.Id);
                }
            }

            //finding the objects in the original tables whose matches were not found
            var deletedGuids = table.Objects.Except(objGuidsWithMatches);
            foreach(var deleted in deletedGuids)
            {
                changes.AddChange(ChangeType.Deletion, tableObjectType, deleted);
            }

            return changes;
        }

        public static bool IsFile3dmTableType(Type type, out Type memberType)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (typeof(File3dmCommonComponentTable<>) == cur)
                {
                    memberType = type.GetGenericArguments().Single();
                    return true;
                }
                type = type.BaseType;
            }
            memberType = null;
            return false;
        }

        public static bool IsFile3dmTableType(Type type)
        {
            Type memberType;
            return IsFile3dmTableType(type, out memberType);
        }
    }
}

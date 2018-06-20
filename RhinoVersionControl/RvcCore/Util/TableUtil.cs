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

        public static IFileDataTable ParseTableData(IEnumerable tableData, string tableName, Type dataType, RvcVersion version, ref DataStore store)
        {
            IFileDataTable table = CreateTableInstance(dataType);
            table.Name = tableName;

            IFileDataTable refTable = version.State.GetMatchingTable(table);
            //if = 
            //incomplete
            throw new NotImplementedException();
        }

        public static IFileDataTable CreateTableInstance(Type memberT)
        {
            if (!typeof(ModelComponent).IsAssignableFrom(memberT))
            {
                throw new NotImplementedException("Invalid member data type for creating the table");
            }
            Type generic = typeof(FileDataTable<>);
            Type[] typeArgs = new Type[] { memberT };
            Type combined = generic.MakeGenericType(typeArgs);

            return (IFileDataTable)Activator.CreateInstance(combined);
        }

        public ChangeSet EvaluateDiffWith3dmData(IFileDataTable table, IEnumerable table3dm, DataStore store)
        {
            var i = table3dm.GetEnumerator();
            while (i.MoveNext())
            {
                ModelComponent comp = (ModelComponent)i.Current;
                if (table.Contains(comp.Id))
                {
                    ModelComponent old = store.ObjectLookup(comp.Id, comp.GetType());
                    if(old == null)
                    {
                        //create addition change
                    }
                    else
                    {
                        //ModelComponent newOne = table.GetModelComponent(comp.Id);
                    }
                }
            }
            //incomplete
            throw new NotImplementedException();
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

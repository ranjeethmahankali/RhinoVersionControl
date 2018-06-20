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

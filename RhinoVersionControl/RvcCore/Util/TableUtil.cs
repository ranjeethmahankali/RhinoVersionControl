using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using Rhino.Runtime;

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
        public static List<T> ToList<T>(IEnumerable<T> table)
            where T : CommonObject
            //where TableT: IEnumerable<T>
        {
            List<T> list = new List<T>();
            foreach (var obj in table)
            {
                obj.EnsurePrivateCopy();
                list.Add(obj);
            }
            return list;
        }
    }
}

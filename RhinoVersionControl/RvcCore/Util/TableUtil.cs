﻿using System;
using System.Collections.Generic;
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
    }
}

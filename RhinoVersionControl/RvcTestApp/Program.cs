using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;

using RvcCore.Util;
using Newtonsoft.Json;
using RvcCore.VersionManagement;
using RvcCore.RvcDataManagement;

namespace RvcTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\ranje\Desktop\RvcTest\testFile.3dm";
            //ChangeSet changes;
            //FileState state1 = FileUtil.ParseFile(path, out changes);

            //Console.WriteLine("Please edit the contents of the file and save it");
            //Console.ReadKey();
            //ChangeSet newChanges;
            //FileState state2 = FileUtil.ParseFile(path, out newChanges);

            //ChangeSet diff = FileState.EvaluateDiff(state1, state2);
                
            //reading the file for the first time
            using (var file = File3dm.Read(path))
            {
                var objList1 = TableUtil.ToList(file.Objects);
                var obj1 = objList1.First();
                Console.Write("Please edit the file and save it.");
                Console.ReadKey();
                using (var file2 = File3dm.Read(path))
                {
                    var objList2 = TableUtil.ToList(file2.Objects);
                    var obj2 = objList2.First();
                    bool equal = obj1.Equals(obj2);
                    //bool customEq = CustomEquals(obj1, obj2);
                }
            }
            Console.ReadKey();
        }
    }
}

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
            ChangeSet changes;
            FileState state1 = FileUtil.ParseFile(path, out changes);

            Console.WriteLine("Please edit the contents of the file and save it");
            Console.ReadKey();
            ChangeSet newChanges;
            FileState state2 = FileUtil.ParseFile(path, out newChanges);

            ChangeSet diff = FileState.EvaluateDiff(state1, state2);
            Console.ReadKey();
        }
    }
}

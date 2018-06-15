using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.FileIO;
using RvcCore.Util;
using Newtonsoft.Json;

namespace RvcTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\ranje\Desktop\testFile.3dm";
            List<double> nums = new List<double>() { 1,2,3,4,5 };
            var first = nums.First();
            List<File3dmObject> objs, newObjs;
            string oldOne, newOne;
            using (File3dm file = File3dm.Read(path))
            {
                objs = TableUtil.ToList(file.Objects);
                oldOne = JsonConvert.SerializeObject(objs);
            }
            Console.WriteLine("Please edit the contents of the file and save it");
            Console.ReadKey();
            using (File3dm file = File3dm.Read(path))
            {
                newObjs = TableUtil.ToList(file.Objects);
                newOne = JsonConvert.SerializeObject(newObjs);
            }

            objs = JsonConvert.DeserializeObject<List<File3dmObject>>(oldOne);
            newObjs = JsonConvert.DeserializeObject<List<File3dmObject>>(newOne);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore.RvcDataManagement
{
    /// <summary>
    /// Stores all the raw data from all versions of a file, to be used by the version management as neccessary
    /// </summary>
    public class DataStore
    {
        #region fields
        #endregion

        #region properties
        public string StoreFileName { get; private set; }
        public string StoreDirectory { get; internal set; }
        #endregion
    }
}

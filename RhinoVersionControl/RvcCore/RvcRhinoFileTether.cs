using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore
{
    /// <summary>
    /// An instance of this class will represent the connection between a Rhino file and an rvc archive that is managing the versions
    /// of that file. This instance will be serialized and saved to the string table of the Rhino doc.
    /// </summary>
    public class RvcRhinoFileTether
    {
        #region fields

        #endregion

        #region properties
        /// <summary>
        /// A This id will be used as a name for the directory and the data store file in the archive of the data
        /// </summary>
        public Guid RvcId { get; set; }
        /// <summary>
        /// Within the archives of a file, there might be several versions and given file may be tethered to any version. This property stores that
        /// information.
        /// </summary>
        public Guid VersionId { get; set; }
        #endregion

        #region constructors

        #endregion

        #region methods

        #endregion
        //incomplete
    }
}

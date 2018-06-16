using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore.VersionManagement
{
    public interface IChange: IEntity
    {
        Type ObjectType { get; }
        Type CollectionType { get; }
    }
}

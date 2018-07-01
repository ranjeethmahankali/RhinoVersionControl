using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore
{
    public interface IEntity: ICloneable
    {
        Guid Id { get; set; }
    }
    [Serializable]
    public abstract class Entity: IEntity
    {
        internal static string ID_PROP_NAME = "id";
        public Guid Id { get; set; }
        public abstract object Clone();
        internal Entity()
        {
            Id = Guid.NewGuid();
        }
    }
}

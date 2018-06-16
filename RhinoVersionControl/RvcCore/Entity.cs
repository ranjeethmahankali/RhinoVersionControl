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

    public abstract class Entity: IEntity
    {
        public Guid Id { get; set; }
        public abstract object Clone();
    }
}

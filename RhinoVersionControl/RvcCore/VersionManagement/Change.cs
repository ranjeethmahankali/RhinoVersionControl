using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvcCore.VersionManagement
{
    /// <summary>
    /// This class represents a single change made to a collection of objects being tracked. This is the base class for all types of changes.
    /// </summary>
    /// <typeparam name="SetT">The type of the collection that is being tracked by this change</typeparam>
    public abstract class Change<SetT>
    {
        #region fields
        private Func<SetT, SetT> _applyChangeLambda, _rollbackChangeLambda;
        #endregion

        #region properties
        public Guid Id { get; internal set; }
        #endregion

        #region constructors
        protected Change(Func<SetT, SetT> applyChange, Func<SetT, SetT> rollbackChange)
        {
            _applyChangeLambda = applyChange;
            _rollbackChangeLambda = rollbackChange;
        }
        #endregion

        #region methods
        public virtual SetT ApplyTo(SetT set)
        {
            if(_applyChangeLambda == null)
            {
                return set;
            }
            return _applyChangeLambda.Invoke(set);
        }

        public virtual SetT RollbackFrom(SetT set)
        {
            if(_rollbackChangeLambda == null)
            {
                return set;
            }
            return _rollbackChangeLambda.Invoke(set);
        }
        #endregion
    }
}

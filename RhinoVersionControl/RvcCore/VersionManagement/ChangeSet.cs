using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Rhino.FileIO;
using Rhino.DocObjects;

namespace RvcCore.VersionManagement
{
    public class ChangeSet: Entity
    {
        #region fields
        private Dictionary<Guid, IChange> _changeDict = null;
        #endregion

        #region properties
        public RvcVersion VersionBefore { get; internal set; }
        public RvcVersion VersionAfter { get; internal set; }
        public Dictionary<Guid, IChange> ChangesMap
        {
            get
            {
                if(_changeDict == null) { _changeDict = new Dictionary<Guid, IChange>(); }
                return _changeDict;
            }
        }
        #endregion

        #region constructors
        public ChangeSet(): base()
        {
            _changeDict = new Dictionary<Guid, IChange>();
        }
        #endregion

        #region methods
        public void AddChange(IChange change)
        {
            if (_changeDict.ContainsKey(change.Id))
            {
                _changeDict[change.Id] = change;
            }
            else
            {
                _changeDict.Add(change.Id, change);
            }
            change.ContainingSet = this;
        }

        public static ChangeSet Merge(ChangeSet set1, ChangeSet set2)
        {
            ChangeSet merged = (ChangeSet)set1.Clone();
            foreach(IChange change in set2.ChangesMap.Values)
            {
                merged.AddChange(change);
            }
            return merged;
        }

        public override object Clone()
        {
            ChangeSet clone = new ChangeSet();
            foreach(var change in ChangesMap.Values)
            {
                clone.AddChange((IChange)change.Clone());
            }
            clone.VersionAfter = VersionAfter;
            clone.VersionBefore = VersionBefore;
            return clone;
        }

        /// <summary>
        /// Generic method to create a change and add it to the set.
        /// </summary>
        /// <param name="type">Type of change</param>
        /// <param name="objType">The type of the object affected by this change</param>
        /// <param name="guidParams">relevant guids in this order: the affected object id, if object is modified, guid if the initial version and 
        /// guid if the final version</param>
        public void AddChange(ChangeType type, Type objType, params Guid[] guidParams)
        {
            if (!typeof(ModelComponent).IsAssignableFrom(objType))
            {
                throw new InvalidOperationException("Invalid object type. It must be a subtype of ModelComponent");
            }
            if(guidParams.Length == 0) { throw new InvalidOperationException("Too few guid parameters provided"); }

            Type rawType = typeof(Change<>);
            Type genericChangeType = rawType.MakeGenericType(new Type[] { objType });

            IChange change = null;
            if(type == ChangeType.Addition)
            {
                MethodInfo generic = genericChangeType.GetMethod("CreateAddition");
                MethodInfo method = generic.MakeGenericMethod(new Type[] { objType });
                change = (IChange)method.Invoke(null, new object[] { guidParams[0] });
            }
            else if(type == ChangeType.Deletion)
            {
                MethodInfo generic = genericChangeType.GetMethod("CreateDeletion");
                MethodInfo method = generic.MakeGenericMethod(new Type[] { objType });
                change = (IChange)method.Invoke(null, new object[] { guidParams[0] });
            }
            else if(type == ChangeType.Modification)
            {
                if(guidParams.Length < 3) { throw new InvalidOperationException("Too few guids for a change type 'modification'."); }
                MethodInfo generic = genericChangeType.GetMethod("CreateModification");
                MethodInfo method = generic.MakeGenericMethod(new Type[] { objType });
                change = (IChange)method.Invoke(null, new object[] { guidParams[0], guidParams[1], guidParams[2] });
            }

            if(change != null) { AddChange(change); }
        }
        #endregion
    }
}

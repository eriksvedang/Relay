using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameTypes;
namespace RelayLib
{
    public class RelayTreeRunner
    {
        TableTwo _table;
        Dictionary<int, RelayTreeNode> _nodes = new Dictionary<int, RelayTreeNode>();
        public RelayTreeRunner(RelayTwo pRelay, string pTableName)
        {
            D.isNull(pRelay);
            if (!pRelay.tables.ContainsKey(pTableName))
                pRelay.CreateTable(pTableName);
            _table = pRelay.GetTable(pTableName);            
            List<RelayTreeNode> nodes = InstantiatorTwo.Process<RelayTreeNode>(_table);
            foreach (RelayTreeNode t in nodes)
            {
                D.assert(t.hasSetupCells, "an object of type " + t.GetType().Name + " did not call base method of SetupCells");
                _nodes.Add(t.objectId, t);
                t.SetRunner(this);
            }
            foreach (RelayTreeNode t in nodes)
            {
                t.RestoreRelations();
            }
        }
        public T CreateNode<T>() where T : RelayTreeNode
        {
            Type t = typeof(T);
            T newNode = Activator.CreateInstance(t) as T;
            newNode.SetRunner(this);
            newNode.CreateNewRelayEntry(_table, t.Name);
            _nodes.Add(newNode.objectId, newNode);
            D.assert(newNode.hasSetupCells, "an object of type " + newNode.GetType().Name + " did not call base method of SetupCells");
            return newNode;
        }
        public IEnumerable<RelayTreeNode> nodes { get { return _nodes.Values; } }
        public T GetNode<T>(int pID ) where T : RelayTreeNode
        {
            RelayTreeNode result = null;
            _nodes.TryGetValue(pID, out result);
            return (T)result;
        }
        internal void Destroy(RelayTreeNode pNode )
        {
            _table.RemoveRowAt(pNode.objectId);
            _nodes.Remove(pNode.objectId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayLib
{
    public class ConcreteTreeNode : RelayTreeNode { }
    public abstract class RelayTreeNode : RelayObjectTwo
    {
        internal bool hasSetupCells = false;
        public ValueEntry<int[]> CELL_children;
        private List<RelayTreeNode> _children = new List<RelayTreeNode>();
        protected RelayTreeRunner _relayTreeRunner = null;
        internal void SetRunner(RelayTreeRunner pRunner) {  _relayTreeRunner = pRunner;   }
        protected override void SetupCells()
        {
            CELL_children = EnsureCell<int[]>("childrenIDs", new int[] { });
            parent = null;
            hasSetupCells = true;
        }

        internal void RestoreRelations()
        {
            foreach( int c in CELL_children.data)
            {
                RelayTreeNode child = _relayTreeRunner.GetNode<RelayTreeNode>( c);
                child.parent = this;
                _children.Add(child);
            }
        }
        public RelayTreeNode parent {
            private set;
            get;
        }
        public void AddChild(RelayTreeNode pChild)
        { 
            pChild.parent = this;
            _children.Add(pChild);
            UpdateCELL_Children();
        }
        public void RemoveAndDestroyChild(RelayTreeNode pChild)
        {
            _children.Remove(pChild);
            pChild.parent = null;
            List<RelayTreeNode> c = new List<RelayTreeNode>(pChild.children);
            foreach (RelayTreeNode tn in c)
                pChild.RemoveAndDestroyChild(tn);
            _relayTreeRunner.Destroy(pChild);
            UpdateCELL_Children();
        }
        private void UpdateCELL_Children()
        {
            List<int> newList = new List<int>();
            foreach(RelayTreeNode tn in  _children)
                newList.Add(tn.objectId);
            CELL_children.data = newList.ToArray();
        }
        public IEnumerable<RelayTreeNode> children
        {
            get { return _children; }
        }
        public int childCount { get { return CELL_children.data.Length; } }
        public T[] GetChildrenRecursive<T>() where T : RelayTreeNode
        {
            List<T> result = new List<T>();
            foreach (RelayTreeNode c in _children)
            {
                if (c is T)
                    result.Add(c as T);
                T[] r = c.GetChildrenRecursive<T>();
                if (r.Length > 0)
                    result.AddRange(r);
            }
            return result.ToArray();
        }
        public int CountChildrenRecursive()
        {
            int result = 0;
            foreach (RelayTreeNode c in _children)
            {
                result += c.CountChildrenRecursive() + 1;
            }
            return result;
        }
        public IEnumerable<T> GetChildren<T>() where T : RelayTreeNode
        {
            foreach (RelayTreeNode n in _children)
                if (n is T)
                    yield return n as T;
        }
    }
}

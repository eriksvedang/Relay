using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RelayLib;
using NUnit.Framework;
namespace Relay_Tests
{
    [TestFixture]
    public class RelayTreeNodeTests
    {
        RelayTwo _relay;
        RelayTreeRunner _runner;

        [SetUp]
        public void Setup()
        {
            _relay = new RelayTwo();
            _runner = new RelayTreeRunner(_relay, "nodes");
        }

        [Test]
        public void SaveAndLoad()
        {
            ConcreteTreeNode t = _runner.CreateNode<ConcreteTreeNode>();
            t.AddChild(_runner.CreateNode<ConcreteTreeNode>());
            t.AddChild(_runner.CreateNode<ConcreteTreeNode>());

            _runner = new RelayTreeRunner(_relay, "nodes");

            t = _runner.GetNode<ConcreteTreeNode>(t.objectId);
            Assert.NotNull(t);
            Assert.AreEqual(2, t.childCount);
        }
        [Test]
        public void Recursion()
        {
            ConcreteTreeNode t = _runner.CreateNode<ConcreteTreeNode>();
            ConcreteTreeNode b = _runner.CreateNode<ConcreteTreeNode>();
            t.AddChild(b);
            b.AddChild(_runner.CreateNode<ConcreteTreeNode>());
            b.AddChild(_runner.CreateNode<ConcreteTreeNode>());

            Assert.AreEqual(3, t.GetChildrenRecursive<ConcreteTreeNode>().Count());
            Assert.AreEqual(3, t.CountChildrenRecursive());
        }
        [Test]
        public void Removal()
        {

            ConcreteTreeNode t = _runner.CreateNode<ConcreteTreeNode>();
            ConcreteTreeNode b = _runner.CreateNode<ConcreteTreeNode>();
            t.AddChild(b);
            b.AddChild(_runner.CreateNode<ConcreteTreeNode>());
            b.AddChild(_runner.CreateNode<ConcreteTreeNode>());

            t.RemoveAndDestroyChild(t.children.First());

            Assert.AreEqual(1, _relay.tables["nodes"].Count());
            
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RelayLib;
namespace Relay_Tests.tests
{
    [TestFixture]
    public class TableTwoTests
    {
        void DumpTable(TableTwo t)
        {
            Console.WriteLine("TABLE - " + t.name);
            foreach (var i in t)
            {
                Console.WriteLine(i.row.ToString() + " " + String.Join(" : ", i.valuesAsStrings));
            }
        }
        [Test]
        public void BasicUsage()
        {
            TableTwo t = new TableTwo("Basic");
            t.AddField<int>("layer");
            t.AddField<string>("name");
            t.AddField<bool>("enabled");
            t.AddField<string>("hash");

            t.CreateRow().
                Set("layer", 12).
                Set("name", "Lars").
                Set("hash", "qwert");
            t.CreateRow().
                Set("name", "Christina").
                Set("enabled", false).
                Set("hash", "qwert");
            t.CreateRow().
                Set("layer", 6).
                Set("name", "Sven").
                Set("enabled", true);
            t.CreateRow().
                Set("name", "Johannes");
            DumpTable(t);
            Assert.AreEqual(12, t.GetValue<int>(0, "layer"));
            Assert.AreEqual(6, t.GetValue<int>(2, "layer"));

            t.RemoveRowAt(1);
            Console.WriteLine("#Remove Christina");
            DumpTable(t);
            Console.WriteLine("#Create Lisa at Row 1");
            t.CreateRow().Set<string>("name", "Lisa");
            DumpTable(t);

            Console.WriteLine("#Remove Johannes and Sven");
            t.RemoveRowAt(3);
            t.RemoveLastRow();
            Assert.AreEqual(2, t.capacity);
            DumpTable(t);
            Console.WriteLine("#Remove Lars");
            t.RemoveRowAt(0);
            Assert.AreEqual(2, t.capacity);
 
            Assert.AreEqual("Lisa", t.GetValue<string>(1, "name"));            
           
        }
        public struct SomeStruct
        {
            public string header;
            public int later;
            public bool ater;
            public HairColor color;
        }
        public enum HairColor
        {
            RAT,
            BLOND,
            BROWN,
            RED,
            GRAY,
            WHITE
        }
        RelayTwo r2 = null;
        [SetUp]
        public void Setup()
        {
            r2 = new RelayTwo();

        }
        [Test]
        public void TestSimpleSerialization()
        {
            SomeStruct s = new SomeStruct();
            s.header = "dkjkgk";
            s.later = 911100299;
            s.ater = true;
            s.color = HairColor.WHITE;

            r2.CreateTable("characters");
            TableTwo c = r2.GetTable("characters");
            c.AddField<string>("name");
            c.AddField<int>("age");
            c.AddField<HairColor>("hairColor");
            c.AddField<SomeStruct>("other");

            c.CreateRow().Set("name", @"Johann3||¤%¤%&'''\'es").Set("age", 23).Set("hairColor", HairColor.RAT).Set("other", s);
            Console.WriteLine(String.Join("|", c[0].valuesAsStrings));
        }
        [Test]
        public void TestEquals()
        {
            { //table name equality
                TableTwo ta = new TableTwo("rawa2");
                TableTwo tb = new TableTwo("rawa2");
                Assert.AreEqual(true, ta.Equals(tb));
            }
            { //table name inequality
                TableTwo ta = new TableTwo("rawa");
                TableTwo tb = new TableTwo("rawa2");
                Assert.AreEqual(false, ta.Equals(tb));
            }
            { //table field type equality
                TableTwo ta = new TableTwo("rawa");
                TableTwo tb = new TableTwo("rawa");
                ta.AddField<string>("string");
                tb.AddField<string>("string");
                Assert.AreEqual(true, ta.Equals(tb));
            }
            { //table field type inequality
                TableTwo ta = new TableTwo("rawa");
                TableTwo tb = new TableTwo("rawa");
                ta.AddField<string>("string");
                tb.AddField<int>("string");
                Assert.AreEqual(false, ta.Equals(tb));
            }
            { //row equality
                TableTwo ta = new TableTwo("rawa");
                TableTwo tb = new TableTwo("rawa");
                ta.AddField<string>("string");
                ta.CreateRow().Set("string", "hello");
                tb.AddField<string>("string");
                tb.CreateRow().Set("string", "hello");
                Assert.AreEqual(true, ta.Equals(tb));
            }
            { //row inequality
                TableTwo ta = new TableTwo("rawa");
                TableTwo tb = new TableTwo("rawa");
                ta.AddField<string>("string");
                ta.CreateRow().Set("string", "hello");
                tb.AddField<string>("string");
                tb.CreateRow().Set("string", "2hello");
                Assert.AreEqual(false, ta.Equals(tb));
            }
        }

    }
}
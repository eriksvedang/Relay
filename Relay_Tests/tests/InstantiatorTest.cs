using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using RelayLib;

namespace Relay_Tests.tests
{
	[TestFixture()]
	public class InstantiatorTest
	{
		public class Stuff : RelayObjectTwo
		{
            protected override void  SetupCells()
            {
                EnsureCell("name", "no-name");
            }
			public string name {
				get {
                    return table.GetValue<string>(objectId, "name");
				}
				set {
                    table.SetValue(objectId, "name", value);
				}
			}
		}

	    [Test()]
		public void BasicUsage()
		{
            RelayTwo relay = new RelayTwo();
			TableTwo table = relay.CreateTable("Stuffs");
            table.AddField<string>("name");
            table.AddField<string>(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME);
            table.CreateRow().Set(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME, "Stuff").Set("name", "first");
            table.CreateRow().Set(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME, "Stuff").Set("name", "second");
            List<Stuff> objects = InstantiatorTwo.Process<Stuff>(table);
			
			Assert.AreEqual(2, objects.Count);
			
			Stuff first = objects[0];
			Stuff second = objects[1];
			
			Assert.AreEqual("first", first.name);
			Assert.AreEqual("second", second.name);
		}
	}
}


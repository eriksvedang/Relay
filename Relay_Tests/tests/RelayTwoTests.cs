using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RelayLib;
using GameTypes;

namespace Relay_Tests.tests
{
    [TestFixture]
    public class RelayTwoTests
    {
		internal void Log(string pMessage) {
			Console.WriteLine(pMessage);
		}		
		
		RelayTwo _relay;
		
		int _rowIndex0, _rowIndex1;
		
        [SetUp]
        public void Setup()
        {			
			D.onDLog += Log;
			
            _relay = new RelayTwo();
            TableTwo table1 = _relay.CreateTable("Table1");
			
			table1.AddField<float>("a");
			table1.AddField<float>("b");
			
			TableRow row0 = table1.CreateRow();
			TableRow row1 = table1.CreateRow();
			
			_rowIndex0 = row0.row;
			_rowIndex1 = row1.row;
			
			row0.Set("a", 3.5f);
			row0.Set("b", 7.0f);
			
			row1.Set("a", 2.0f);
			row1.Set("a", 5.0f);
        }
		
		[Test]
        public void BasicUsage()
        {
			TableTwo table1 = _relay.GetTable("Table1");
            Assert.AreEqual(3.5f, table1[_rowIndex0].Get<float>("a"));
            Assert.AreEqual(7.0f, table1[_rowIndex0].Get<float>("b"));
            Assert.AreEqual(5.0f, table1[_rowIndex1].Get<float>("a"));
        }
		
		#if DEBUG
		[Test]
		#endif
        public void AccessingWrongTable()
        {
            Assert.Throws<RelayException>(() =>
            {
                _relay.GetTable("Wrong Table Name");
            });
        }
		
		#if DEBUG
		[Test]
		#endif
        public void AccessingObjectThatDoesNotExist()
        {
            int wrongId = 150;
			TableTwo table1 = _relay.GetTable("Table1");
            Assert.Throws<RelayException>(() => 
            {                
				Console.WriteLine(table1[wrongId].row);
            });
        }
		
		#if DEBUG
		[Test]
		#endif
        public void AccessingAttributeThatDoesNotExist()
        {
			TableTwo table1 = _relay.GetTable("Table1");
			TableRow row0 = table1[_rowIndex0];
			Assert.Throws<RelayException>(() =>
            {
				row0.Get<float>("nonexisting_attribute_name");
            });
        }
		
		#if DEBUG
		[Test]
		#endif
        public void AccessingAttributeWithRightNameButWrongType()
        {
			TableTwo table1 = _relay.GetTable("Table1");
			TableRow row0 = table1[_rowIndex0];
			Assert.Throws<RelayException>(() =>
            {
				row0.Get<bool>("a"); // the "a" attribute is saved as a 'float', not a 'bool'
            });
        }
		
		[Test]
        public void GettingValueEntryReference()
        {
			TableTwo table1 = _relay.GetTable("Table1");
			Log("Row index: " + _rowIndex0);
			ValueEntry<float> value_a = table1.GetValueEntryEnsureDefault<float>(_rowIndex0, "a" , 0.0f);
            Assert.AreEqual(3.5f, value_a.data);
        }
	
		#if DEBUG
		[Test]
		#endif
        public void GettingValueEntryForObjectThatDoesNotExist()
        {
            int wrongId = 150;
			TableTwo table1 = _relay.GetTable("Table1");
            Assert.Throws<RelayException>(() => 
            {
                table1.GetValueEntryEnsureDefault<float>(wrongId, "a", 0.0f);
            });
        }
		
		#if DEBUG
		[Test]
		#endif
        public void RemovingRows()
        {
            TableTwo table1 = _relay.GetTable("Table1");
			Assert.AreEqual(2, table1.GetIndexesOfPopulatedRows().Length);
			Assert.IsNotNull(table1.GetRow(_rowIndex0));
			table1.RemoveRowAt(_rowIndex0);
			Assert.AreEqual(1, table1.GetIndexesOfPopulatedRows().Length);
			Assert.Throws<RelayException>(() => 
            { 
                table1.GetRow(_rowIndex0);
            });
			table1.RemoveRowAt(_rowIndex1);
			Assert.AreEqual(0, table1.GetIndexesOfPopulatedRows().Length);
        }
		
		#if DEBUG
		[Test]
		#endif
        public void RemovingRowThatDoesNotExist()
        {
            int wrongId = 150;
			TableTwo table1 = _relay.GetTable("Table1");
            Assert.Throws<RelayException>(() => 
            { 
                table1.RemoveRowAt(wrongId);
            });
        }
		
		// TODO Borde detta vara ett fel?
		/*
		[Test]
        public void UsingValueEntryReferenceOfDeletedRow()
        {
            TableTwo table1 = _relay.GetTable("Table1");
			ValueEntry<float> value_a = table1.GetValueEntryAndEnsureField<float>(_rowIndex0, "a");
			table1.RemoveRowAt(_rowIndex0);
            Assert.Throws<RelayException>(() => { 
            	Log(value_a.data.ToString());
            });
        }
        */
		
		[Test]
        public void ArrayBasics()
        {
            TableTwo table1 = _relay.GetTable("Table1");

            int[] array1 = new int[] { 42, 100, 3, 17 };
            
			table1.AddField<int[]>("luckyNumbers");
            table1.SetValue(_rowIndex0, "luckyNumbers", array1);
			
            int[] array2 = table1.GetValue<int[]>(_rowIndex0, "luckyNumbers");

            Assert.AreSame(array1, array2);
        }
		
		[Test]
        public void SaveAndLoad()
        {
			int rowNr0, rowNr1, rowNr2;
			
			{
				RelayTwo r1 = new RelayTwo();
				
	            r1.CreateTable("Table1");
	            TableTwo table = r1.GetTable("Table1");
				
				rowNr0 = table.CreateRow().row;
				rowNr1 = table.CreateRow().row;
				rowNr2 = table.CreateRow().row;
				
				table.GetValueEntryEnsureDefault<float>(rowNr0, "a", 3.5f);
				table.GetValueEntryEnsureDefault<float>(rowNr0, "b", 7.0f);
				table.GetValueEntryEnsureDefault<float>(rowNr1, "a", 2.0f);
                table.SetValue<float>(rowNr0, "a", 5.0f);
                table.GetValueEntryEnsureDefault<string>(rowNr2, "blehu", "ap\na");
                Assert.AreEqual(5.0f, table[rowNr0].Get<float>("a"));
                Assert.AreEqual(7.0f, table[rowNr0].Get<float>("b"));
                Assert.AreEqual(2.0f, table[rowNr1].Get<float>("a"));
                Assert.AreEqual("ap\na", table[rowNr2].Get<string>("blehu"));

				r1.SaveAll("sdfsdf.json");
			}
			{
                
	            RelayTwo r2 = new RelayTwo("sdfsdf.json");
	
	            TableTwo table = r2.GetTable("Table1");

                Assert.AreEqual(5.0f, table[rowNr0].Get<float>("a"));
                Assert.AreEqual(7.0f, table[rowNr0].Get<float>("b"));
                Assert.AreEqual(2.0f, table[rowNr1].Get<float>("a"));
                Assert.AreEqual("ap\na", table[rowNr2].Get<string>("blehu"));
			}
        }
		
		[Test()]
		public void LoadFromSeveralPartialDatabases()
        {
			{
				RelayTwo r1 = new RelayTwo();
				TableTwo t = r1.CreateTable("Table");
				
				t.AddField<string>("animal");
				t.AddField<int>("age");
				
				TableRow row0 = t.CreateRow();
				row0.Set("animal", "rabbit");
				row0.Set("age", 5);
				
				TableRow row1 = t.CreateRow();
				row1.Set("animal", "salmon");
				row1.Set("age", 10);
				
				TableRow row2 = t.CreateRow();
				row2.Set("animal", "spider");
				row2.Set("age", 1);
				
				r1.SaveAll("r1.json");
			}
			
			{
				RelayTwo r2 = new RelayTwo();
				TableTwo t = r2.CreateTable("Table");
				
				t.AddField<string>("animal");
				t.AddField<bool>("carnivore");
				
				TableRow row0 = t.CreateRow();
				row0.Set("animal", "mouse");
				row0.Set("carnivore", false);
				
				TableRow row1 = t.CreateRow();
				row1.Set("animal", "fox");
				row1.Set("carnivore", true);
				
				r2.SaveAll("r2.json");
			}
		
			{
				RelayTwo combined = new RelayTwo();
				combined.AppendTables("r1.json");
                combined.AppendTables("r2.json");
				
				Assert.AreEqual(1, combined.tables.Count);
				
				TableTwo t = combined.GetTable("Table");
				Assert.AreEqual(3, t.fieldNames.Length);
				
				Console.WriteLine("The merged table contains the following rows: ");
				foreach(int rowIndex in t.GetIndexesOfPopulatedRows())
				{
					TableRow row = t[rowIndex];
					Console.WriteLine("Values in row " + rowIndex);
					foreach(string s in row.valuesAsStrings) {
						Console.Write(s + ", ");
					}
                    Console.WriteLine("\n");
				}
				Assert.AreEqual(5, t.GetIndexesOfPopulatedRows().Length);
				
				TableRow rabbit = t[0];
				TableRow salmon = t[1];
				TableRow spider = t[2];
				TableRow mouse = t[3];
				TableRow fox = t[4];
				
				Assert.AreEqual("rabbit", rabbit.Get<string>("animal"));
				Assert.AreEqual("salmon", salmon.Get<string>("animal"));
				Assert.AreEqual("spider", spider.Get<string>("animal"));
				Assert.AreEqual("mouse", mouse.Get<string>("animal"));
				Assert.AreEqual("fox", fox.Get<string>("animal"));
				
				Assert.AreEqual(5, rabbit.Get<int>("age"));
				Assert.AreEqual(10, salmon.Get<int>("age"));
				Assert.AreEqual(1, spider.Get<int>("age"));
                Assert.Throws<RelayException>(() =>
                {
                    Assert.AreEqual(0, mouse.Get<int>("age"));
                });
                Assert.Throws<RelayException>(() =>
                {
                    Assert.AreEqual(0, fox.Get<int>("age"));
                });
				Assert.AreEqual(true, fox.Get<bool>("carnivore"));
			}
		}
		
		/*
		[Test()]
		public void LoadFromSeveralPartialDatabasesAdvanced()
        {
			{
				RelayTwo r1 = new RelayTwo();
				TableTwo t = r1.CreateTable("Table");
				
				
				
				r1.Save("r1.json");
			}
			
			{
				RelayTwo r2 = new RelayTwo();
				TableTwo t = r2.CreateTable("Table");
				
								
				
				r2.Save("r2.json");
			}
		
			{
				RelayTwo combined = new RelayTwo();
				combined.AppendTable("r1.json");
                combined.AppendTable("r2.json");
				
				
			}
		}
		*/
		        
		[Test()]
        public void MergeTwoSubsets()
        {
            RelayTwo relay = new RelayTwo();

            TableTwo table = relay.CreateTable("Table1");
            table.AddField<string>("name");
            table.AddField<bool>("1337");

            TableRow row1 = table.CreateRow();
            TableRow row2 = table.CreateRow();
            TableRow row3 = table.CreateRow();

            row1.Set("name", "Arne");
            row1.Set("1337", false);

            row2.Set("name", "Björn");
            row2.Set("1337", true);

            row3.Set("name", "Charlie");
            row3.Set("1337", false);

            Assert.AreEqual(3, table.GetRows().Length);

            RelayTwo subsetA = relay.Subset("Table1", (o => o.Get<bool>("1337") == true));
            RelayTwo subsetB = relay.Subset("Table1", (o => o.Get<bool>("1337") == false));
            subsetA.MergeWith(subsetB);
            Assert.Throws<RelayTwo.RelayMergeException>(() => subsetB.MergeWith(relay));
            Assert.AreEqual(true, subsetA.Equals(relay));
            Assert.AreEqual(false, subsetB.Equals(relay));
        }
		
	    [Test()]
        public void MergeSaves()
        {
			{
	            RelayTwo relay = new RelayTwo();
				
				TableTwo table = relay.CreateTable("Table");
	
	            table.AddField<string>("name");
	
	            TableRow row1 = table.CreateRow();
	            TableRow row2 = table.CreateRow();
	            TableRow row3 = table.CreateRow();
	
	            row1.Set("name", "a");
	            row2.Set("name", "b");
				row3.Set("name", "c");
	
	            Assert.AreEqual(3, table.GetRows().Length);
				Assert.AreEqual("a", table.GetRow(0).Get<string>("name"));
				Assert.AreEqual("b", table.GetRow(1).Get<string>("name"));
				Assert.AreEqual("c", table.GetRow(2).Get<string>("name"));
	
	            RelayTwo save1 = relay.Subset("Table", (o => o.Get<string>("name") == "a"));
	            RelayTwo save2 = relay.Subset("Table", (o => o.Get<string>("name") == "b"));
				RelayTwo save3 = relay.Subset("Table", (o => o.Get<string>("name") == "c"));
				
				Assert.AreEqual(1, save1.GetTable("Table").GetRows().Length);
				Assert.AreEqual(1, save2.GetTable("Table").GetRows().Length);
				Assert.AreEqual(1, save3.GetTable("Table").GetRows().Length);
				
				save1.SaveAll("PartialSave1.json");
				save2.SaveAll("PartialSave2.json");
				save3.SaveAll("PartialSave3.json");
			}
			{
				RelayTwo relay = new RelayTwo();
				relay.MergeWith(new RelayTwo("PartialSave1.json"));
				relay.MergeWith(new RelayTwo("PartialSave2.json"));
				relay.MergeWith(new RelayTwo("PartialSave3.json"));
				
				TableTwo table = relay.GetTable("Table");
				Assert.AreEqual(3, table.GetRows().Length);
				
				Assert.AreEqual("a", table.GetRow(0).Get<string>("name"));
				Assert.AreEqual("b", table.GetRow(1).Get<string>("name"));
				Assert.AreEqual("c", table.GetRow(2).Get<string>("name"));
			}
        }
		
		 [Test()]
        public void MergeSavesWithDifferentFields()
        {
			{
	            RelayTwo relay = new RelayTwo();
				
				TableTwo table = relay.CreateTable("Table");
	
	            table.AddField<string>("oldField");
	
	            TableRow row1 = table.CreateRow();
	            TableRow row2 = table.CreateRow();
				TableRow row3 = table.CreateRow();
	
	            row1.Set("oldField", "kottar");
	            row2.Set("oldField", "stenar");
				row3.Set("oldField", "gräs");
				
				relay.Subset("Table", (o => o.row == 0)).SaveAll("s0.json");
				relay.Subset("Table", (o => o.row == 1)).SaveAll("s1.json");
				relay.Subset("Table", (o => o.row == 2)).SaveAll("s2.json");
	
				table.AddField<int>("newField");
				
	            TableRow row4 = table.CreateRow();
				TableRow row5 = table.CreateRow();		
				
				row4.Set("newField", 500);
				row5.Set("newField", 1000);
				
				relay.Subset("Table", (o => o.row == 3)).SaveAll("s3.json");
				relay.Subset("Table", (o => o.row == 4)).SaveAll("s4.json");
			}
			{
				RelayTwo relay = new RelayTwo();
				
				relay.MergeWith(new RelayTwo("s4.json"));
				relay.MergeWith(new RelayTwo("s0.json"));
				relay.MergeWith(new RelayTwo("s1.json"));
				relay.MergeWith(new RelayTwo("s2.json"));
				relay.MergeWith(new RelayTwo("s3.json"));
				
				
				TableTwo table = relay.GetTable("Table");
				Assert.AreEqual(5, table.GetRows().Length);
				
				Assert.AreEqual("kottar", table.GetRow(0).Get<string>("oldField"));
				Assert.AreEqual("stenar", table.GetRow(1).Get<string>("oldField"));
				Assert.AreEqual("gräs", table.GetRow(2).Get<string>("oldField"));
				
				Assert.AreEqual(500, table.GetRow(3).Get<int>("newField"));
				Assert.AreEqual(1000, table.GetRow(4).Get<int>("newField"));
			}
        }
		
		[Test()]
		public void SaveSubsetOfDatabase()
        {
			{
				RelayTwo relay = new RelayTwo();
				
				TableTwo table = relay.CreateTable("Table1");
				table.AddField<string>("name");
				table.AddField<bool>("1337");
				
				TableRow row1 = table.CreateRow();
				TableRow row2 = table.CreateRow();
				TableRow row3 = table.CreateRow();
				
				row1.Set("name", "Arne");
				row1.Set("1337", false);
				
				row2.Set("name", "Björn");
				row2.Set("1337", true);
				
				row3.Set("name", "Charlie");
				row3.Set("1337", false);
				
				Assert.AreEqual(3, table.GetRows().Length);
				
				RelayTwo subset = relay.Subset("Table1", (o => o.Get<bool>("1337") == true));
				
				subset.SaveAll("Only1337People.json");
			}
		
			{
				RelayTwo relay = new RelayTwo("Only1337People.json");
				TableTwo table = relay.GetTable("Table1");
				TableRow[] rows = table.GetRows();
				Assert.AreEqual(1, rows.Length);
				Assert.AreEqual("Björn", table.First().Get<string>("name"));
			}
		}
		
		[Test()]
        public void GetListOfRowIdsInTable()
        {
			RelayTwo relay = new RelayTwo();
			TableTwo table = relay.CreateTable("Table");
			
			table.CreateRow();
			table.CreateRow();
			table.CreateRow();
			
			int[] allObjectIds = table.GetIndexesOfPopulatedRows();

			Assert.AreEqual(3, allObjectIds.Length);
			CollectionAssert.AllItemsAreUnique(allObjectIds);
        }
		
    }
}

//#define PRINT_TIME_DATA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameTypes;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace RelayLib
{
    public class RelayTwo
    {
        public SerializableDictionary<string, TableTwo> tables = new SerializableDictionary<string, TableTwo>();
     
        public RelayTwo()
        { 

        }

        public RelayTwo(string pFilename)
        {
            LoadAll(pFilename);
        }

        public TableTwo CreateTable(string pTableName)
        {
#if DEBUG
            if (tables.ContainsKey(pTableName))
            {
                throw new RelayException("Database already contains a table with name " + pTableName);
            }
#endif
            TableTwo newTable = new TableTwo(pTableName);
            tables.Add(pTableName, newTable);
            return newTable;
        }
     
        public TableTwo GetTable(string pTableName)
        {
#if DEBUG
            if (!tables.ContainsKey(pTableName))
            {
                throw new RelayException("Can't find table with name " + pTableName);
            }
#endif
            return tables[pTableName];            
        }
     
        IEnumerable<float> Save(string pFilename)
        {
            FileStream f = new FileStream(pFilename, FileMode.Create);
            StreamWriter tw = new StreamWriter(f);
            int tableCount = tables.Count;
            int tableIndex = 0;

            foreach (TableTwo t in tables.Values) {
                tw.WriteLine(t.name);
                tw.WriteLine(t.Count());
                tw.WriteLine(JsonConvert.SerializeObject(t.fieldNames, Formatting.None));
                tw.WriteLine(JsonConvert.SerializeObject(t.fieldDataTypeNames, Formatting.None));
                foreach (TableRow r in t) {
                    tw.WriteLine(JsonConvert.SerializeObject(r.GetSerializableTableRow(), Formatting.None));
                }
                yield return (float)tableIndex / (float)tableCount;
            }

            tw.Flush();
            tw.Close();
            tw.Dispose();
            f.Dispose();
        }
     
        public void SaveAll(string pFilename)
        {
            foreach (float f in Save(pFilename))
                ;
        }
     
        public void SaveTableSubsetSeparately(string pTableName, string pSaveFilePath)
        {
            RelayTwo subset = Subset(pTableName, (o => true));
            subset.SaveAll(pSaveFilePath);
        }

        public void LoadAll(string pFilename)
        {
            foreach (float f in Load(pFilename))
                ;
        }

        public IEnumerable<float> Load(string pFilename)
        {
#if PRINT_TIME_DATA
            DateTime startTime = DateTime.Now;
#endif

            FileStream f = new FileStream(pFilename, FileMode.Open);
            StreamReader sw = new StreamReader(f);

            while (!sw.EndOfStream) {
                string tableName = sw.ReadLine();
                int tableCount = Convert.ToInt32(sw.ReadLine());

#if PRINT_TIME_DATA
                //DateTime deserializeStartTime = DateTime.Now;
#endif

                string[] fieldNames = JsonConvert.DeserializeObject<string[]>(sw.ReadLine());
                string[] typeNames = JsonConvert.DeserializeObject<string[]>(sw.ReadLine());

#if PRINT_TIME_DATA
                //TimeSpan deserializeSpan = DateTime.Now - deserializeStartTime;
                //Console.WriteLine("Deserializing line took " + deserializeSpan.TotalSeconds + " seconds");

                //DateTime restStartTime = DateTime.Now;
#endif

                #if PRINT_TIME_DATA
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                #endif

                TableTwo newTable = new TableTwo(tableName);
                AddFieldsToTable(newTable, fieldNames, typeNames);

                #if PRINT_TIME_DATA
                stopwatch.Stop();
                Console.WriteLine("AddFieldsToTable: " + stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
                stopwatch.Start();
                #endif

                for (int i = 0; i < tableCount; i++) {

                    #if PRINT_TIME_DATA
                    stopwatch.Reset();
                    stopwatch.Start();
                    #endif

                    SerializableTableRow r = JsonConvert.DeserializeObject<SerializableTableRow>(sw.ReadLine());

                    #if PRINT_TIME_DATA
                    stopwatch.Stop();
                    Console.WriteLine("DeserializeObject: " + stopwatch.ElapsedMilliseconds);

                    stopwatch.Reset();
                    stopwatch.Start();
                    #endif

                    if (r.row >= newTable.capacity) {
                        newTable.SetCapacity(r.row + 1);
                    }

                    #if PRINT_TIME_DATA
                    stopwatch.Stop();
                    Console.WriteLine("SetCapacity: " + stopwatch.ElapsedMilliseconds);
                    
                    stopwatch.Reset();
                    stopwatch.Start();
                    #endif

                    r.InsertToTable(newTable);

                    #if PRINT_TIME_DATA
                    stopwatch.Stop();
                    Console.WriteLine("InsertToTable: " + stopwatch.ElapsedMilliseconds);
                    #endif
                }

                tables.Add(newTable.name, newTable);

#if PRINT_TIME_DATA
                //TimeSpan restSpan = DateTime.Now - restStartTime;
                //Console.WriteLine("Rest took " + restSpan.TotalSeconds + " seconds");
#endif

                yield return ((float)sw.BaseStream.Position) / ((float)sw.BaseStream.Length);
            }

            f.Flush();
            f.Close();
            f.Dispose();

#if PRINT_TIME_DATA
            TimeSpan span = DateTime.Now - startTime;
            Console.WriteLine ("Loading " + pFilename + " took " + span.TotalSeconds + " seconds");
#endif
        }

        private void AddFieldsToTable(TableTwo pTable, string[] pFieldNames, string[] pDataTypeNames)
        {
            D.assert(pFieldNames.Length == pDataTypeNames.Length, "field definitions does not match");

            for (int i = 0; i < pFieldNames.Length; i++) {
                Type genericType = typeof(TableField<>);
                string dataTypeName = pDataTypeNames[i];
                //Console.WriteLine(dataTypeName);
                Type[] typeArgs = null;

                // Check through all assemblies for the type arguemnts
                foreach (Assembly assm in AppDomain.CurrentDomain.GetAssemblies()) {
                    typeArgs = new Type[] { assm.GetType(dataTypeName) };
                    if (typeArgs[0] != null)
                        break; //found type
                }
                
                D.isNull(typeArgs[0], dataTypeName + " was not found ");
                Type repositoryType = genericType.MakeGenericType(typeArgs);

                ITableField result = (ITableField)Activator.CreateInstance(repositoryType, pFieldNames[i]);
                pTable.AddField(result);
            }
        }

        /// <summary>
        /// Append all entries in all tables, source IDs will be discarded.
        /// </summary>
        public void AppendTables(RelayTwo pRelay)
        {
            RelayTwo additiveRelay = pRelay;
            // Cycle through all new tables in the loaded relay
            foreach (String tableName in additiveRelay.tables.Keys) {
                TableTwo fromTable = additiveRelay.tables[tableName];
                // If the table already exists, append the entries from the new table.
                if (tables.ContainsKey(tableName)) {
                    TableTwo toTable = tables[tableName];
                    // Ensure that all fields exists in the old table.
                    foreach (ITableField f in fromTable.fields) {
                        if (!toTable.fieldNames.Contains(f.name))
                            toTable.AddField(f.GetEmptyCopy());
                    }
                    foreach (TableRow fromRow in fromTable) {
                        TableRow toRow = toTable.CreateRow();
                        foreach (ITableField fromField in fromTable.fields) {
                            ITableField toField = toTable.GetField(fromField.name);
                            toField.SetValueFromString(toRow.row, fromField.GetValueAsString(fromRow.row));
                        }
                    }
                }
                else {
                    this.tables[fromTable.name] = fromTable;
                }
            }
        }
     
        /// <summary>
        /// Append all entries in all tables, source IDs will be discarded.
        /// </summary>
        public void AppendTables(string pFilename)
        {
            //Console.WriteLine("Appending table from file " + pFilename);
            RelayTwo additiveRelay = new RelayTwo(pFilename);
            AppendTables(additiveRelay);
        }

        public RelayTwo Subset(string pTableName, Func<TableRow, bool> pPredicate)
        {
            RelayTwo relaySubset = new RelayTwo();
            TableTwo tableSubset = relaySubset.CreateTable(pTableName);
            TableTwo sourceTable = tables[pTableName];

            foreach (ITableField f in sourceTable.fields) {
                tableSubset.AddField(f.GetEmptyCopy());
            }

            tableSubset.SetCapacity(sourceTable.capacity);

            foreach (TableRow sourceRow in sourceTable.Where(pPredicate)) {
                SerializableTableRow newRow = sourceRow.GetSerializableTableRow();
                newRow.InsertToTable(tableSubset);
                //Console.WriteLine("added row " + newRow.row);
            }

            return relaySubset;
        }

        public class RelayMergeException : Exception
        {
            public RelayMergeException(string pMessage) : base(pMessage)
            {
            }
        }
   
        public void MergeWith(RelayTwo pSource)
        {
            foreach (TableTwo targetTable in tables.Values) {
                if (pSource.tables.ContainsKey(targetTable.name)) {      
                    TableTwo sourceTable = pSource.tables[targetTable.name];
                 
                    // Add missing fields to targetTable
                    foreach (ITableField field in sourceTable.fields) {
                        if (!targetTable.fieldNames.Contains(field.name)) {
                            targetTable.AddField(field.GetEmptyCopy());
                        }
                    }
                 
                    // Add missing fields to sourceTable
                    foreach (ITableField field in targetTable.fields) {
                        if (!sourceTable.fieldNames.Contains(field.name)) {
                            sourceTable.AddField(field.GetEmptyCopy());
                        }
                    }
                 
                    if (sourceTable.capacity > targetTable.capacity)
                        targetTable.SetCapacity(sourceTable.capacity);
                    
                    foreach (TableRow r in sourceTable) {
                        SerializableTableRow sr = r.GetSerializableTableRow();
                        
                        if (targetTable.ContainsRow(sr.row))
                            throw new RelayMergeException("table " + targetTable.name + " does already contain row " + sr.row);
                        
                        sr.InsertToTable(targetTable);
                    }
                }
            }
         
            // Copy complete tables from subsetB if they don't exist in this relay
            foreach (TableTwo tableB in pSource.tables.Values) {
                if (!tables.ContainsKey(tableB.name)) {
                    TableTwo newTable = CreateTable(tableB.name);
                 
                    // Add fields to the new table
                    foreach (ITableField field in tableB.fields) {
                        newTable.AddField(field.GetEmptyCopy());
                    }
                 
                    newTable.SetCapacity(tableB.capacity);

                    foreach (TableRow r in tableB) {
                        SerializableTableRow sr = r.GetSerializableTableRow();
                        sr.InsertToTable(newTable);
                    }
                }
            }
        }
     
        public override bool Equals(object obj)
        {
            RelayTwo r = obj as RelayTwo;
            if (r == null || r.tables.Count != tables.Count)
                return false;
            foreach (TableTwo t in tables.Values) {
                if (!t.Equals(r.tables[t.name]))
                    return false;
            }
            return true;
        }
     
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string ValueToString(object pValue)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(pValue);
        }

        public static T StringToValue<T>(string pString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(pString);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameTypes;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Reflection;
namespace RelayLib
{
    public class SerializableTableRow
    {
        public int row;
        public string[] values;
        public void InsertToTable(TableTwo pTable)
        {
            pTable._usedRows[row] = true;
            TableRow tr = pTable.GetRow(row);
            tr.valuesAsStrings = values;
        }

    }
	
    public struct TableRow
    {
        private TableTwo _table;
        private int _row;
		
        internal TableRow( TableTwo pTable, int pRow )
        {
            _table = pTable;
            _row = pRow;
        }
		
        public int row { get { return _row; } set { _row = value; } }
		
        public T Get<T>(string pFieldName)
        {
            return _table.GetValue<T>(row, pFieldName);
        }
		
        public TableRow Set<T>(string pFieldName, T pValue)
        {
            _table.SetValue<T>(_row, pFieldName, pValue);
            return this;
        }
		
        public string[] valuesAsStrings
        {
            get 
            {
                TableTwo tab = _table;
                int r = _row;
                var i = from ITableField f in _table.fields
                        select tab.GetStringValue(r, f.name);
                return i.ToArray();
            }
            set 
            {
                int i = 0;
                foreach (ITableField f in _table.fields)
                {
                    f.SetValueFromString(_row, value[i++]);
                }
            }
        }
		
        public SerializableTableRow GetSerializableTableRow()
        { 
            SerializableTableRow sr = new SerializableTableRow();
            sr.values = valuesAsStrings;
            sr.row = _row;
            return sr;
        }
    }
	
    public class TableTwo : IEnumerable<TableRow>
    {
        public const string NULL_TOKEN = "NULL_TOKEN";
        public string name { get; private set; }
        private int _capacity = 0;
        public List<bool> _usedRows = new List<bool>();
        private Dictionary<string, ITableField> _fields = new Dictionary<string, ITableField>();
		
        public TableTwo(string pName)
        {
            name = pName;
        }
        public void EnsureField<T>(string pFieldName)
        {
            if (!_fields.ContainsKey(pFieldName))
            {
                AddField<T>(pFieldName);
            }
        }
        public ValueEntry<T> GetValueEntryEnsureDefault<T>(int pRow, string pFieldName, T pDefaultValue)
        {
            EnsureField<T>(pFieldName);
#if DEBUG
			if(!ContainsRow(pRow)) {
                throw new RelayException("The row " + pRow + " does not exist in table " + name);
			}
#endif
            TableField<T> field = _fields[pFieldName] as TableField<T>;
#if DEBUG
            if (field == null)
            {
                throw new RelayException("Can't access field '" + pFieldName + "' using the type '" + typeof(T).Name + "', use '" + _fields[pFieldName].type.Name + "' instead");
            }
#endif 
            if (field.entries[pRow] == null)
            {
                field.entries[pRow] = new ValueEntry<T>();
                field.entries[pRow].data = pDefaultValue;
            }
            return (_fields[pFieldName] as TableField<T>).entries[pRow];
        }
        




        public void AddField<T>(string pFieldName) 
        { 
#if DEBUG
            D.assert( !_fields.ContainsKey(pFieldName), "field does already exist!");
#endif
            TableField<T> f = new TableField<T>(pFieldName);
            AddField(f);
        }
		
        internal void AddField(ITableField pField)
        {
            pField.rowCount = capacity;
            _fields.Add(pField.name, pField);
#if DEBUG
            EnsureFieldEquality();
#endif
        }

        /// <typeparam name="T"></typeparam>
        /// <param name="pRow"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pValue"></param>
        public void SetValue<T>(int pRow, string pFieldName, T pValue)
        {

#if DEBUG
            if (!_fields.ContainsKey(pFieldName))
            {
                throw new RelayException("The field '" + pFieldName + "' does not exist");
            }
#endif
            TableField<T> field = _fields[pFieldName] as TableField<T>;
#if DEBUG
            if (field == null)
            {
                throw new RelayException("Can't access field '" + pFieldName + "' using the type '" + typeof(T).Name + "', use '" + _fields[pFieldName].type.Name + "' instead");
            }
#endif      
            if (field.entries[pRow] == null)
            {
                field.entries[pRow] = new ValueEntry<T>();
            }
            field.entries[pRow].data = pValue;
        }
        public T GetValue<T>(int pRow, string pFieldName)
        {
            return GetValueEntry<T>(pRow, pFieldName).data;
        }
        public ValueEntry<T> GetValueEntry<T>(int pRow, string pFieldName)
        {
#if DEBUG
            if (!ContainsRow(pRow))
            {
                throw new RelayException("The row " + pRow + " does not exist in table " + name);
            }
            if (!_fields.ContainsKey(pFieldName))
            {
                throw new RelayException("The field " + pFieldName + " does not exist in table " + name);
            }
            TableField<T> field = _fields[pFieldName] as TableField<T>;
            if (field == null)
            {
                throw new RelayException("Can't access field '" + pFieldName + "' using the type '" + typeof(T).Name + "', use '" + _fields[pFieldName].type.Name + "' instead");
            }
            if (field.entries[pRow] == null)
            {
                throw new RelayException("Can't get value since cell is null: row " + pRow + " field " + pFieldName);
            }
#endif
            return (_fields[pFieldName] as TableField<T>).entries[pRow];
        }
        public string GetStringValue(int pRow, string pFieldName)
        {
#if DEBUG
            if(!_fields.ContainsKey(pFieldName))
            {
                throw new RelayException("The field '" + pFieldName + "' does not exist in table " + name);
			}
#endif
            return _fields[pFieldName].GetValueAsString(pRow);
        }


		
        /// <summary>
        /// Adds a new Row, increases the capacity in all fields.
        /// Also tries to recycle old rows.
        /// </summary>
        /// <returns>Returns newly added row index</returns>
        public TableRow CreateRow()
        {
            TableRow result;
            int freeIndex = GetOneFreeIndex();
            if (freeIndex == -1 )
            {
                freeIndex = _capacity;
                SetCapacity(_capacity + 1);
            }
            _usedRows[freeIndex] = true;
            result = new TableRow(this, freeIndex);
#if DEBUG
            EnsureFieldEquality();
#endif
            return result;
        }
		
        private int GetOneFreeIndex()
        {
            for (int i = 0; i < capacity; i++)
                if (_usedRows[i] == false)
                    return i;
            return -1;
        }
		
        public void RemoveRowAt(int pRow)
        {
#if DEBUG
            bool rowExists = ContainsRow(pRow);
			if(!rowExists) {
				throw new RelayException("Can't remove row " + pRow + " because it doesn't exist in table " + name);
			}
#endif
            if(pRow == _capacity - 1)
            {
                RemoveLastRow();
            }
            else
            {
                _usedRows[pRow] = false;
                foreach(ITableField t in _fields.Values )
                    t.ClearEntryAtRow(pRow);
            }
#if DEBUG
            EnsureFieldEquality();
#endif
        }
		
        /// <summary>
        /// Removes last row, also tries to clear up some free indexes
        /// </summary>
        public void RemoveLastRow()
        {
            SetCapacity(_capacity - 1);
            while(_capacity > 0 && _usedRows[capacity - 1] == false )
            {
                RemoveLastRow();
            }
#if DEBUG
            EnsureFieldEquality();
#endif
        }

        private void EnsureFieldEquality()
        {
            if(_fields.Values.Count == 0)
                return;
            int[] rowCounts = (from f in _fields.Values select f.rowCount).ToArray();
            foreach (int i in rowCounts)
                D.assert(i == rowCounts[0], "All fields must have the same row count");
        }

        /// <summary>
        /// Returns all the rows indexes which are in use
        /// </summary>
        public int[] GetIndexesOfPopulatedRows()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < _capacity; i++)
            {
                if (_usedRows[i] == true)
                    result.Add(i);
            }
            return result.ToArray();
        }
		
        public int UsedRowCount()
        { 
            int result= 0;
            for (int i = 0; i < _capacity; i++)
                if (_usedRows[i])
                    result++;
            return result;
        }
        
		public TableRow[] GetRows()
        {
            int[] rows = GetIndexesOfPopulatedRows();
			TableRow[] result = new TableRow[ rows.Length ];
            int i = 0;
            foreach(int rowIndex in rows) {
				result[i++] = GetRow(rowIndex);
			}
            return result;
		}

        public int capacity
        { get { return _capacity; } }
       
		#region IEnumerable<TableRow> Members


        
        public string[] fieldNames
        {
            get
            {
                if (fields.Count() == 0)
                    return new string[] { };
                var result = (from ITableField f in fields select f.name);
                return result.ToArray();
                
            }
        }
        public string[] fieldDataTypeNames
        {
            get
            {
                if (fields.Count() == 0)
                    return new string[] { };
                var result = (from ITableField f in fields select f.type.FullName);
                return result.ToArray(); 
            }
        }
        public IEnumerable<ITableField> fields
        {
            get { return _fields.Values; }
        }

        public IEnumerator<TableRow> GetEnumerator()
        {
            int[] rows = GetIndexesOfPopulatedRows();
            foreach (int i in rows)
                yield return new TableRow(this, i);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            int[] rows = GetIndexesOfPopulatedRows();
            foreach (int i in rows)
                yield return new TableRow(this, i);
        }

        #endregion
		
		// Square bracket operator
		public TableRow this[int pRow]
		{
			get { 
				#if DEBUG
            	if(!ContainsRow(pRow)) {
                    throw new RelayException("Row " + pRow + " does not exist in table " + name);
				}
				#endif
				return new TableRow(this, pRow); 
			}
		}
        
		public ITableField GetField(string pFieldName)
        {
            return _fields[pFieldName];
        }
		
		public TableRow GetRow(int pRow)
		{
			return this[pRow];
		}
        
		public int Count()
        {
            return GetIndexesOfPopulatedRows().Length;
        }
		
        public void SetCapacity(int pNewCapacity)
        {
            foreach (ITableField t in _fields.Values)
                t.rowCount = pNewCapacity;

            _capacity = pNewCapacity;

            while (_usedRows.Count < _capacity)
            {
                _usedRows.Add(false);
            }
            while (_usedRows.Count > _capacity)
            {
                _usedRows.RemoveAt(_usedRows.Count - 1);
            }
        }
		
        public override bool Equals (object obj)
        {
            TableTwo p = obj as TableTwo;
            if (p == null ||
                p.name != name ||
                p.capacity != this.capacity || 
                !p.fieldNames.SequenceEqual(this.fieldNames) ||
                !p.fieldDataTypeNames.SequenceEqual(this.fieldDataTypeNames))
                return false;
            for (int i = 0; i < capacity; i++)
            {
                bool containsRow = ContainsRow(i);
                if (containsRow != p.ContainsRow(i))
                    return false;
                if (containsRow && !this[i].valuesAsStrings.SequenceEqual(p[i].valuesAsStrings))
                    return false;
            }

            return true;
        }
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

        internal bool ContainsRow(int pRowIndex)
        {
            return pRowIndex > -1 && pRowIndex < _capacity && _usedRows[pRowIndex];
        }
            

    }
}

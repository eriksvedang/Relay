using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace RelayLib
{
    public interface ITableField
    {
        string name { get; }
        Type type { get; }
        int rowCount { get; set; }
        void ClearEntryAtRow(int pIndex);

        string GetValueAsString(int pRow);
        void SetValueFromString(int pRow, string pValue);
        
        ITableField GetEmptyCopy();
    }

    public class TableField<T> : ITableField
    {
        public List<ValueEntry<T>> entries = new List<ValueEntry<T>>();
        public string name { get; private set; }
        public Type type { get; private set; }

        public ITableField GetEmptyCopy()
        {
            return (ITableField)new TableField<T>(name);
        }

        public TableField(string pName)
        {
            type = typeof(T);
            name = pName;   
        }
       
		public void ClearEntryAtRow(int pRow)
        {
            //Console.WriteLine("Clearing entry at row" + pRow);
            entries[pRow] = null;
        }
      
        static JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings());

		public string GetValueAsString(int pRow)
        {
            if (entries[pRow] != null)
            {
                StringWriter sb = new StringWriter();
                JsonTextWriter tw = new JsonTextWriter(sb);
                tw.Formatting = Formatting.None;
                _serializer.Serialize(tw, entries[pRow].data);
                tw.Close();
                return sb.ToString();
            }
            else
            {
                return TableTwo.NULL_TOKEN;
            }
        }

        static JsonSerializerSettings settings = new JsonSerializerSettings();
        static JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

        public void SetValueFromString(int pRow, string pValue)
        {
            if (pValue == TableTwo.NULL_TOKEN)
            {
                entries[pRow] = null;
            }
            else
            {
                if (entries[pRow] == null) {
                    entries[pRow] = new ValueEntry<T>();
                }

                JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(pValue));
                entries[pRow].data = jsonSerializer.Deserialize<T>(jsonTextReader);
                jsonTextReader.Close();
            }
        }
		
        public int rowCount
        {
            get { return entries.Count; }
            set
            {
                if (value == 0)
                {
                    entries.Clear();  
                }
                //TODO: make faster range remove and add
                while (value < entries.Count) //remove one, slow and easy
                    entries.RemoveAt(entries.Count - 1);
                while (value > entries.Count) //fill empty slots
                    entries.Add(null);
            }
        }
    }
}


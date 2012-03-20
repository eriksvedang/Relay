using System;
using RelayLib;
using GameTypes;

namespace RelayLib
{
	public abstract class RelayObjectTwo
	{
		public static string CSHARP_CLASS_FIELD_NAME = "csharp-class";
		public TableTwo table{get;private set;}
        public void LoadFromExistingRelayEntry(TableTwo pTable, int pObjectID, string pClassName)
        {
            table = pTable;
            objectId = pObjectID;
#if DEBUG
            D.assert(table.ContainsRow(objectId), "object id does not exist! " + objectId);
            D.assert(table.GetValue<string>(pObjectID, CSHARP_CLASS_FIELD_NAME) == pClassName, "classname missmatch!");
#endif
            SetupCells();
        }

		public void CreateNewRelayEntry(TableTwo pTable, string pClassName)
		{
			table = pTable;
            objectId = table.CreateRow().row;
            table.EnsureField<string>(CSHARP_CLASS_FIELD_NAME);
            table.SetValue<string>(objectId, CSHARP_CLASS_FIELD_NAME, pClassName);
			SetupCells();
		}

        protected abstract void SetupCells();

        protected ValueEntry<T> EnsureCell<T>(string pAttributeName, T pDefaultValue)
        {
            table.EnsureField<T>(pAttributeName);
            ValueEntry<T> v = table.GetValueEntryEnsureDefault<T>(objectId, pAttributeName, pDefaultValue);
            return v;
        }

        public void AddDataListener<T>(string pFieldName, ValueEntry<T>.DataChangeHandler pHandler)
        {
            table.GetValueEntry<T>(objectId, pFieldName).onDataChanged += pHandler;
        }
        
		public void RemoveDataListener<T>(string pFieldName, ValueEntry<T>.DataChangeHandler pHandler)
        {
            table.GetValueEntry<T>(objectId, pFieldName).onDataChanged -= pHandler;
        }

        public int objectId
        {
            get;
            protected set;
        }
	}
}


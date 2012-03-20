using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
namespace RelayLib
{
    public class ValueEntry<T>
    {
        public ValueEntry()
        {
        }
        public delegate void DataChangeHandler(T pOldValue, T pNewValue);

        internal ValueEntry(T pValue, DataChangeHandler pOnDataChanged)
        {
            _value = pValue;
            onDataChanged = pOnDataChanged;
        }

        private T _value = default(T);
        public T data
        {
            set
            {
                if (onDataChanged == null)
                {
                    _value = value;
                }
                else 
                {
                    if (_value == null && value == null)
                        return;
                    if (
                        (_value != null && !_value.Equals(value)) ||
                        (value != null)
                        )
                    {
                        T oldValue = _value;
                        _value = value;
                        onDataChanged(oldValue, _value);
                    }
                }
            }
            get
            {
                return _value;
            }
        }
        public event DataChangeHandler onDataChanged;
    }
}

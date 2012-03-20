using System;
using System.Collections.Generic;
using System.Text;

namespace RelayLib
{
    public class RelayException : Exception
    {
        public RelayException(string pDescription)
            : base(pDescription)
        {
        }
    }
	
	public class CantFindClassWithNameException : Exception
    {
        public CantFindClassWithNameException(string pDescription)
            : base(pDescription)
        {
        }
    }
}

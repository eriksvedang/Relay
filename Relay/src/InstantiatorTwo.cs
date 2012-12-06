using System;
using System.Reflection;
using System.Collections.Generic;
using GameTypes;

namespace RelayLib
{	
	public class InstantiatorTwo
	{
        public static Type[] GetSubclasses(Type baseType)
        {
            List<Type> subTypes = new List<Type>();
            List<Type> temp = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                try {
                    if(assembly.GetName().Name.Contains("NUnit")) {
                        continue;
                    }
                    temp.AddRange(assembly.GetTypes());
                }
                catch(ReflectionTypeLoadException) {
                    Console.WriteLine("There was an error loading the assembly " + assembly.FullName + " ");
                    /*foreach(var loaderException in e.LoaderExceptions) {
                        Console.WriteLine(loaderException.Message);
                    }*/
                }
            }

            Type[] allTypes = temp.ToArray();

            for (int i = 0; i < allTypes.Length; i++)
            {
                Type t = allTypes[i];
                if (t.IsSubclassOf(baseType))
                {
                    subTypes.Add(t);
                }
            }

            subTypes.Add(baseType);
            return subTypes.ToArray();
        }

        public static Type GetType(string pName, Type[] pTypeCollection)
        {
            foreach (Type type in pTypeCollection) {
                if (type.Name == pName) {
                   return type;
                }
            }
            return null;
        }

        public static List<T> Process<T>(TableTwo pTable, Type[] pSubTypes) where T : RelayObjectTwo
        {
            Type[] subTypes = pSubTypes;
            List<T> newInstances = new List<T>();

            foreach (TableRow tr in pTable)
            {
                string className = tr.Get<string>(RelayObjectTwo.CSHARP_CLASS_FIELD_NAME);
                Type type = GetType(className, subTypes);
                if (type == null) {
                    throw new CantFindClassWithNameException("Can't find class with name " + className + " to instantiate");
                }
                T newInstance = Activator.CreateInstance(type) as T;
                D.isNull(newInstance);
                newInstance.LoadFromExistingRelayEntry(pTable, tr.row, className);
                newInstances.Add(newInstance);
            }

            return newInstances;
        }

        public static List<T> Process<T>(TableTwo pTable, Type pType) where T : RelayObjectTwo
        {
            Type[] subTypes = GetSubclasses(pType);
            return Process<T>(pTable, subTypes);
        }

        public static List<T> Process<T>(TableTwo pTable) where T : RelayObjectTwo
		{
            return Process<T>(pTable, typeof(T));
		}

        public static T Create<T>(TableTwo pTable) where T : RelayObjectTwo
        {
            Type t = typeof(T);
            T newItem = Activator.CreateInstance(t) as T;
            newItem.CreateNewRelayEntry(pTable, t.Name);
            return newItem;
        }

		static void PrintSubTypes(List<Type> subTypes)
		{
			foreach(Type t in subTypes) {
				Console.WriteLine(t.Name);
			}
            Console.ReadKey();
			Console.WriteLine("= " + subTypes.Count);
		}
    }
}


using System.Reflection;
using System.Diagnostics;
using testingus;



namespace namspone
{
	/// <summary> Attribute used to force non-public properties to be Serialized. </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class NSerialize : Attribute
	{

	}

	/// <summary> Attribute used to force public properties to be ignored during Serialization. </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class NIgnore : Attribute
	{

	}


	internal class NObject
	{
		public string objNamespace { get; set; }
		public string objAssembly { get; set; }
		public object thisObject { get; set; }
		public bool isArray { get; } = false;
		public Dictionary<string, object> properties = new();

		public NObject(object obj)
		{
			if (obj.GetType().IsArray)
			{
				isArray = true;
			}
			thisObject = obj;
		}


	}

	public class TestClass
	{
		public string TestString = "Hello!";
		public int TestInteger = 5;
		public TestClassSecond[] ClassArrayTest = { new(), new() };
		public float TestFloat = 3.14f;
		//public TestClassSecond testtwo = new();
		public TestClass() { }
	}




	public static class Program
	{
		static void Main()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			NFile nfil = new NFile(new TestClass());
			nfil.Save("test", "test");
			stopwatch.Stop();
			Console.WriteLine(nfil.fileString);
			Console.WriteLine("" + stopwatch.Elapsed.TotalSeconds + "s taken to serialize.");
		}
	}

	/// <summary> Neptune object Serialization. </summary>
	public class NFile
	{
		internal List<NObject> objects;
		internal List<object> propertyList = new();
		internal String fileString = "";
		internal bool instring = false;

		protected bool isStandardType(Type reftype)
		{
			if (

						 reftype == typeof(int)
						|| reftype == typeof(float)
						|| reftype == typeof(double)
						|| reftype == typeof(short)
						|| reftype == typeof(long)
						|| reftype == typeof(uint)
						|| reftype == typeof(ulong)
						|| reftype == typeof(ushort)
						|| reftype == typeof(char)
						|| reftype == typeof(bool)
						|| reftype == typeof(string))
			{
				return true;
			}
			else
				return false;

		}


		internal string Stringify(NObject targ)
		{
			string resultString = "";
			resultString += targ.objNamespace + '+' + targ.objAssembly;
			return resultString;
		}

		/// <summary> Finalize the Serialization and save it to a .NFile in the designated folder. </summary>
		public void Save(string path, string name)
		{
			int arrayIndex = 0;
			string fullpath = (path.EndsWith('\\') ? path : path + '\\') + name + ".NEPOBJ";
			Dictionary<uint, object> values = new();
			//FileStream fs = File.Create(fullpath);

			if (fileString == "")
			{
				fileString = "#DESC\n"
				+ "CREATED:" + DateTime.Now.Year + 'y' + DateTime.Now.Month + 'm' + DateTime.Now.Day + 'd' + DateTime.Now.Hour + ':' + DateTime.Now.Minute + '\n'
				+ "#OBJECTINDEX\n";
				for (int i = 0; i < objects.Count; i++)
				{
					NObject obj = objects[i];
					fileString += "" + i + ':' + obj.objAssembly + "#" + obj.objNamespace;
					if (obj.thisObject.GetType().IsArray)
					{
						fileString += '{';
						Array enumerab = (Array)obj.thisObject;
						//Console.WriteLine("array!!! " + enumerab.Length);
						for (int p = 0; p < enumerab.Length; p++)
						{
							int indx = propertyList.IndexOf(enumerab.GetValue(p));
							fileString += ("" + indx) + (p + 1 < enumerab.Length ? "," : "");
							arrayIndex++;
						}
						fileString += '}';
					}
					else
						if (obj.properties.Count > 0)
						{ fileString += '{'; }
						for (int c = 0; c < obj.properties.Count; c++)
						{
							int indx = objects.FindIndex(x => x.thisObject == obj.properties.Values.ToArray()[c]);
							fileString += '"' + obj.properties.Keys.ToArray()[c] + '"' + ':'
							+ (isStandardType(obj.properties.Values.ToArray()[c].GetType()) ? propertyList.IndexOf(obj.properties.Values.ToArray()[c]) : "o" + (indx == -1 ? propertyList.IndexOf(obj.properties.Values.ToArray()[c]) : indx) ); //obj.properties.Values.ToArray()[c];
							if (c + 1 < obj.properties.Count)
							{
								fileString += ';';
							}

						}
					if (obj.properties.Count > 0)
					{ fileString += '}'; }
					fileString += '\n';
				}

				fileString += "#VALUES\n";

				for (int i = 0; i < propertyList.Count; i++)
				{
					object obj = propertyList[i];


					if (obj.GetType() == typeof(string))
					{
						fileString += "" + i + ':' + '"' + (string)obj + '"' + '\n';
					}
					else if (obj.GetType() == typeof(char))
					{
						fileString += "" + i + ':' + '\'' + (char)obj + '\'' + '\n';
					}
					else if (obj.GetType() == typeof(int))
					{
						fileString += "" + i + ':' + 'i' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(long))
					{
						fileString += "" + i + ':' + 'l' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(short))
					{
						fileString += "" + i + ':' + 's' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(uint))
					{
						fileString += "" + i + ':' + 'u' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(ulong))
					{
						fileString += "" + i + ':' + "ul" + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(ushort))
					{
						fileString += "" + i + ':' + "us" + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(float))
					{
						fileString += "" + i + ':' + 'f' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(double))
					{
						fileString += "" + i + ':' + 'd' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(bool))
					{
						fileString += "" + i + ':' + ((bool)obj == true ? 'T' : 'F') + '\n';
					}



				}

				/*fileString += "#ARRAYDATA\n";
				int arrayindx = 0;

				for (int i = 0; i < objects.Count; i++)
				{
					NObject obj = objects[i];

					if (obj.thisObject.GetType().IsArray)
					{
						fileString += "" + arrayindx;
						arrayindx++;
					}


				}*/

				/*for (int i = 0; i < objects.Length; i++)
                {
                    NObject obj = objects[i];

                    if (obj.thisObject.GetType().IsArray)
                    {
                        fileString += "" + i + ":[";
                        foreach (object val in obj.thisObject)
                        {
                            fileString += val
                        }
                    }
                }*/

			}
			else
			{

			}
		}

		//public NFile(Resource file)
		//{

		//}

		/// <summary> Creates a new NFile for object Serialization. </summary>
		public NFile(object obj)
		{
			List<NObject> refedObjects = new();
			//List<object> arrayValues = new();

			Type objType = obj.GetType();

			NObject nobj = new NObject(obj);

			nobj.objNamespace = objType.FullName;
			nobj.objAssembly = objType.Assembly.GetName().Name;


			refedObjects.Add(nobj);


			void AnalyzeObject(object targ)
			{
				NObject ntarg = refedObjects.Find(b => b.thisObject == targ);

				Type targType = targ.GetType();
				ntarg.objNamespace = targType.FullName;
				ntarg.objAssembly = targType.Assembly.GetName().Name;



				FieldInfo[] objValues = targType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (FieldInfo info in objValues)
				{
					if (info.IsPublic)
					{
						bool found = false;
						foreach (Attribute atrrib in info.GetCustomAttributes())
						{
							if (atrrib.GetType() == typeof(NIgnore))
							{
								found = true;
								break;
							}
						}
						if (found)
						{
							continue;
						}
					}
					else
					{
						bool found = false;
						foreach (Attribute atrrib in info.GetCustomAttributes())
						{
							if (atrrib.GetType() == typeof(NSerialize))
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							continue;
						}
					}


					object refVal = info.GetValue(targ);
					//Console.WriteLine(refVal);
					Type reftype = refVal.GetType();
					if (reftype.IsArray && reftype != typeof(int[])
						&& reftype != typeof(float[])
						&& reftype != typeof(double[])
						&& reftype != typeof(short[])
						&& reftype != typeof(long[])
						&& reftype != typeof(uint[])
						&& reftype != typeof(ulong[])
						&& reftype != typeof(ushort[])
						&& reftype != typeof(char[])
						&& reftype != typeof(bool[])
						&& reftype != typeof(string[])
						)
					{
						refedObjects.Add(new NObject(refVal));
						nobj.properties.Add(info.Name, refVal);
						for (int i = 0; i < ((Array)refVal).Length; i++)
						{
							//refedObjects.Add(new NObject(refVal));
							if (!refedObjects.Exists(x => x.thisObject == ((Array)refVal).GetValue(i)))
							{
								refedObjects.Add(new NObject(((Array)refVal).GetValue(i)));
								if (!propertyList.Contains(((Array)refVal).GetValue(i)))
								{ propertyList.Add(((Array)refVal).GetValue(i)); }
							}
							else
							{
								nobj.properties.Add(info.Name, ((Array)refVal).GetValue(i));
								if (!propertyList.Contains(((Array)refVal).GetValue(i)))
								{ propertyList.Add(((Array)refVal).GetValue(i)); }
							}
							AnalyzeObject(((Array)refVal).GetValue(i));
						}
					}
					else if (reftype.IsArray)
					{
						refedObjects.Add(new NObject(refVal));
						ntarg.properties.Add(info.Name, refVal);
						for (int i = 0; i < ((Array)refVal).Length; i++)
						{
							//Console.WriteLine(i);
							if (!refedObjects.Exists(x => x.thisObject == ((Array)refVal).GetValue(i)))
							{
								
								//refedObjects.Add(new NObject(((Array)refVal).GetValue(i)));
								if (!propertyList.Contains(((Array)refVal).GetValue(i)))
								{ propertyList.Add(((Array)refVal).GetValue(i)); }
							}
							else
							{
								ntarg.properties.Add(info.Name, ((Array)refVal).GetValue(i));
								if (!propertyList.Contains(((Array)refVal).GetValue(i)))
								{ propertyList.Add(((Array)refVal).GetValue(i)); }
								//AnalyzeObject(((Array)refVal).GetValue(i));
							}
						}
						AnalyzeObject(refVal);
					}
					else
					{
						//Console.WriteLine(refVal);
						if (!refedObjects.Exists(x => x.thisObject == refVal))
						{
							refedObjects.Add(new NObject(refVal));
							ntarg.properties.Add(info.Name, refVal);
							if (!propertyList.Contains(refVal))
							{ propertyList.Add(refVal); }
						}
						else
						{
							ntarg.properties.Add(info.Name, refVal);
							if (!propertyList.Contains(refVal))
							{ propertyList.Add(refVal); }
						}

						AnalyzeObject(refVal);

					}
				}
			}

			FieldInfo[] objValues = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (FieldInfo info in objValues)
			{
				if (info.IsPublic)
				{
					bool found = false;
					foreach (Attribute atrrib in info.GetCustomAttributes())
					{
						if (atrrib.GetType() == typeof(NIgnore))
						{
							found = true;
							break;
						}
					}
					if (found)
					{
						continue;
					}
				}
				else
				{
					bool found = false;
					foreach (Attribute atrrib in info.GetCustomAttributes())
					{
						if (atrrib.GetType() == typeof(NSerialize))
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						continue;
					}
				}


				object refVal = info.GetValue(obj);
				Type reftype = refVal.GetType();
				if (reftype.IsArray && reftype != typeof(int[])
						&& reftype != typeof(float[])
						&& reftype != typeof(double[])
						&& reftype != typeof(short[])
						&& reftype != typeof(long[])
						&& reftype != typeof(uint[])
						&& reftype != typeof(ulong[])
						&& reftype != typeof(ushort[])
						&& reftype != typeof(char[])
						&& reftype != typeof(bool[])
						&& reftype != typeof(string[])
						)
				{
					//Console.WriteLine("non default array!");
					refedObjects.Add(new NObject(refVal));
					nobj.properties.Add(info.Name, refVal);
					for (int i = 0; i < ((Array)refVal).Length; i++)
					{
						//refedObjects.Add(new NObject(refVal));
						if (!refedObjects.Exists(x => x.thisObject == ((Array)refVal).GetValue(i)))
						{
							refedObjects.Add(new NObject(((Array)refVal).GetValue(i)));
							if (!propertyList.Contains(((Array)refVal).GetValue(i)))
							{ propertyList.Add(((Array)refVal).GetValue(i)); }
						}
						else
						{
							nobj.properties.Add(info.Name, ((Array)refVal).GetValue(i));
							if (!propertyList.Contains(((Array)refVal).GetValue(i)))
							{ propertyList.Add(((Array)refVal).GetValue(i)); }
						}
						AnalyzeObject(((Array)refVal).GetValue(i));
					}
					AnalyzeObject(refVal);
				}
				else
				{
					
					if (!refedObjects.Exists(x => x.thisObject == refVal))
					{
						refedObjects.Add(new NObject(refVal));
						nobj.properties.Add(info.Name, refVal);
						if (!propertyList.Contains(refVal))
						{ propertyList.Add(refVal); }
					}
					else
					{
						nobj.properties.Add(info.Name, refVal);
						if (!propertyList.Contains(refVal))
						{ propertyList.Add(refVal); }
					}
					AnalyzeObject(refVal);
				}

			}

			objects = refedObjects;
		}
	}


	public static class NSerializer
	{


	}

}
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
			TestClass testcloss = new TestClass();
			testcloss.TestFloat = 52.50f;
			Stopwatch stopwatch = Stopwatch.StartNew();
			NFile nfil = new NFile(testcloss);
			nfil.Save("test", "test");
			stopwatch.Stop();
			Console.WriteLine(nfil.fileString);
			Console.WriteLine("" + stopwatch.Elapsed.TotalSeconds + "s taken to serialize.");
			stopwatch.Restart();
			object woah = NFile.Deserialize(nfil.fileString);
			stopwatch.Stop();
			Console.WriteLine(((TestClass)woah).TestFloat);
			Console.WriteLine("" + stopwatch.Elapsed.TotalSeconds + "s taken to deserialize.");
		}
	}

	/// <summary> Neptune object Serialization. </summary>
	public class NFile
	{
		internal List<NObject> objects;
		internal List<object> propertyList = new();
		internal String fileString = "";

		public static object Deserialize(string NFileData)
		{
			string[] Lines = NFileData.Split('\n');
			List<object> Objects = new();
			List<string> ObjectLines = new();
			Dictionary<int,object> Values = new();
			bool readingString = false;

			int ObjectsHeaderLine = 0;
			int ValuesHeaderLine = 0;

			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i] == "#OBJECTS")
				{ ObjectsHeaderLine = i + 1; }
				else if (Lines[i] == "#VALUES")
				{ ValuesHeaderLine = i + 1; break;}
			}

			for (int i = ValuesHeaderLine; i < Lines.Length - 1; i++)
			{
				string Line = Lines[i];
				int NumberEnd = 0;
				for (int j = 0; j < Line.Length; j++)
				{
					if (Line[j] == ':')
					{ NumberEnd = j + 1; break; }
				}
				//Console.WriteLine(Line.Length);
				//Console.WriteLine(NumberEnd);
				//Console.WriteLine(Line);
				char ValueType = Line[NumberEnd];

				object Value = null;
				if (ValueType == 'T')
				{ Value = true; }
				else if (ValueType == 'F')
				{ Value = false; }
				else if (ValueType == 'i')
				{ Value = int.Parse(Line.Substring(NumberEnd + 1, Line.Length - 1 - NumberEnd)); }
				else if (ValueType == 'f')
				{ Value = float.Parse(Line.Substring(NumberEnd + 1, Line.Length - 1 - NumberEnd)); }

				else if (ValueType == '"')
				{ Value = Line.Substring(NumberEnd + 1, Line.Length - 2 - NumberEnd); }
				else if (ValueType == '\'')
				{ Value = Line[NumberEnd + 2]; }

				else if (ValueType == 'd')
				{ Value = double.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }
				else if (ValueType == 'l')
				{ Value = long.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }

				else if (ValueType == 's')
				{ Value = double.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }

				else if (ValueType == 'I')
				{ Value = long.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }
				else if (ValueType == 'L')
				{ Value = long.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }
				else if (ValueType == 'S')
				{ Value = long.Parse(Line.Substring(NumberEnd + 1, Line.Length - NumberEnd)); }

				//Console.WriteLine(Line[NumberEnd]);
				//Console.WriteLine("" + NumberEnd + ' ' + Line.Substring(0, NumberEnd));
				Values.Add(int.Parse(Line.Substring(0, NumberEnd - 1)), Value);
			}

			int mixups = 0;
			//Console.WriteLine("AAAAAAAAAAAAAAA " + (Lines.Length - ValuesHeaderLine));
			for (int i = ObjectsHeaderLine; i < Lines.Length + ObjectsHeaderLine + 5 - ValuesHeaderLine; i++)
			{
				string Line = Lines[i];
				int SeparatorIndex = 0;
				int TypeEndIndex = 0;
				bool HasProperties = false;
				bool IsArray = false;

				for (int j = 0; j < Line.Length; j++)
				{
					char c = Line[j];
					if (c == '#')
					{ SeparatorIndex = j; }
					else if (c == '{')
					{ TypeEndIndex = j - 1; break; }
				}
				if (TypeEndIndex == 0)
				{
					TypeEndIndex = Line.Length - 1;
				}
				else
				{
					HasProperties = true;
				}

				string AssemblyName = Line[..(SeparatorIndex)];
				//Console.WriteLine(AssemblyName);
				string TypeName = Line.Substring(SeparatorIndex + 1, TypeEndIndex - SeparatorIndex);
				if (TypeName.EndsWith("[]"))
				{
					IsArray = true;
					TypeName = Line.Substring(SeparatorIndex + 1, TypeEndIndex - SeparatorIndex - 2);
				}


				//Objects.Add(AssemblyName + ' ' + TypeName);

				object NewObject = null;

				if (TypeName == "System.String" && !IsArray)
				{
					NewObject = "";
				}
				else if (TypeName == "System.Int32" && !IsArray)
				{
					NewObject = 0;
				}
				else if (TypeName == "System.Single" && !IsArray)
				{
					NewObject = 0.0f;
				}
				else if (TypeName == "System.Boolean" && !IsArray)
				{
					NewObject = true;
				}
				else
				{
					if (IsArray)
					{
						string[] PropertyStrings2 = Line.Substring(TypeEndIndex + 2, Line.Length - TypeEndIndex - 3).Split(',');
						NewObject = Array.CreateInstance(Type.GetType(TypeName), PropertyStrings2.Length);
					}
					else
					{ NewObject = Activator.CreateInstance(AssemblyName, TypeName).Unwrap(); }
				}
				mixups++;

				//if (NewObject == null)
				//{ Console.WriteLine("object was null :("); }

				Objects.Add(NewObject);
				ObjectLines.Add(Line);

			}

			//Console.WriteLine("mixups: " + mixups);

			//Console.WriteLine("Objects: " + Objects.Count);

			//foreach (object obj in Objects)
			//{
			//	Console.WriteLine("Object: " + obj);
			//}

			for (int i = 0; i < Objects.Count; i++)
			{
				object NewObject = Objects.ToArray()[i];
				string Line = ObjectLines.ToArray()[i];
				//Console.WriteLine(Line);
				int SeparatorIndex = 0;
				int TypeEndIndex = 0;
				bool HasProperties = false;
				bool IsArray = NewObject.GetType().IsArray;

				for (int j = 0; j < Line.Length; j++)
				{
					char c = Line[j];
					if (c == '#')
					{ SeparatorIndex = j; }
					else if (c == '{')
					{ TypeEndIndex = j - 1; break; }
				}
				if (TypeEndIndex == 0)
				{
					TypeEndIndex = Line.Length - 1;
				}
				else
				{
					HasProperties = true;
				}

				if (HasProperties && !IsArray)
				{
					string[] PropertyStrings = Line.Substring(TypeEndIndex + 2, Line.Length - TypeEndIndex - 3).Split(';');
					foreach (string str in PropertyStrings)
					{

						//Console.WriteLine("stringle: " + str);
						int NumberSplit = str.LastIndexOf(':');
						//Console.WriteLine(NumberSplit);
						//Console.WriteLine(str.Substring(1, NumberSplit - 2));
						//Console.WriteLine(str[(NumberSplit + 1)..]);
						//Console.WriteLine(NewObject);
						//Console.WriteLine(NewObject.GetType().GetFields().Length);
						//foreach (FieldInfo prop in NewObject.GetType().GetFields())
						//{
						//	Console.WriteLine(prop.Name);
						//}
						string ValString = str[(NumberSplit + 1)..];
						if (ValString.StartsWith('o'))
						{
							ValString = ValString[1..];
							//Console.WriteLine(ValString);
							NewObject.GetType().GetField(str.Substring(1, NumberSplit - 2), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(NewObject, Objects.ElementAt(Int32.Parse(ValString)));
						}
						else
						{
							//Console.WriteLine(Values[Int32.Parse(ValString)]);
							//Console.WriteLine(NewObject == null);
							//Console.WriteLine(str.Substring(1, NumberSplit - 2));
							NewObject.GetType().GetField(str.Substring(1, NumberSplit - 2),BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(NewObject, Values[Int32.Parse(ValString)]);
							//Console.WriteLine("RAAAAAAGH");
						}
					}

				}
				else if (IsArray)
				{
					string[] PropertyStrings = Line.Substring(TypeEndIndex + 2, Line.Length - TypeEndIndex - 3).Split(',');
					Array array = ((Array)NewObject); //Array.CreateInstance(NewObject.GetType(), PropertyStrings.Length);
					//Console.WriteLine("array length = " + array.Length);

					//Console.WriteLine(NewObject.GetType());
					//Console.WriteLine(array.GetType());
					int j = 0;
					if (isStandardArray(array.GetType()))
					{
						foreach (string str in PropertyStrings)
						{
							//Console.WriteLine((Objects.ElementAt(Int32.Parse(str)).Key.GetType()));
							//Console.WriteLine(j);
							//Console.WriteLine("array length = " + array.Length);
							//Console.WriteLine(Values[Int32.Parse(str)]);
							//Console.WriteLine(array);
							array.SetValue(Values[Int32.Parse(str)], j);
							j++;
							break;
						}
					}
					else
					{
						foreach (string str in PropertyStrings)
						{
							//Console.WriteLine((Objects.ElementAt(Int32.Parse(str)).Key.GetType()));
							//Console.WriteLine(j);
							//Console.WriteLine("array length = " + array.Length);
							//Console.WriteLine((Objects.ElementAt(Int32.Parse(str))));
							//Console.WriteLine(array);
							array.SetValue(Objects.ElementAt(Int32.Parse(str)), j);
							j++;
							break;
						}
					}
				}
			}
		
			
			/*for (int i = 0; i < Objects.Count; i++)
			{
				Console.WriteLine(Objects.Keys.ToArray()[i]);
			}*/

			return Objects.ToArray()[0];
		}
	
		
	

		protected static bool isStandardType(Type reftype)
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

		protected static bool isStandardArray(Type reftype)
		{
			if (

						 reftype == typeof(int[])
						|| reftype == typeof(float[])
						|| reftype == typeof(double[])
						|| reftype == typeof(short[])
						|| reftype == typeof(long[])
						|| reftype == typeof(uint[])
						|| reftype == typeof(ulong[])
						|| reftype == typeof(ushort[])
						|| reftype == typeof(char[])
						|| reftype == typeof(bool[])
						|| reftype == typeof(string[]))
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
				+ "#OBJECTS\n";
				for (int i = 0; i < objects.Count; i++)
				{
					NObject obj = objects[i];
					fileString += "" + obj.objAssembly + "#" + obj.objNamespace;
					if (obj.thisObject.GetType().IsArray)
					{
						fileString += '{';
						Array enumerab = (Array)obj.thisObject;
						//Console.WriteLine("array!!! " + enumerab.Length);
						for (int p = 0; p < enumerab.Length; p++)
						{
							if (isStandardType(enumerab.GetValue(p).GetType()))
							{
								int indx = propertyList.IndexOf(enumerab.GetValue(p));
								fileString += ("" + indx) + (p + 1 < enumerab.Length ? "," : "");
							}
							else
							{
								int indx = objects.FindIndex(x => x.thisObject == enumerab.GetValue(p));
								fileString += ("" + indx) + (p + 1 < enumerab.Length ? "," : "");
							}
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
						fileString += "" + i + ':' + 'I' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(ulong))
					{
						fileString += "" + i + ':' + 'L' + Convert.ToString(obj) + '\n';
					}
					else if (obj.GetType() == typeof(ushort))
					{
						fileString += "" + i + ':' + 'S' + Convert.ToString(obj) + '\n';
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

			nobj.objNamespace = objType.Namespace + '.' + objType.Name;
			nobj.objAssembly = objType.Assembly.GetName().Name;


			refedObjects.Add(nobj);


			void AnalyzeObject(object targ)
			{
				NObject ntarg = refedObjects.Find(b => b.thisObject == targ);

				Type targType = targ.GetType();
				ntarg.objNamespace = targType.Namespace + '.' + targType.Name;
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
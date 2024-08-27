using NeptuneSerialzer;
using System.Diagnostics;


namespace Example
{
	public class TestClass
	{
		public string TestString = "Hello!";
		public int TestInteger = 5;
		public TestClassSecond[] ClassArrayTest = { new TestClassSecond(), new TestClassSecond() };
		public float TestFloat = 3.14f;
		public TestClassSecond testtwo = new();
		public TestClass() { }
	}


	public class TestClassSecond
	{
		[NSerialize] private string PrivateValue = "This property is private, but is still Serialized due to [NSerialize]";
		[NIgnore] public string PublicIgnoredValue = "This property is public, but will not be Serialized due to [NIgnore]";
		public string PublicValue = "This property is public and will be Serialized.";
		public bool TestBoolValue = true;
		public bool[] TestBoolArray = { true, false, true };
		public TestClassSecond() { }
	}


	public static class Program
	{
		static void Main()
		{
			TestClass testClass = new TestClass();
			testClass.TestFloat = 52.50f;

			Stopwatch stopwatch = Stopwatch.StartNew();

			string SerializationString = NSerializer.Serialize(testClass);

			stopwatch.Stop();

			Console.WriteLine(SerializationString);
			Console.WriteLine("" + stopwatch.Elapsed.TotalSeconds + "s taken to serialize.");
			stopwatch.Restart();

			object DeserializedObject = NSerializer.Deserialize(SerializationString);

			stopwatch.Stop();
			Console.WriteLine("" + stopwatch.Elapsed.TotalSeconds + "s taken to deserialize.");
		}
	}
}

using namspone;

namespace testingus
{

	

		public class TestClassSecond
		{

			[NSerialize] private string PrivateValue = "This property is private, but is still Serialized due to [NSerialize]";
			[NIgnore] public string PublicIgnoredValue = "This property is public, but will not be Serialized due to [NIgnore]";
			public string PublicValue = "This property is public and will be Serialized.";
			public bool TestBoolValue = true;
			public bool[] TestBoolArray = { true, false, true };
			public TestClassSecond() { }
		}
	
}
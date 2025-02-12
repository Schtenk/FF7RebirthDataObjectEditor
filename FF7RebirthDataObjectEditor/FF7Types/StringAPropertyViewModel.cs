using FF7R2.DataObject.Properties;

namespace FF7RebirthDataObjectEditor.FF7Types;

public class StringAPropertyViewModel(PropertyValue propertyValue) : APropertyViewModel<string>(propertyValue)
{
	protected override bool TryParse(string input, out string result)
	{
		result = input;
		return true;
	}
}
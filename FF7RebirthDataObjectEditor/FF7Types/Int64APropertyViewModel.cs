using FF7R2.DataObject.Properties;

namespace FF7RebirthDataObjectEditor.FF7Types;

public class Int64APropertyViewModel(PropertyValue propertyValue) : APropertyViewModel<long>(propertyValue)
{
	protected override bool TryParse(string input, out long result) => long.TryParse(input, out result);
}
using FF7R2.DataObject.Properties;

namespace FF7RebirthDataObjectEditor.FF7Types;

public class FloatAPropertyViewModel(PropertyValue propertyValue) : APropertyViewModel<float>(propertyValue)
{
	protected override bool TryParse(string input, out float result) => float.TryParse(input, out result);
}
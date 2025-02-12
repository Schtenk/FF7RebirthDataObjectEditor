using FF7R2.DataObject.Properties;

namespace FF7RebirthDataObjectEditor.FF7Types;

public class UInt16APropertyViewModel(PropertyValue propertyValue) : APropertyViewModel<ushort>(propertyValue)
{
	protected override bool TryParse(string input, out ushort result) => ushort.TryParse(input, out result);
}
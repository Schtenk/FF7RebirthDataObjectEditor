using CUE4Parse.GameTypes.FF7.Assets.Objects.Properties;
using FF7R2.DataObject.Properties;

namespace FF7RebirthDataObjectEditor.FF7Types;

public static class PropertyViewModelFactory
{
	public static IPropertyViewModel Create(PropertyValue propertyValue)
	{
		if (propertyValue is ArrayProperty arrayProperty)
			return new ArrayPropertyViewModel(arrayProperty);
		
		return propertyValue.property.UnderlyingType switch
		{
			FF7propertyType.BoolProperty => new BoolAPropertyViewModel(propertyValue),
			FF7propertyType.ByteProperty => new ByteAPropertyViewModel(propertyValue),
			FF7propertyType.Int8Property => new Int8APropertyViewModel(propertyValue),
			FF7propertyType.UInt16Property => new UInt16APropertyViewModel(propertyValue),
			FF7propertyType.Int16Property => new Int16APropertyViewModel(propertyValue),
			FF7propertyType.UIntProperty => new UIntAPropertyViewModel(propertyValue),
			FF7propertyType.IntProperty => new IntAPropertyViewModel(propertyValue),
			FF7propertyType.Int64Property => new Int64APropertyViewModel(propertyValue),
			FF7propertyType.FloatProperty => new FloatAPropertyViewModel(propertyValue),
			FF7propertyType.StrProperty => new StringAPropertyViewModel(propertyValue),
			FF7propertyType.NameProperty => new NameAPropertyViewModel(propertyValue),
			_ => throw new NotSupportedException($"Unsupported property type?: {propertyValue.property.UnderlyingType}")
		};
	}
}
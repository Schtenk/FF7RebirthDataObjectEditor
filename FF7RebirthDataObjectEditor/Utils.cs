using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using FF7RebirthDataObjectEditor.FF7Types;

namespace FF7RebirthDataObjectEditor;

public static class Utils
{
	public static string ExportToCsv(ObservableCollection<EntryRow> entries)
	{
		var sb = new StringBuilder();

		var headerColumns = new List<string> { "Index", "Name" };

		var firstEntry = entries.First();
		var propertyCount = firstEntry.Data.Properties.Count;
		var childCountByProperty = new int[propertyCount];

		foreach (var entry in entries)
		{
			var entryProperties = entry.Data.Properties;
			for (var i = 0; i < propertyCount; ++i)
				childCountByProperty[i] = Math.Max(childCountByProperty[i], entryProperties[i].Children.Count);
		}

		for (var i = 0; i < propertyCount; ++i)
		{
			var property = firstEntry.Data.Properties[i];
			headerColumns.Add(property.Name);
			for (int childIndex = 0; childIndex < childCountByProperty[i]; ++childIndex)
				headerColumns.Add((childIndex+1).ToString());
		}
        
		sb.AppendLine(string.Join(",", headerColumns.Select(EscapeForCsv)));

		foreach (var row in entries)
		{
			var rowValues = new List<string>
			{
				row.Index.ToString(),
				row.Name
			};

			for (var i = 0; i < propertyCount; ++i)
			{
				var property = row.Data.Properties[i];
				var expectedChildren = childCountByProperty[i];
				if (expectedChildren == 0)
					rowValues.Add(property.Value);
				else
				{
					rowValues.Add(null);
                    
					foreach (var child in property.Children)
						rowValues.Add(child.Value);
                    
					var expectedEmpties = expectedChildren - property.Children.Count;
					for (int emptyIndex = 0; emptyIndex < expectedEmpties; ++emptyIndex)
						rowValues.Add(null);
				}
			}
            
			sb.AppendLine(string.Join(",", rowValues.Select(EscapeForCsv)));
		}

		return sb.ToString();
	}
	public static IEnumerable<IPropertyViewModel> Flatten(IEnumerable<IPropertyViewModel> rootItems)
	{
		var stack = new Stack<IPropertyViewModel>(rootItems);

		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;

			if (current.Children == null)
				continue;
            
			foreach (var child in current.Children)
				stack.Push(child);
		}
	}
	public static IEnumerable<IPropertyViewModel> FlattenProperties(IEnumerable<IPropertyViewModel> properties)
	{
		foreach (var prop in properties)
		{
			yield return prop;
			if (prop.Children != null && prop.Children.Any())
			{
				foreach (var child in FlattenProperties(prop.Children))
				{
					yield return child;
				}
			}
		}
	}

	public static string EscapeForCsv(string field)
	{
		if (field == null)
			return field;
		if (field.Contains(",") || field.Contains("\""))
		{
			field = field.Replace("\"", "\"\"");
			return $"\"{field}\"";
		}
		return field;
	}
	private static IEnumerable<string> ParseCsvLine(string line)
	{
		if (string.IsNullOrEmpty(line))
			yield break;

		var inQuotes = false;
		var value = new StringBuilder();

		for (int i = 0; i < line.Length; i++)
		{
			var c = line[i];

			if (inQuotes)
			{
				if (c == '"')
				{
					if (i + 1 < line.Length && line[i + 1] == '"')
					{
						value.Append('"');
						i++;
					}
					else
						inQuotes = false;
				}
				else
					value.Append(c);
			}
			else
			{
				if (c == '"')
					inQuotes = true;
				else if (c == ',')
				{
					yield return value.ToString();
					value.Clear();
				}
				else
					value.Append(c);
			}
		}
		yield return value.ToString();
	}
    
	public static void ImportFromCsv(string filePath, PropertyGridControl propertyGrid)
	{
		var lines = File.ReadAllLines(filePath);
		if (lines.Length < 2)
			throw new Exception("CSV file does not contain any data rows.");

		var firstEntry = propertyGrid.AssetEntries.First();
		var propertyCount = firstEntry.Data.Properties.Count;
		var childCountByProperty = new int[propertyCount];
		var headerParts = ParseCsvLine(lines[0]).ToList();
		var headerIndex = 2;
		for (var propertyIndex = 0; propertyIndex < propertyCount; ++propertyIndex)
		{
			var property = firstEntry.Data.Properties[propertyIndex];
			if (!IsComplexProperty(property))
			{
				headerIndex++;
				continue;
			}

			headerIndex++;
			while (headerIndex < headerParts.Count && int.TryParse(headerParts[headerIndex], out _))
			{
				headerIndex++;
				childCountByProperty[propertyIndex]++;
			}
		}

		for (int i = 1; i < lines.Length; i++)
		{
			var line = lines[i];
			var entry = propertyGrid.AssetEntries[i - 1];
			ApplyValuesFromLine(line, entry, propertyCount, childCountByProperty);
		}
	}

	private static void ApplyValuesFromLine(string line, EntryRow entry, int propertyCount, int[] childCountByProperty)
	{
		var parts = ParseCsvLine(line).ToList();

		if (parts.Count < 2)
			return;

		int partIndex = 2;
		var properties = entry.Data.Properties.ToList();
		for (var propertyIndex = 0; propertyIndex < propertyCount; ++propertyIndex)
		{
			var property = properties[propertyIndex];
			if (!IsComplexProperty(property))
			{
				property.Value = parts[partIndex++];
				continue;
			}
			partIndex++;
			var childCount = childCountByProperty[propertyIndex];
			var tempChildren = new List<string>();
			for (var childIndex = 0; childIndex < childCount; ++childIndex)
				tempChildren.Add(parts[partIndex++]);
                
			for (var childIndex = tempChildren.Count - 1; childIndex >= 0; childIndex--)
			{
				if (!string.IsNullOrWhiteSpace(tempChildren[childIndex]))
					break;
				tempChildren.RemoveAt(childIndex);
			}

			MatchArrayToReadArray(property, tempChildren);
		}
	}

	private static void MatchArrayToReadArray(IPropertyViewModel property, List<string> tempChildren)
	{
		var realChildren = property.Children.ToList();
		var realChildrenCount = realChildren.Count;
		var smallerSize = Math.Min(realChildren.Count, tempChildren.Count);
		for (var childIndex = 0; childIndex < smallerSize; ++childIndex)
			realChildren[childIndex].Value = tempChildren[childIndex];

		var theArray = property as ArrayPropertyViewModel;
                
		//we're over, remove
		for (var childIndex = realChildrenCount - 1; childIndex >= smallerSize; --childIndex)
			theArray.RemoveAt(childIndex);
                
		//we were under, insert
		for (var childIndex = smallerSize; childIndex < tempChildren.Count; ++childIndex)
		{
			var childProperty = theArray.InsertAt(childIndex, theArray.CreatePropertyValue());
			childProperty.Value = tempChildren[childIndex];
		}
	}


	public static bool IsComplexProperty(IPropertyViewModel property)
	{
		return property is ArrayPropertyViewModel;
	}

}
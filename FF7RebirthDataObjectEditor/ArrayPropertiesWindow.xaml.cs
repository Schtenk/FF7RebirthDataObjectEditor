using System.Collections.ObjectModel;
using System.Windows;
using FF7R2.DataObject.Properties;
using FF7RebirthDataObjectEditor.FF7Types;

namespace FF7RebirthDataObjectEditor;

public partial class ArrayPropertiesWindow : Window
{
	private ArrayPropertyViewModel _array;
	public ObservableCollection<EntryRow> AssetEntries { get; set; } = new();

	public ArrayPropertiesWindow(ArrayPropertyViewModel array)
	{
		_array = array;
		DataContext = this;
		InitializeComponent();
		Repaint();
	}

	private void Repaint()
	{
		AssetEntries.Clear();
		var arrayPropertySubProperties = _array.Children;
        
		var index = 0;
		foreach (var value in arrayPropertySubProperties)
		{
			var wrappedEntry = new RowPropertiesViewModel(value);
			AssetEntries.Add(new EntryRow { Index = (index++), Name = $"{value.Name}", Data = wrappedEntry });
		}
		
		PropertyGrid.GenerateColumns(false);
	}

	private void AddElement_Click(object sender, RoutedEventArgs e)
	{
		var instancedArrayElement = _array.CreatePropertyValue();
		
		var insertIndex = _array.Children.Count;
		if (PropertyGrid.assetDataGrid.SelectedItem is EntryRow assetEntry)
			insertIndex = assetEntry.Index + 1;
		_array.InsertAt(insertIndex, instancedArrayElement);
		Repaint();
	}

	private void RemoveElement_Click(object sender, RoutedEventArgs e)
	{
		if (PropertyGrid.assetDataGrid.SelectedItem is EntryRow assetEntry)
			_array.RemoveAt(assetEntry.Index);
		Repaint();
	}
}
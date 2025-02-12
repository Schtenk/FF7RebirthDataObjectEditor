using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FF7RebirthDataObjectEditor;

public class MainViewModel : INotifyPropertyChanged
{
	private string _filePath;

	public string FilePath
	{
		get => _filePath;
		set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
	}

	public ObservableCollection<EntryRow> AssetEntries { get; set; } = new();

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged(string name) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
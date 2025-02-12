using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FF7R2.DataObject;
using FF7RebirthDataObjectEditor.FF7Types;
using Microsoft.Win32;

namespace FF7RebirthDataObjectEditor;

public partial class MainWindow : Window
{
    private const string SettingsFileName = "appsettings.json";
        
    private readonly MainViewModel _viewModel;
    private AppSettings _settings;
    private IoStoreAsset _asset;


    public MainWindow()
    {
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        InitializeComponent();
        LoadSettings();
        _viewModel.FilePath = _settings.LastOpenFile;
        RefreshForFilePath(_viewModel.FilePath);
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialogue = new OpenFileDialog
        {
            Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*"
        };

        if (TryGetTargetDirectory(_settings.LastOpenFile, out var directory))
            dialogue.InitialDirectory = directory;

        if (dialogue.ShowDialog() != true)
            return;
            
        var filePath = dialogue.FileName;
        _viewModel.FilePath = filePath;

        RefreshForFilePath(filePath);
        
        _settings.LastOpenFile = filePath;
        SaveSettings();
    }
    
    private void ExportCSVButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            Title = "Export to CSV"
        };

        if (dlg.ShowDialog() != true)
            return;
        
        var csvText= Utils.ExportToCsv(PropertyGrid.AssetEntries);
        try
        {
            File.WriteAllText(dlg.FileName, csvText);
            new Process
            {
                StartInfo = new ProcessStartInfo(dlg.FileName)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportCSVButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            Title = "Import from CSV"
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                Utils.ImportFromCsv(dlg.FileName, PropertyGrid);
                MessageBox.Show("Import completed successfully.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


    private void RefreshForFilePath(string filePath)
    {
        _viewModel.AssetEntries.Clear();

        if (string.IsNullOrWhiteSpace(filePath) || !new FileInfo(filePath).Exists)
            return;

        try
        {
            _asset = IoStoreAsset.Load(filePath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }

        var index = 0;
        foreach (var (key, value) in _asset.innerAsset.frozenObject.DataTable)
        {
            var wrappedEntry = new RowPropertiesViewModel(value);
            _viewModel.AssetEntries.Add(new EntryRow 
                { 
                    Index = (index++), 
                    Name = $"{key.ToString()}", 
                    Data = wrappedEntry 
                });
        }

        PropertyGrid.GenerateColumns(true);
    }

    private void AddAnyNewFNames()
    {
        var properties = new List<NamePropertyViewModel>();
        foreach (var data in _viewModel.AssetEntries)
            properties.AddRange(Utils.Flatten(data.Data.Properties).OfType<NamePropertyViewModel>());

        foreach (var prop in properties)
        {
            var wantedString = prop.UserText;
            if (wantedString == null)
                continue;
                        
            var originalFNamesTexts = _asset.Names.Select(x => x.Text).ToList();
            var matchingExistingFNameIndex = originalFNamesTexts.IndexOf(wantedString);
            if (matchingExistingFNameIndex != -1)
                prop.Data = originalFNamesTexts[matchingExistingFNameIndex];
            else
                prop.Data = _asset.AddFName(wantedString);
        }
    }

    private void LoadSettings()
    {
        _settings = new AppSettings();
        try
        {
            if (!File.Exists(SettingsFileName))
                return;
            var json = File.ReadAllText(SettingsFileName);
            _settings = JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch
        {
        }
    }

    private void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFileName, json);
        }
        catch
        {
        }
    }

    private void SaveFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialogue = new SaveFileDialog()
        {
            Filter = "UAsset files (*.uasset)|*.uasset|All files (*.*)|*.*"
        };

        if (TryGetTargetDirectory(_settings.LastSaveFile, out var directory))
            dialogue.InitialDirectory = directory;

        if (dialogue.ShowDialog() != true)
            return;
            
        var filePath = dialogue.FileName;

        _settings.LastSaveFile = filePath;
        SaveSettings();
            
        SaveFileTo(filePath);
    }

    public void SaveFileTo(string filePath)
    {
        AddAnyNewFNames();
            
        _asset.Save(filePath, IoStoreAsset.Mode.WRITE_PARSED_DATA);
    }

    private bool TryGetTargetDirectory(string targetFile, out string directory)
    {
        directory = default;
        if (string.IsNullOrWhiteSpace(targetFile))
            return false;
        directory = Path.GetDirectoryName(targetFile);
        return Directory.Exists(directory);
    }
    
}
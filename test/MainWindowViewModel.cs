using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using NAudio.Wave;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave.SampleProviders;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace test;

public partial class MainWindowViewModel : ObservableObject
{
	[ObservableProperty]
	public string _filePath;
	[ObservableProperty]
	public string _fileTargetPath;
	[ObservableProperty]
	public ObservableCollection<string> _files;
	[ObservableProperty]
	public List<string> _addfiles;
	[ObservableProperty]
	public List<string> _chosenfiles;
	[ObservableProperty]
	public List<string> _fullFilePaths;
	[ObservableProperty]
	public string _logWindow;
    [ObservableProperty]
    public bool _stopCommandReceived;

    public MainWindowViewModel()
	{
		_filePath = "Enter path to files";
		_files = new ObservableCollection<string>();
		_addfiles = new List<string>();
		_chosenfiles = new List<string>();
		_fullFilePaths = new List<string>();
		_fileTargetPath = "Enter path to save files";
		_logWindow = string.Empty;
        _stopCommandReceived = false;

    }

	[RelayCommand]
	public void ExcecuteSetPath()
	{
		try
		{
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select a directory";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

				if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				string selectedPath = folderBrowserDialog.SelectedPath;
				FilePath = selectedPath;
				GetFilesFromDirectory();
				LogWindow += $"Source path set to: {selectedPath}\n";
			}
		}
		catch (Exception ex)
		{
			LogWindow += $"Error setting source path: {ex.Message}\n";
		}
	}

	[RelayCommand]
	public void ExcecuteSetTargetPath()
	{
		try
		{
			using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select a directory";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

				if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				string selectedPath = folderBrowserDialog.SelectedPath;
				FileTargetPath = selectedPath;
				LogWindow += $"Target path set to: {selectedPath}\n";
			}
		}
		catch (Exception ex)
		{
			LogWindow += $"Error setting target path: {ex.Message}\n";
		}
	}

    public void StartPlaybackThread()
    {
        try
        {
            Task.Run(() => {MergeSoundFiles(); });
        }
        catch (Exception ex)
        {
            LogWindow += $"Error starting playback thread: {ex.Message}\n";
        }
    }
    [RelayCommand]
	public void MergeSoundFiles()
	{
		try
		{
			// Ensure the target path is set
			if (string.IsNullOrEmpty(FileTargetPath))
			{
				LogWindow = "Please set the target path to save the merged file.";
				return;
			}

			// Create the full file paths for the chosen files
			CreateFullFilePaths();

			// Create a list to hold the audio file readers
			List<AudioFileReader> audioFileReaders = new List<AudioFileReader>();

			// Add each audio file reader to the list
			foreach (string filePath in FullFilePaths)
			{
				audioFileReaders.Add(new AudioFileReader(filePath));
			}

			// Create a concatenating sample provider
			var concatenatedProvider = new ConcatenatingSampleProvider(audioFileReaders);

			// Initialize the output device
			using (var outputDevice = new WaveOutEvent())
			{
				outputDevice.Init(concatenatedProvider);
				outputDevice.Play();

				// Wait for playback to finish
				while (outputDevice.PlaybackState == PlaybackState.Playing)
				{
                    if (StopCommandReceived)
                    {
                        outputDevice.Stop();
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
				}
			}
            // Dispose the audio file readers
            foreach (var reader in audioFileReaders)
			{
				reader.Dispose();
			}

			LogWindow += "Audio files merged and played successfully!\n";
		}
		catch (Exception ex)
		{
			LogWindow += $"An error occurred while merging audio files: {ex.Message}\n";
		}
        finally
        {
            StopCommandReceived = false;
        }
	}

    public void CreateFullFilePaths()
	{
		FullFilePaths.Clear();
		foreach (string file in Chosenfiles)
		{
			FullFilePaths.Add(Path.Combine(FilePath, file));
		}
	}

	private void GetFilesFromDirectory()
	{
		Files.Clear();
		foreach (string file in Directory.GetFiles(FilePath))
		{
			if (file.Contains(".wav") || file.Contains(".mp3"))
			{
				Files.Add(Path.GetFileName(file));
			}
		}
	}
}

using System.Windows.Controls;
using System.Windows.Input;

using ListBox = System.Windows.Forms.ListBox;

namespace test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
    private void RemoveItems(object sender, System.Windows.RoutedEventArgs e)
    {
        foreach(var item in AudioFiles.SelectedItems)
        {
            ((MainWindowViewModel)DataContext).Chosenfiles.Remove(item.ToString());
        }
        ((MainWindowViewModel)DataContext).CreateFullFilePaths();
    }
    private void ItemsSelected(object sender, System.Windows.RoutedEventArgs e)
    {
        List<string> selectedFiles = new List<string>();
        foreach (var item in AudioSources.SelectedItems)
        {
            selectedFiles.Add(item.ToString());
        }
        ((MainWindowViewModel)DataContext).Chosenfiles = selectedFiles;
        ((MainWindowViewModel)DataContext).CreateFullFilePaths();
    }

    private void MergeClick(object sender, System.Windows.RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).StartPlaybackThread();
    }

    private void StopPlay(object sender, System.Windows.RoutedEventArgs e)
    {
        ((MainWindowViewModel)DataContext).StopCommandReceived = true;
    }
}

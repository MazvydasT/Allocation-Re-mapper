using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace AllocationRerouter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "eM-Planner data (*.xml)|*.xml",
                Multiselect = true,
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                ReadOnlyChecked = true
            })
            {
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK && openFileDialog.FileNames.Length > 0) return;

                ListView listView = null;

                if (sender == BrowseOperations)
                    listView = OperationsList;

                else if (sender == BrowseOldEBOMs)
                    listView = OldEBOMList;

                else if (sender == BrowseNewEBOMs)
                    listView = NewEBOMList;

                if (listView == null) return;

                listView.ItemsSource = (listView.ItemsSource?.OfType<string>() ?? new string[0]).Where(p => !openFileDialog.FileNames.Contains(p)).Concat(openFileDialog.FileNames.Distinct());
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var deletedEntry = (string)button.DataContext;
            var listView = (ListView)button.Tag;

            listView.ItemsSource = listView.ItemsSource.OfType<string>().Where(entry => entry != deletedEntry);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressTracker.Progress.Reset();

            if (OperationsList.Items.Count == 0 || OldEBOMList.Items.Count == 0 || NewEBOMList.Items.Count == 0) return;

            var state = GlobalState.State;
            var progress = ProgressTracker.Progress;

            Stream outputStream;
            string outputFileName;

            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "eM-Planner data (*.xml)|*.xml",
                CheckPathExists = true,
                OverwritePrompt = true,
                RestoreDirectory = true
            })
            {
                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                state.Idle = false;
                state.ActiveView = GlobalState.View.Stats;

                outputStream = saveFileDialog.OpenFile();
                outputFileName = saveFileDialog.FileName;
            }

            Task.Run(() =>
            {
                using (outputStream)
                {
                    var exception = Mapper.Map(OperationsList.ItemsSource.OfType<string>().ToList(),
                        OldEBOMList.ItemsSource.OfType<string>().ToList(),
                        NewEBOMList.ItemsSource.OfType<string>().ToList(),
                        outputStream);

                    if (exception != null)
                    {
                        outputStream.Dispose();



                        progress.State = TaskbarItemProgressState.Error;
                        progress.Max = 1;
                        progress.Value = 1;

                        if (File.Exists(outputFileName))
                            File.Delete(outputFileName);

                        Console.Error.WriteLine(exception);
                    }

                    else
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!AppWindow.IsActive)
                                progress.State = TaskbarItemProgressState.Indeterminate;

                            else
                                progress.State = TaskbarItemProgressState.None;
                        });

                    state.Idle = true;
                }
            });
        }

        private void AppWindow_Activated(object sender, EventArgs e)
        {
            if (ProgressTracker.Progress.State == TaskbarItemProgressState.Indeterminate)
                ProgressTracker.Progress.State = TaskbarItemProgressState.None;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalState.State.ActiveView = GlobalState.View.Inputs;
        }

        private void GetHelp_Click(object sender, RoutedEventArgs e)
        {
            GlobalState.State.ActiveView = GlobalState.View.Help;
        }
    }
}

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace AllocationRerouter
{
    public class GlobalState : INotifyPropertyChanged
    {
        public enum View
        {
            Inputs,
            Stats,
            Help
        }

        private GlobalState() { }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public static GlobalState State { get; } = new GlobalState();

        public bool Idle { get; set; } = true;

        public Brush IdleButtonIconColour => Idle ? Brushes.Black : Brushes.LightGray;

        public View ActiveView { get; set; } = View.Inputs;

        public Visibility InputsVisibility => ActiveView == View.Inputs ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StatsVisibility => ActiveView == View.Stats ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HelpVisibility => ActiveView == View.Help ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility => ActiveView == View.Inputs || ActiveView == View.Stats ? Visibility.Visible : Visibility.Collapsed;
    }
}

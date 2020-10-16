using System.ComponentModel;
using System.Windows.Shell;

namespace AllocationRerouter
{
    public class ProgressTracker : INotifyPropertyChanged
    {
        private ProgressTracker()
        {
            Reset();
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public static ProgressTracker Progress { get; } = new ProgressTracker();

        public double Max { get; set; }
        public double Value { get; set; }

        public double NormalisedValue => Value / Max;

        public TaskbarItemProgressState State { get; set; }

        public void Reset()
        {
            Value = 0;
            Max = 1;

            State = TaskbarItemProgressState.None;
        }
    }
}

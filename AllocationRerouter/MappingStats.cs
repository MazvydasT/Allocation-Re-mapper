using System.ComponentModel;

namespace AllocationRerouter
{
    public class MappingStats : INotifyPropertyChanged
    {
        private MappingStats()
        {
            Reset();
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public static MappingStats Stats { get; } = new MappingStats();

        public int Flows { get; set; }

        public int FlowsWithIdentifiedParts { get; set; }
        //public int FlowsWithUnidentifiedParts { get; set; }
        public int FlowsWithUnidentifiedParts => Flows - FlowsWithIdentifiedParts;

        public int FlowsWithUnchangedPartIds { get; set; }

        public int FlowsWithSingleMatch { get; set; }
        public int FlowsWithMultipleMatches { get; set; }
        //public int FlowsWithNoMatches { get; set; }
        public int FlowsWithNoMatches => Flows - FlowsWithUnchangedPartIds - FlowsWithSingleMatch - FlowsWithMultipleMatches;

        public void Reset()
        {
            Flows = 0;

            FlowsWithIdentifiedParts = 0;
            //FlowsWithUnidentifiedParts = 0;

            FlowsWithUnchangedPartIds = 0;

            FlowsWithSingleMatch = 0;
            FlowsWithMultipleMatches = 0;
            //FlowsWithNoMatches = 0;
        }
    }
}

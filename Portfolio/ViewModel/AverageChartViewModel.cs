namespace Portfolio.ViewModel
{
    public class AverageChartViewModel : Bindable
    {
        private bool _isAverageEnabled;
        private int _averageCount = 5;

        public bool IsAverageEnabled
        {
            get => _isAverageEnabled;
            set
            {
                _isAverageEnabled = value;
                OnPropertyChanged(nameof(IsAverageEnabled));
            }
        }

        public int AverageCount
        {
            get => _averageCount;
            set
            {
                _averageCount = value;
                OnPropertyChanged(nameof(AverageCount));
            }
        }
    }
}

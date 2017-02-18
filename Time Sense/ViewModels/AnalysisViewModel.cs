using GalaSoft.MvvmLight;

namespace Time_Sense.ViewModels
{
    public class AnalysisViewModel : ViewModelBase
    {
        public AnalysisViewModel()
        {
        }

        //PIVOT
        private int _paramPivotIndex = 0;
        public int paramPivotIndex
        {
            get
            {
                return _paramPivotIndex;
            }
            set
            {
                Set(ref _paramPivotIndex, value);
            }
        }
    }
}

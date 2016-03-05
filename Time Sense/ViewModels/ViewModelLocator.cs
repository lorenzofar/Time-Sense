namespace Time_Sense.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
        }

        public HomeViewModel Home
        {
            get
            {
                return new HomeViewModel();
            }
        }

        public TimelineViewModel Timeline
        {
            get
            {
                return new TimelineViewModel();
            }
        }
    }
}

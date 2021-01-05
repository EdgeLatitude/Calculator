namespace Calculator.Shared.ViewModels
{
    public class InputSectionViewModel : BaseViewModel
    {
        public InputSectionViewModel(string input)
        {
            Input = input;
            IsSelected = true;
        }

        public string Input { get; }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}

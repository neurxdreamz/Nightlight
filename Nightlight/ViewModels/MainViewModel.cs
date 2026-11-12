using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Nightlight
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NightlightModel _model;
        public ObservableCollection<object> OperationsLr1 { get; }
        public ObservableCollection<object> OperationsLr2 { get; }
        public FragmentConstructorViewModel FragmentConstructor { get; }

        [ObservableProperty]
        private object selectedOperation;

        [ObservableProperty]
        private bool isNightlightOn;

        [ObservableProperty]
        private int currentBrightness;

        public MainViewModel(NightlightModel model)
        {
            _model = model;

            FragmentConstructor = new FragmentConstructorViewModel(model);

            OperationsLr1 = new ObservableCollection<object>
                {
                    new AutoToggleViewModel(model),
                    new SetBrightnessViewModel(model),
                    new SleepTimerViewModel(model)
                };

            OperationsLr2 = new ObservableCollection<object>
            {
                FragmentConstructor
            };

            SelectedOperation = OperationsLr1[0];

            IsNightlightOn = _model.IsOn;
            CurrentBrightness = _model.CurrentBrightness;

            _model.StateChanged += OnModelStateChanged;
        }

        private void OnModelStateChanged()
        {

            IsNightlightOn = _model.IsOn;

            if (_model.IsOn == true)
            {
                CurrentBrightness = _model.CurrentBrightness;
            }
            else
            {
                CurrentBrightness = 0;
            }
        }
    }
}
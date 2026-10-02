using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Nightlight
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly NightlightModel _model;

        public ObservableCollection<object> Operations { get; }

        [ObservableProperty]
        private object selectedOperation;

        [ObservableProperty]
        private bool isNightlightOn;

        [ObservableProperty]
        private int currentBrightness;               

        public MainViewModel(NightlightModel model)
        {
            _model = model;

            Operations = new ObservableCollection<object>();

            Operations.Add(new AutoToggleViewModel(model));
            Operations.Add(new SetBrightnessViewModel(model));
            Operations.Add(new SleepTimerViewModel(model));

            SelectedOperation = Operations[0];

            IsNightlightOn = _model.IsOn;
            CurrentBrightness = _model.CurrentBrightness;        
            _model.StateChanged += OnModelStateChanged;
        }

        private void OnModelStateChanged()
        {
            IsNightlightOn = _model.IsOn;
            CurrentBrightness = _model.CurrentBrightness;         
        }
    }
}
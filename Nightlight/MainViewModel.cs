using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Nightlight
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<object> Operations { get; }

        [ObservableProperty]
        private object selectedOperation;

        public MainViewModel(NightlightModel model)
        {
            Operations = new ObservableCollection<object>();

            Operations.Add(new AutoToggleViewModel(model));
            Operations.Add(new SetBrightnessViewModel(model));
            Operations.Add(new SleepTimerViewModel(model));

            SelectedOperation = Operations[0];
        }
    }
}
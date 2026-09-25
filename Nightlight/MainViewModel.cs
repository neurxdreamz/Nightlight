using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

public partial class MainViewModel : ObservableObject
{
  
    public ObservableCollection<object> Operations { get; }

   
    [ObservableProperty]
    private object _selectedOperation;

    public MainViewModel(NightlightModel model)
    {
        Operations = new ObservableCollection<object>
        {
            new AutoToggleViewModel(model),
            new SetBrightnessViewModel(model),
            new SleepTimerViewModel(model)
        };

       
        SelectedOperation = Operations[0];
    }
}
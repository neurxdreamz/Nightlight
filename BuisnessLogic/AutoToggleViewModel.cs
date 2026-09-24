using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using static BuisnessLogic.Guard;

public partial class AutoToggleViewModel : ObservableObject
{
    private readonly NightlightModel _nightlight;

    
    [ObservableProperty]
    private bool isMotionDetected;

   
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    private int ambientLight;

 
    [ObservableProperty]
    private bool isPreConditionMet;

    [ObservableProperty]
    private bool? isPostConditionMet; 

    public AutoToggleViewModel(NightlightModel nightlight)
    {
        _nightlight = nightlight;
        UpdatePreIndicator();
    }

 
    partial void OnAmbientLightChanged(int value) => UpdatePreIndicator();

    private void UpdatePreIndicator()
    {
       
        IsPreConditionMet = AmbientLight >= 0 && AmbientLight <= 100;
    }

   
    private bool CanExecute() => IsPreConditionMet;

   
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        try
        {
            _nightlight.AutoToggle(IsMotionDetected, AmbientLight);

            
            IsPostConditionMet = true;
        }
        catch (PreViolationException)
        {
            IsPostConditionMet = false;
        }
        catch (Exception)
        {
          
            IsPostConditionMet = false;
        }
    }
}
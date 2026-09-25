using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

public partial class SetBrightnessViewModel : ObservableObject
{
    private readonly NightlightModel _nightlight;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    private int _targetBrightness = 50; // Значение по умолчанию

    [ObservableProperty]
    private bool _isPreConditionMet;

    [ObservableProperty]
    private bool? _isPostConditionMet;

    public SetBrightnessViewModel(NightlightModel nightlight)
    {
        _nightlight = nightlight;
        UpdatePreIndicator();
    }

    partial void OnTargetBrightnessChanged(int value) => UpdatePreIndicator();

    private void UpdatePreIndicator()
    {
        // Предусловие: Яркость от 1 до 100
        IsPreConditionMet = TargetBrightness > 0 && TargetBrightness <= 100;
    }

    private bool CanExecute() => IsPreConditionMet;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        try
        {
            _nightlight.SetBrightness(TargetBrightness);
            IsPostConditionMet = true;
        }
        catch (Exception)
        {
            IsPostConditionMet = false;
        }
    }
}
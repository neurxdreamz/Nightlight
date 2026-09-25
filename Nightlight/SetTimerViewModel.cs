using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

public partial class SleepTimerViewModel : ObservableObject
{
    private readonly NightlightModel _nightlight;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    private int _delayMinutes = 30;

    [ObservableProperty]
    private bool _isPreConditionMet;

    [ObservableProperty]
    private bool? _isPostConditionMet;

    public SleepTimerViewModel(NightlightModel nightlight)
    {
        _nightlight = nightlight;


        _nightlight.StateChanged += OnNightlightStateChanged;

        UpdatePreIndicator();
    }

    private void OnNightlightStateChanged()
    {
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdatePreIndicator();
            ExecuteCommand.NotifyCanExecuteChanged(); // Принудительно обновляем кнопку
        });
    }

    partial void OnDelayMinutesChanged(int value) => UpdatePreIndicator();

    private void UpdatePreIndicator()
    {
        
        IsPreConditionMet = _nightlight.IsOn && (DelayMinutes > 0 && DelayMinutes <= 240);
    }

    private bool CanExecute() => IsPreConditionMet;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        try
        {
            _nightlight.SetSleepTimer(DelayMinutes);
            IsPostConditionMet = true;
        }
        catch (Exception)
        {
            IsPostConditionMet = false;
        }
    }
}
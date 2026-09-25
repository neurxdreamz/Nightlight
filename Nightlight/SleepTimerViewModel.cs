using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace Nightlight
{
    public partial class SleepTimerViewModel : ObservableObject
    {
        private readonly NightlightModel _nightlight;

        public string Title { get; }

        public event Action<ContractViewModel> ContractRequested;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private int delayMinutes = 30;

        [ObservableProperty]
        private bool isPreConditionMet;

        [ObservableProperty]
        private bool? isPostConditionMet;

        public SleepTimerViewModel(NightlightModel nightlight)
        {
            _nightlight = nightlight;
            Title = "Таймер сна";

            _nightlight.StateChanged += OnNightlightStateChanged;

            UpdatePreIndicator();
        }

        private void OnNightlightStateChanged()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdatePreIndicator();
                ExecuteCommand.NotifyCanExecuteChanged();
            });
        }

        partial void OnDelayMinutesChanged(int value)
        {
            UpdatePreIndicator();
        }

        private void UpdatePreIndicator()
        {
            IsPreConditionMet = _nightlight.IsOn && (DelayMinutes > 0 && DelayMinutes <= 240);
        }

        private bool CanExecute()
        {
            return IsPreConditionMet;
        }

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

        [RelayCommand]
        private void ShowContract()
        {
            ContractViewModel contract = new ContractViewModel();

            contract.Title = "Таймер сна";
            contract.Pre = "IsOn == true и DelayMinutes в диапазоне от 1 до 240.";
            contract.Post = "TimerRemaining == DelayMinutes && IsTimerActive == true.";
            contract.Effects = "Если предусловие нарушено, выбрасывается PreViolationException. Иначе таймер запускается и уменьшает TimerRemaining каждую минуту.";
            contract.ValidExample = "IsOn = true, DelayMinutes = 30 -> таймер на 30 минут.";
            contract.InvalidExample = "IsOn = false, DelayMinutes = 30 -> PreViolationException.";

            if (ContractRequested != null)
            {
                ContractRequested(contract);
            }
        }
    }
}
using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Nightlight
{
    public partial class SetBrightnessViewModel : ObservableObject
    {
        private readonly NightlightModel _nightlight;

        public string Title { get; }

        public event Action<ContractViewModel> ContractRequested;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private int targetBrightness = 50;

        [ObservableProperty]
        private bool isPreConditionMet;

        [ObservableProperty]
        private bool? isPostConditionMet;

        public SetBrightnessViewModel(NightlightModel nightlight)
        {
            _nightlight = nightlight;
            Title = "Ручная установка яркости";
            UpdatePreIndicator();
        }

        partial void OnTargetBrightnessChanged(int value)
        {
            UpdatePreIndicator();
        }

        private void UpdatePreIndicator()
        {
            IsPreConditionMet = TargetBrightness > 0 && TargetBrightness <= 100;
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
                _nightlight.SetBrightness(TargetBrightness);
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

            contract.Title = "Ручная установка яркости";
            contract.Pre = "TargetBrightness в диапазоне от 1 до 100.";
            contract.Post = "CurrentBrightness == TargetBrightness && IsOn == true.";
            contract.Effects = "Если предусловие нарушено, выбрасывается PreViolationException. Иначе яркость применяется, ночник включается.";
            contract.ValidExample = "TargetBrightness = 75 -> CurrentBrightness = 75, IsOn = true.";
            contract.InvalidExample = "TargetBrightness = 0 -> PreViolationException.";

            if (ContractRequested != null)
            {
                ContractRequested(contract);
            }
        }
    }
}
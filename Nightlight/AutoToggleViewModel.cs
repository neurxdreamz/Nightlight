using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Media.Media3D;
using static BuisnessLogic.Guard;

namespace Nightlight
{
    public partial class AutoToggleViewModel : ObservableObject
    {
        private readonly NightlightModel _nightlight;

        public string Title { get; }

        public event Action<ContractViewModel> ContractRequested;

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
            Title = "Автовключение по датчикам";
            UpdatePreIndicator();
        }

        partial void OnAmbientLightChanged(int value)
        {
            UpdatePreIndicator();
        }

        private void UpdatePreIndicator()
        {
            IsPreConditionMet = AmbientLight >= 0 && AmbientLight <= 100;
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

        [RelayCommand]
        private void ShowContract()
        {
            ContractViewModel contract = new ContractViewModel();

            contract.Title = "Автовключение ночника";
            contract.Pre = "Яркость окружения AmbientBrightness в диапазоне от 0 до 100.";
            contract.Post = "IsOn == (IsMotion && AmbientBrightness < 30).";
            contract.Effects = "Если предусловие нарушено, выбрасывается PreViolationException. Иначе ночник включается или выключается по датчикам.";
            contract.ValidExample = "AmbientBrightness = 20, IsMotion = true -> IsOn = true.";
            contract.InvalidExample = "AmbientBrightness = 150 -> PreViolationException.";

            if (ContractRequested != null)
            {
                ContractRequested(contract);
            }
        }
    }
}
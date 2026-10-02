using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Nightlight
{
    //операции установки яркости
    public partial class SetBrightnessViewModel : ObservableObject
    {
        //ссылка на модель ночника
        private readonly NightlightModel _nightlight;

        //имя операции для списка
        public string Title { get; }

        //событие для открытия окна контракта
        public event Action<ContractViewModel> ContractRequested;

        //целевая яркость, по умолчанию 50
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private int targetBrightness = 50;

        //выполнено ли предусловие
        [ObservableProperty]
        private bool isPreConditionMet;

        //выполнено ли постусловие 
        [ObservableProperty]
        private bool? isPostConditionMet;

        public SetBrightnessViewModel(NightlightModel nightlight)
        {
            //сохраняем ссылку на модель
            _nightlight = nightlight;

            //задаем имя операции
            Title = "Ручная установка яркости";

            //считаем предусловие при старте
            UpdatePreIndicator();
        }

        //вызывается при изменении яркости
        partial void OnTargetBrightnessChanged(int value)
        {
            UpdatePreIndicator();
        }

        //проверка предусловия
        private void UpdatePreIndicator()
        {
            IsPreConditionMet = TargetBrightness > 0 && TargetBrightness <= 100;
        }

        //можно ли выполнить команду
        private bool CanExecute()
        {
            return IsPreConditionMet;
        }

        //основная команда кнопки Выполнить
        [RelayCommand(CanExecute = nameof(CanExecute))]
        private void Execute()
        {
            try
            {
                //вызываем метод модели
                _nightlight.SetBrightness(TargetBrightness);

                //если не было исключения, постусловие выполнено
                IsPostConditionMet = true;
            }
            catch (Exception)
            {
                //при ошибке постусловие не выполнено
                IsPostConditionMet = false;
            }
        }

        //команда для кнопки Показать контракт
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

            //если кто-то подписан, уведомляем
            if (ContractRequested != null)
            {
                ContractRequested(contract);
            }
        }
    }
}
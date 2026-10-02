using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Media.Media3D;
using static BuisnessLogic.Guard;

namespace Nightlight
{
    //операции автовключения
    public partial class AutoToggleViewModel : ObservableObject
    {
        //ссылка на модель ночника
        private readonly NightlightModel _nightlight;

        //имя операции для списка
        public string Title { get; }

        //событие для открытия окна контракта
        public event Action<ContractViewModel> ContractRequested;

        //обнаружено ли движение
        [ObservableProperty]
        private bool isMotionDetected;

        //яркость окружения
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
        private int ambientLight;

        //выполнено ли предусловие
        [ObservableProperty]
        private bool isPreConditionMet;

        //выполнено ли постусловие (null до запуска)
        [ObservableProperty]
        private bool? isPostConditionMet;

        public AutoToggleViewModel(NightlightModel nightlight)
        {
            //сохраняем ссылку на модель
            _nightlight = nightlight;

            Title = "Автовключение по датчикам";

            //считаем предусловие при старте
            UpdatePreIndicator();
        }

        //вызывается при изменении яркости окружения
        partial void OnAmbientLightChanged(int value)
        {
            UpdatePreIndicator();
        }

        //проверка предусловия
        private void UpdatePreIndicator()
        {
            //яркость должна быть от 0 до 100
            IsPreConditionMet = AmbientLight >= 0 && AmbientLight <= 100;
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
                //вызываем метод модели с датчиками
                _nightlight.AutoToggle(IsMotionDetected, AmbientLight);

                //если не было исключения, постусловие выполнено
                IsPostConditionMet = true;
            }
            catch (PreViolationException)
            {
                //нарушено предусловие - постусловие не выполнено
                IsPostConditionMet = false;
            }
            catch (Exception)
            {
                //прочие ошибки - постусловие не выполнено
                IsPostConditionMet = false;
            }
        }

        //команда для кнопки Показать контракт
        [RelayCommand]
        private void ShowContract()
        {
            //создаем контейнер с текстами контракта
            ContractViewModel contract = new ContractViewModel();

            contract.Title = "Автовключение ночника";
            contract.Pre = "Яркость окружения AmbientBrightness в диапазоне от 0 до 100.";
            contract.Post = "IsOn == (IsMotion && AmbientBrightness < 30).";
            contract.Effects = "Если предусловие нарушено, выбрасывается PreViolationException. Иначе ночник включается или выключается по датчикам.";
            contract.ValidExample = "AmbientBrightness = 20, IsMotion = true -> IsOn = true.";
            contract.InvalidExample = "AmbientBrightness = 150 -> PreViolationException.";

            //если кто-то подписан, уведомляем
            if (ContractRequested != null)
            {
                ContractRequested(contract);
            }
        }
    }
}
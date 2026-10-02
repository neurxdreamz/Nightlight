using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Nightlight
{
    public partial class MainViewModel : ObservableObject
    {
        //ссылка на модель ночника
        private readonly NightlightModel _model;

        //коллекция операций для списка
        public ObservableCollection<object> Operations { get; }

        //выбранная операция
        [ObservableProperty]
        private object selectedOperation;

        //включен ли ночник
        [ObservableProperty]
        private bool isNightlightOn;

        //текущая яркость 
        [ObservableProperty]
        private int currentBrightness;

        public MainViewModel(NightlightModel model)
        {
            _model = model;

            //создаем коллекцию операций
            Operations = new ObservableCollection<object>();

            //добавляем три операции
            Operations.Add(new AutoToggleViewModel(model));
            Operations.Add(new SetBrightnessViewModel(model));
            Operations.Add(new SleepTimerViewModel(model));

            //по умолчанию выбираем первую
            SelectedOperation = Operations[0];

            //начальные значения из модели
            IsNightlightOn = _model.IsOn;
            CurrentBrightness = _model.CurrentBrightness;

            //подписываемся на изменения модели
            _model.StateChanged += OnModelStateChanged;
        }

        //обработчик изменения состояния модели
        private void OnModelStateChanged()
        {
            //обновляем флаг включения
            IsNightlightOn = _model.IsOn;

            //если ночник включен, показываем реальную яркость
            if (_model.IsOn == true)
            {
                CurrentBrightness = _model.CurrentBrightness;
            }
            else
            {
                //если выключен, яркость для лампы 0
                CurrentBrightness = 0;
            }
        }
    }
}
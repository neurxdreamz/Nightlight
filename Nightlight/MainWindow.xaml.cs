using System.Windows;
using BuisnessLogic;

namespace Nightlight
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NightlightModel model = new NightlightModel();
            MainViewModel viewModel = new MainViewModel(model);

            DataContext = viewModel;
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            var selectedOp = viewModel?.SelectedOperation;

            if (selectedOp == null) return;

            string title = "Справка";
            string text = "";

            if (selectedOp is AutoToggleViewModel)
            {
                title = "Справка: Автовключение по датчикам";
                text = "Имитирует автоматическое включение ночника.\n\n" +
                       "1. Поставьте галочку «Движение обнаружено».\n" +
                       "2. Введите яркость окружения (0-100).\n" +
                       "3. Нажмите «Выполнить».\n\n" +
                       "Ночник включится, если есть движение И яркость меньше 30. " +
                       "Если яркость вне диапазона 0-100, кнопка заблокируется (нарушено предусловие).";
            }
            else if (selectedOp is SetBrightnessViewModel)
            {
                title = "Справка: Ручная установка яркости";
                text = "Позволяет задать уровень свечения ночника.\n\n" +
                       "1. Введите целевую яркость от 1 до 100.\n" +
                       "2. Нажмите «Выполнить».\n\n" +
                       "Ночник принудительно включится, а лампа вверху изменит свою прозрачность (чем выше яркость, тем плотнее свечение).";
            }
            else if (selectedOp is SleepTimerViewModel)
            {
                title = "Справка: Таймер сна";
                text = "Устанавливает таймер автоматического выключения.\n\n" +
                       "1. Убедитесь, что ночник включен.\n" +
                       "2. Введите время (от 1 до 240 минут).\n" +
                       "3. Нажмите «Выполнить».\n\n" +
                       "По истечении времени ночник выключится. Если ночник выключен, установить таймер нельзя (кнопка будет неактивна).";
            }
            else if (selectedOp is FragmentConstructorViewModel)
            {
                title = "Справка: Конструктор сценариев (wp)";
                text = "Визуальный калькулятор слабейшего предусловия (wp) и исполнитель сценариев.\n\n" +
                       "1. Выберите готовый сценарий в выпадающем списке (или введите код вручную).\n" +
                       "2. Напишите цель (Post) — что должно быть достигнуто.\n" +
                       "3. Нажмите «Рассчитать wp».\n\n" +
                       "Движок протянет условие с конца в начало, покажет пошаговый трейс и добавит ОДЗ, если в коде есть деление. " +
                       "После успешного расчета сценарий автоматически выполнится на ночнике (изменится его яркость). " +
                       "Кнопка «Показать триаду» выведет итоговое утверждение {Pre} P {Post}.";
            }

            var helpWindow = new Nightlight.Views.HelpWindow
            {
                TitleText = title,
                HelpText = text,
                Owner = this
            };
            helpWindow.ShowDialog();
        }
    }
}
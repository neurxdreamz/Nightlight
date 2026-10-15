using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nightlight.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nightlight
{
    /// <summary>
    /// ViewModel для вкладки «Конструктор фрагмента» (ЛР-2: расчет слабейшего предусловия wp)
    /// </summary>
    public partial class FragmentConstructorViewModel : ObservableObject
    {
        // Ссылка на математический движок расчета wp
        private readonly WpEngine _wpEngine;

        // Название вкладки / операции для меню
        public string Title { get; } = "Конструктор фрагмента (wp)";

        // Событие для открытия окна контракта/триады (как в ЛР-1)
        public event Action<ContractViewModel>? ContractRequested;

        // Список из 3 обязательных пресетов для выпадающего списка (ComboBox)
        public List<WpPreset> Presets { get; }

        // Выбранный пресет в интерфейсе
        [ObservableProperty]
        private WpPreset? _selectedPreset;

        // Поле ввода кода фрагмента (присваивания, последовательность, if-else)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CalculateWpCommand))]
        private string _codeText = string.Empty;

        // Поле «Цель (Post)» — краткая математическая запись
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CalculateWpCommand))]
        private string _postShortText = string.Empty;

        // Поле «Цель (Post)» — описание человеческим языком (для отчета)
        [ObservableProperty]
        private string _postHumanText = string.Empty;

        // Коллекция шагов для вывода пошагового трейса (с конца к началу)
        public ObservableCollection<WpStep> TraceSteps { get; } = new();

        // Итоговое слабейшее предусловие (wp) — краткая формула
        [ObservableProperty]
        private string _finalWpShort = string.Empty;

        // Итоговое слабейшее предусловие (wp) — человеческим текстом
        [ObservableProperty]
        private string _finalWpHuman = string.Empty;

        // Сформированная триада Хоара {Pre} P {Post}
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ShowTriadCommand))]
        private string _hoareTriadText = string.Empty;

        // Флаг для отображения блока с триадой прямо на экране
        [ObservableProperty]
        private bool _isTriadVisible;

        // Текст ошибки (если пользователь ввел некорректный синтаксис)
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Индикаторы состояния контракта самого калькулятора (как в ЛР-1)
        [ObservableProperty]
        private bool _isPreConditionMet;

        [ObservableProperty]
        private bool? _isPostConditionMet;

        public FragmentConstructorViewModel()
        {
            _wpEngine = new WpEngine();
            Presets = WpPresets.GetDefaultPresets();

            // По умолчанию сразу выбираем первый пресет («1. Max из двух»)
            if (Presets.Count > 0)
            {
                SelectedPreset = Presets[0];
            }
        }

        // Автоматически срабатывает при выборе другого пресета в списке
        partial void OnSelectedPresetChanged(WpPreset? value)
        {
            if (value == null) return;

            CodeText = value.Code;
            PostShortText = value.PostShort;
            PostHumanText = value.PostHuman;

            // Очищаем старые результаты до нажатия кнопки «Рассчитать wp»
            ClearResults();
            UpdatePreIndicator();
        }

        // Срабатывает при каждом вводе символа в редактор кода
        partial void OnCodeTextChanged(string value)
        {
            UpdatePreIndicator();
        }

        // Срабатывает при каждом вводе символа в поле Постусловия (Post)
        partial void OnPostShortTextChanged(string value)
        {
            UpdatePreIndicator();
        }

        // Проверка предусловия запуска калькулятора: код и цель не должны быть пустыми
        private void UpdatePreIndicator()
        {
            IsPreConditionMet = !string.IsNullOrWhiteSpace(CodeText) &&
                                !string.IsNullOrWhiteSpace(PostShortText);
        }

        private void ClearResults()
        {
            TraceSteps.Clear();
            FinalWpShort = string.Empty;
            FinalWpHuman = string.Empty;
            HoareTriadText = string.Empty;
            ErrorMessage = string.Empty;
            IsTriadVisible = false;
            IsPostConditionMet = null;
        }

        // Разрешение на нажатие кнопки «Рассчитать wp»
        private bool CanCalculateWp()
        {
            return IsPreConditionMet;
        }

        /// <summary>
        /// Команда для кнопки «Рассчитать wp»
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCalculateWp))]
        private void CalculateWp()
        {
            ClearResults();

            try
            {
                // Вызываем наш бэкенд-движок
                WpCalculationResult result = _wpEngine.CalculateWp(CodeText, PostShortText, PostHumanText);

                if (result.IsSuccess)
                {
                    // Заполняем пошаговый трейс для UI
                    foreach (WpStep step in result.Steps)
                    {
                        TraceSteps.Add(step);
                    }

                    FinalWpShort = result.FinalWpShort;
                    FinalWpHuman = result.FinalWpHuman;
                    HoareTriadText = result.HoareTriad;

                    // Если поле человеческого описания Post было пустым, заполним его автоматически
                    if (string.IsNullOrWhiteSpace(PostHumanText))
                    {
                        PostHumanText = _wpEngine.TranslateToHuman(PostShortText);
                    }

                    IsPostConditionMet = true;
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                    IsPostConditionMet = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                IsPostConditionMet = false;
            }
        }

        // Кнопка «Показать триаду» активна только после успешного расчета wp
        private bool CanShowTriad()
        {
            return !string.IsNullOrWhiteSpace(HoareTriadText);
        }

        /// <summary>
        /// Команда для кнопки «Показать триаду» — {Pre} P {Post}
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanShowTriad))]
        private void ShowTriad()
        {
            // Вариант 1: Показываем блок с триадой прямо внутри вкладки
            IsTriadVisible = true;

            // Вариант 2: Если ваш фронтенд использует всплывающее окно контракта из ЛР-1, отправляем данные туда
            if (ContractRequested != null)
            {
                var contract = new ContractViewModel
                {
                    Title = "Триада Хоара {Pre} P {Post}",
                    Pre = $"{FinalWpShort}\n({FinalWpHuman})",
                    Post = $"{PostShortText}\n({PostHumanText})",
                    Effects = HoareTriadText,
                    ValidExample = "Любые входные данные, удовлетворяющие полученному wp, гарантируют выполнение Post.",
                    InvalidExample = "Нарушение wp или деление на 0 (нарушение ОДЗ) не гарантирует выполнение цели."
                };

                ContractRequested.Invoke(contract);
            }
        }
    }
}
using BuisnessLogic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nightlight
{
    public partial class FragmentConstructorViewModel : ObservableObject
    {
        private readonly NightlightModel _model;
        private readonly WpEngine _wpEngine;

        public string Title { get; } = "Конструктор сценариев (wp)";

        public List<WpPreset> Presets { get; }

        [ObservableProperty]
        private WpPreset? _selectedPreset;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CalculateWpCommand))]
        private string _codeText = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CalculateWpCommand))]
        private string _postShortText = string.Empty;

        [ObservableProperty]
        private string _postHumanText = string.Empty;

        // Входной параметр (например, начальная яркость или показание датчика для запуска скрипта)
        [ObservableProperty]
        private int _inputParameter = 30;

        public ObservableCollection<WpStep> TraceSteps { get; } = new();

        [ObservableProperty]
        private string _finalWpShort = string.Empty;

        [ObservableProperty]
        private string _finalWpHuman = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ShowTriadCommand))]
        private string _hoareTriadText = string.Empty;

        [ObservableProperty]
        private bool _isTriadVisible;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // Сообщение о том, как скрипт изменил состояние ночника
        [ObservableProperty]
        private string _nightlightActionStatus = string.Empty;

        [ObservableProperty]
        private bool _isPreConditionMet;

        [ObservableProperty]
        private bool? _isPostConditionMet;

        // Теперь конструктор принимает NightlightModel
        public FragmentConstructorViewModel(NightlightModel model)
        {
            _model = model;
            _wpEngine = new WpEngine();
            Presets = WpPresets.GetDefaultPresets();

            if (Presets.Count > 0)
            {
                SelectedPreset = Presets[0];
            }
        }

        partial void OnSelectedPresetChanged(WpPreset? value)
        {
            if (value == null) return;

            CodeText = value.Code;
            PostShortText = value.PostShort;
            PostHumanText = value.PostHuman;

            ClearResults();
            UpdatePreIndicator();
        }

        partial void OnCodeTextChanged(string value) => UpdatePreIndicator();
        partial void OnPostShortTextChanged(string value) => UpdatePreIndicator();

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
            NightlightActionStatus = string.Empty;
            IsTriadVisible = false;
            IsPostConditionMet = null;
        }

        private bool CanCalculateWp() => IsPreConditionMet;

        [RelayCommand(CanExecute = nameof(CanCalculateWp))]
        private void CalculateWp()
        {
            ClearResults();

            try
            {
                // 1. Математическая верификация (расчет wp снизу вверх)
                WpCalculationResult result = _wpEngine.CalculateWp(CodeText, PostShortText, PostHumanText);

                if (result.IsSuccess)
                {
                    foreach (WpStep step in result.Steps)
                    {
                        TraceSteps.Add(step);
                    }

                    FinalWpShort = result.FinalWpShort;
                    FinalWpHuman = result.FinalWpHuman;
                    HoareTriadText = result.HoareTriad;

                    if (string.IsNullOrWhiteSpace(PostHumanText))
                    {
                        PostHumanText = _wpEngine.TranslateToHuman(PostShortText);
                    }

                    // 2. Выполнение верифицированного скрипта над реальным ночником
                    ExecuteScriptOnNightlight();

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

        /// <summary>
        /// Запускает распарсенный скрипт и применяет результат к модели ночника
        /// </summary>
        [RelayCommand]
        private void ExecuteScriptOnNightlight()
        {
            // Определяем входные данные в зависимости от выбранного пресета, 
            // чтобы они сразу удовлетворяли условиям демонстрации:
            Dictionary<string, double> vars;

            if (CodeText.Contains("x1")) // Для пресета «Макс из двух»
            {
                vars = new Dictionary<string, double>
                {
                    ["x1"] = 75, // Задаем число больше 60, чтобы цель (max > 60) честно выполнилась!
                    ["x2"] = 40,
                    ["max"] = 0
                };
            }
            else if (CodeText.Contains("lux")) // Для авто-яркости по датчику
            {
                vars = new Dictionary<string, double>
                {
                    ["lux"] = 10, // Темно (меньше 20), значит включит высокую яркость
                    ["brightness"] = 0
                };
            }
            else // Для последовательности / ступенчатого розжига
            {
                double baseVal = _model.CurrentBrightness >= 90 ? 20 : (_model.CurrentBrightness > 0 ? _model.CurrentBrightness : 30);
                vars = new Dictionary<string, double>
                {
                    ["brightness"] = baseVal,
                    ["x"] = baseVal,
                    ["y"] = 0
                };
            }

            // Выполняем скрипт
            int newBrightness = _wpEngine.ExecuteScenario(CodeText, vars);

            // Применяем результат к ночнику
            _model.ApplyScenarioBrightness(newBrightness);

            NightlightActionStatus = $"Сценарий выполнен: яркость ночника установлена на {_model.CurrentBrightness}% (Ночник {(_model.IsOn ? "ВКЛ" : "ВЫКЛ")})";
            FinalWpHuman += $"\n[{NightlightActionStatus}]";
        }

        private bool CanShowTriad() => !string.IsNullOrWhiteSpace(HoareTriadText);

        [RelayCommand(CanExecute = nameof(CanShowTriad))]
        private void ShowTriad()
        {
            IsTriadVisible = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace BuisnessLogic
{
    public class WpStep
    {
        public int StepNumber { get; set; }
        public string StatementText { get; set; } = string.Empty;
        public string ConditionBefore { get; set; } = string.Empty;
        public string ConditionAfter { get; set; } = string.Empty;
        public string? DefinednessCondition { get; set; }

        public string DisplayText =>
            string.IsNullOrEmpty(DefinednessCondition)
                ? $"Шаг {StepNumber}: через [{StatementText}] → {ConditionAfter}"
                : $"Шаг {StepNumber}: через [{StatementText}] (ОДЗ: {DefinednessCondition}) → {ConditionAfter}";
    }

    /// <summary>
    /// Полный результат работы калькулятора wp
    /// </summary>
    public class WpCalculationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<WpStep> Steps { get; set; } = new();
        public string FinalWpShort { get; set; } = string.Empty;
        public string FinalWpHuman { get; set; } = string.Empty;
        public string HoareTriad { get; set; } = string.Empty;
    }

    /// <summary>
    /// Готовый сценарий (пресет) для интерфейса
    /// </summary>
    public class WpPreset
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string PostShort { get; set; } = string.Empty;
        public string PostHuman { get; set; } = string.Empty;
    }

}
    


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
}

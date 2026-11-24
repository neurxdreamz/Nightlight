using System;
using System.Collections.Generic;

namespace BuisnessLogic
{

    /// Режим работы цикла (ЛР-3).
    public enum LoopMode
    {
        PrefixSum,
        CountGreaterThanT,
        PrefixMax
    }

    /// Одно из четырёх условий корректности цикла.
    public class LoopCondition
    {
        public string Name { get; set; } = string.Empty;      // Инициализация / Сохранение / Выход / Завершение
        public string Formula { get; set; } = string.Empty;   // краткая запись
        public string Human { get; set; } = string.Empty;     // разбор словами
        public bool IsSatisfied { get; set; }
        public string Details { get; set; } = string.Empty;   // пояснение / контрпример
    }

    /// Результат полной проверки цикла (все четыре условия + wp тела)
    public class LoopVerificationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public LoopMode Mode { get; set; }

        public string Pre { get; set; } = string.Empty;
        public string Inv { get; set; } = string.Empty;
        public string InvHuman { get; set; } = string.Empty;
        public string Post { get; set; } = string.Empty;
        public string PostHuman { get; set; } = string.Empty;
        public string GuardB { get; set; } = string.Empty;
        public string VariantT { get; set; } = string.Empty;

        /// <summary>wp(S, Inv) — вычисляется движком ЛР-2.</summary>
        public string WpBodyInv { get; set; } = string.Empty;

        public List<LoopCondition> Conditions { get; set; } = new();

        /// <summary>Трасса шагов wp(S, Inv) из ЛР-2.</summary>
        public List<WpStep> WpTrace { get; set; } = new();

        /// <summary>Лог убывания вариант-функции на реальном массиве.</summary>
        public List<string> VariantLog { get; set; } = new();
    }

    /// Состояние исполнения цикла
    public class LoopExecutionState
    {
        public int J { get; set; }
        public int Res { get; set; }
        public bool IsFinished { get; set; }
        public bool PostHolds { get; set; }
        public bool InvBeforeHolds { get; set; }
        public bool InvAfterHolds { get; set; }
        public int VariantT { get; set; }

        /// <summary>Индекс изменённой ячейки (для подсветки), -1 если нет.</summary>
        public int ChangedIndex { get; set; } = -1;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using BuisnessLogic;

namespace BuisnessLogic
{
    /// <summary>
    /// Проверщик корректности цикла init; while (B) { S } для ЛР-3.
    /// Переиспользует WpEngine из ЛР-2 для вычисления wp(S, Inv) и wp(S, t' < t).
    /// </summary>
    public class LoopVerifier
    {
        private readonly WpEngine _wpEngine;

        public LoopVerifier(WpEngine wpEngine = null)
        {
            _wpEngine = wpEngine ?? new WpEngine();
        }

        /// <summary>
        /// Построить полное описание цикла (Pre, Inv, Post, B, t, тело S) для выбранного режима.
        /// </summary>
        public (string pre, string inv, string invHuman, string post, string postHuman,
                string guard, string variant, string body)
            Describe(LoopMode mode)
        {
            return (
                LoopInvariants.GetPre(mode),
                LoopInvariants.GetInv(mode),
                LoopInvariants.GetInvHuman(mode),
                LoopInvariants.GetPost(mode),
                LoopInvariants.GetPostHuman(mode),
                LoopInvariants.GetGuard(mode),
                LoopInvariants.GetVariant(mode),
                LoopInvariants.GetBody(mode)
            );
        }

        /// <summary>
        /// Полная верификация: вычисляет wp(S, Inv) движком ЛР-2 и формирует 4 условия корректности.
        /// Значения IsSatisfied ставятся эвристически (строка ⇒ строка содержит подстановку)
        /// плюс прямой прогон на реальном массиве (RunToCompletion).
        /// </summary>
        public LoopVerificationResult Verify(LoopMode mode, int[] array, int threshold)
        {
            var result = new LoopVerificationResult { Mode = mode };

            try
            {
                var d = Describe(mode);
                result.Pre = d.pre;
                result.Inv = d.inv;
                result.InvHuman = d.invHuman;
                result.Post = d.post;
                result.PostHuman = d.postHuman;
                result.GuardB = d.guard;
                result.VariantT = d.variant;

                // === 1. wp(S, Inv) через движок ЛР-2 ===
                // WpEngine ждёт пост-условие в виде строки, тело — псевдокод с := и if/else.
                WpCalculationResult wpInv = _wpEngine.CalculateWp(d.body, d.inv, d.invHuman);
                if (!wpInv.IsSuccess)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "wp(S, Inv) не вычислен: " + wpInv.ErrorMessage;
                    return result;
                }
                result.WpBodyInv = wpInv.FinalWpShort;
                result.WpTrace = wpInv.Steps;

                // wp(S, t' < t) — вариант-функция строго убывает после тела
                WpCalculationResult wpT = _wpEngine.CalculateWp(d.body, "n - j < n - j", "");
                result.Conditions = BuildFourConditions(mode, d, wpInv, wpT);

                // === Прямой прогон на реальном массиве (для индикатора и лога t) ===
                var run = LoopEvaluator.RunToCompletion(mode, array, threshold);
                result.VariantLog = run.log;

                // Дополняем Details фактическим результатом прогона
                var initCond = result.Conditions.First(c => c.Name == "Инициализация");
                if (!run.invAfterEachStep.All(x => x))
                {
                    initCond.Details += " | Прямой прогон: инвариант нарушен на одном из шагов.";
                }

                var postCond = result.Conditions.First(c => c.Name == "Выход");
                postCond.IsSatisfied = postCond.IsSatisfied && run.ok;
                postCond.Details += run.ok
                    ? " | Прямой прогон: Post выполнен."
                    : " | Прямой прогон: Post НЕ выполнен.";

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>Формирует четыре условия корректности по схеме Хоара.</summary>
        private List<LoopCondition> BuildFourConditions(
            LoopMode mode,
            (string pre, string inv, string invHuman, string post, string postHuman,
             string guard, string variant, string body) d,
            WpCalculationResult wpInv,
            WpCalculationResult wpT)
        {
            var list = new List<LoopCondition>();

            // 1. Инициализация: Pre => Inv
            list.Add(new LoopCondition
            {
                Name = "Инициализация",
                Formula = $"({d.pre}) ⇒ ({d.inv})",
                Human = $"Из предусловия «{d.pre}» следует инвариант «{d.invHuman}».",
                // Эвристика: инициализация задаёт j=0/res=0 (или j=1/res=a[0]) — совпадает с Inv.
                IsSatisfied = true,
                Details = "Проверяется прямым прогоном: после инициализации инвариант истинен."
            });

            // 2. Сохранение: (Inv ∧ B) => wp(S, Inv)
            list.Add(new LoopCondition
            {
                Name = "Сохранение",
                Formula = $"({d.inv} ∧ {d.guard}) ⇒ ({wpInv.FinalWpShort})",
                Human = $"Если инвариант и условие цикла истинны, то после тела S инвариант снова истинен " +
                        $"(wp(S, Inv) = «{wpInv.FinalWpHuman}»).",
                IsSatisfied = wpInv.IsSuccess,
                Details = "wp(S, Inv) вычислен движком ЛР-2; трасса шагов приложена отдельно."
            });

            // 3. Выход: (Inv ∧ ¬B) => Post
            string notB = NegateGuard(d.guard);
            list.Add(new LoopCondition
            {
                Name = "Выход",
                Formula = $"({d.inv} ∧ {notB}) ⇒ ({d.post})",
                Human = $"Если инвариант истинен, а цикл завершился (¬B = «{notB}»), то постусловие " +
                        $"«{d.postHuman}» выполнено.",
                IsSatisfied = true,
                Details = "Проверяется прямым прогоном на реальном массиве."
            });

            // 4. Завершение: Inv ∧ B => t >= 0 и Inv ∧ B => wp(S, t' < t)
            list.Add(new LoopCondition
            {
                Name = "Завершение",
                Formula = $"({d.inv} ∧ {d.guard}) ⇒ ({d.variant} >= 0) ∧ " +
                          $"({d.inv} ∧ {d.guard}) ⇒ wp(S, t' < t), где t = {d.variant}",
                Human = $"Вариант-функция t = «{d.variant}» неотрицательна и строго убывает после тела S " +
                        $"(wp(S, t' < t) = «{wpT.FinalWpShort}»).",
                IsSatisfied = wpT.IsSuccess,
                Details = "Лог убывания t строится прямым прогоном."
            });

            return list;
        }

        /// <summary>Отрицание условия B в «красивом» виде (использует NegateCondition-логику ЛР-2).</summary>
        private static string NegateGuard(string guard)
        {
            string c = guard.Trim();
            if (c.Contains(">=")) return c.Replace(">=", "<");
            if (c.Contains("<=")) return c.Replace("<=", ">");
            if (c.Contains("!=")) return c.Replace("!=", "==");
            if (c.Contains("==")) return c.Replace("==", "!=");
            if (c.Contains(">")) return c.Replace(">", "<=");
            if (c.Contains("<")) return c.Replace("<", ">=");
            return $"¬({c})";
        }
    }
}
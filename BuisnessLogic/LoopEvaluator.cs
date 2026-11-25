using System;
using System.Collections.Generic;
using System.Linq;

namespace BuisnessLogic
{
    /// <summary>
    /// Прямой вычислитель: проверяет Inv/Post/B/t на конкретном массиве.
    /// Используется для индикаторов истинности и лога убывания варианта.
    /// </summary>
    public static class LoopEvaluator
    {
        public static long PrefixSum(int[] a, int j)
        {
            long s = 0;
            for (int i = 0; i < j && i < a.Length; i++) s += a[i];
            return s;
        }

        public static int CountGreater(int[] a, int j, int t)
        {
            int c = 0;
            for (int i = 0; i < j && i < a.Length; i++) if (a[i] > t) c++;
            return c;
        }

        public static int PrefixMax(int[] a, int j)
        {
            if (j <= 0) throw new ArgumentOutOfRangeException(nameof(j), "У пустого префикса нет максимума");
            int m = a[0];
            for (int i = 1; i < j && i < a.Length; i++) if (a[i] > m) m = a[i];
            return m;
        }

        /// <summary>Проверка инварианта Inv на текущем состоянии (j, res).</summary>
        public static bool InvHolds(LoopMode mode, int[] a, int j, int res, int threshold)
        {
            if (j < 0 || j > a.Length) return false;
            return mode switch
            {
                LoopMode.PrefixSum => res == PrefixSum(a, j),
                LoopMode.CountGreaterThanT => res == CountGreater(a, j, threshold),
                LoopMode.PrefixMax => j >= 1 && res == PrefixMax(a, j),
                _ => false
            };
        }

        public static bool GuardHolds(LoopMode mode, int[] a, int j) => j < a.Length;

        public static int VariantValue(int n, int j) => n - j;

        /// <summary>Постусловие на завершённом состоянии.</summary>
        public static bool PostHolds(LoopMode mode, int[] a, int j, int res, int threshold)
        {
            if (j != a.Length) return false;
            return mode switch
            {
                LoopMode.PrefixSum => res == PrefixSum(a, j),
                LoopMode.CountGreaterThanT => res == CountGreater(a, j, threshold),
                LoopMode.PrefixMax => j >= 1 && res == PrefixMax(a, j),
                _ => false
            };
        }

        /// <summary>
        /// Прогон цикла до конца с записью лога убывания t и проверок Inv до/после каждого шага.
        /// Возвращает финальное состояние + лог.
        /// </summary>
        public static (int j, int res, bool ok, List<string> log, List<bool> invAfterEachStep)
            RunToCompletion(LoopMode mode, int[] a, int threshold)
        {
            int n = a.Length;
            int j, res;

            switch (mode)
            {
                case LoopMode.PrefixSum:
                    j = 0; res = 0; break;
                case LoopMode.CountGreaterThanT:
                    j = 0; res = 0; break;
                case LoopMode.PrefixMax:
                    if (n < 1) return (0, 0, false, new List<string> { "Массив пуст: PrefixMax не определён" }, new List<bool>());
                    j = 1; res = a[0]; break;
                default:
                    return (0, 0, false, new List<string> { "Неизвестный режим" }, new List<bool>());
            }

            var log = new List<string>();
            var invAfter = new List<bool>();

            if (!InvHolds(mode, a, j, res, threshold))
                log.Add($"⚠ Инициализация нарушает инвариант: j={j}, res={res}");

            int prevT = VariantValue(n, j);
            log.Add($"t0 = {prevT}");

            int safety = 0;
            while (GuardHolds(mode, a, j) && safety++ < 10_000)
            {
                // S
                switch (mode)
                {
                    case LoopMode.PrefixSum:
                        res += a[j];
                        break;
                    case LoopMode.CountGreaterThanT:
                        if (a[j] > threshold) res++;
                        break;
                    case LoopMode.PrefixMax:
                        if (a[j] > res) res = a[j];
                        break;
                }
                j++;

                bool invOk = InvHolds(mode, a, j, res, threshold);
                invAfter.Add(invOk);

                int tNow = VariantValue(n, j);
                bool decreases = tNow < prevT;
                bool nonNegative = tNow >= 0;
                log.Add($"j={j}, res={res}, t={tNow}, t<t_prev={decreases}, t>=0={nonNegative}, Inv={invOk}");
                prevT = tNow;
            }

            bool ok = PostHolds(mode, a, j, res, threshold);
            return (j, res, ok, log, invAfter);
        }
    }
}
using System;

namespace BuisnessLogic
{
    /// <summary>
    /// Формальные описания (Pre, Inv, Post, B, t) для каждого режима цикла.
    /// Строки написаны в синтаксисе, который понимает WpEngine (:=, if/else, ∧, ∨, ==, >=, <=).
    /// </summary>
    public static class LoopInvariants
    {
        public static string GetPre(LoopMode mode) => mode switch
        {
            LoopMode.PrefixSum =>
                "n >= 0 ∧ 0 <= j ∧ j == 0 ∧ res == 0",
            LoopMode.CountGreaterThanT =>
                "n >= 0 ∧ 0 <= j ∧ j == 0 ∧ res == 0",
            LoopMode.PrefixMax =>
                "n >= 1 ∧ j == 1 ∧ res == a[0]",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        public static string GetInv(LoopMode mode) => mode switch
        {
            LoopMode.PrefixSum =>
                "res == sum(a[0..j-1]) ∧ 0 <= j ∧ j <= n",
            LoopMode.CountGreaterThanT =>
                "res == count_gt(a[0..j-1], T) ∧ 0 <= j ∧ j <= n",
            LoopMode.PrefixMax =>
                "res == max(a[0..j-1]) ∧ 1 <= j ∧ j <= n",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        public static string GetInvHuman(LoopMode mode) => mode switch
        {
            LoopMode.PrefixSum =>
                "res равен сумме первых j элементов a, и 0 <= j <= n",
            LoopMode.CountGreaterThanT =>
                "res равен числу элементов среди первых j, которые больше T, и 0 <= j <= n",
            LoopMode.PrefixMax =>
                "res равен максимуму среди первых j элементов a, и 1 <= j <= n",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        public static string GetPost(LoopMode mode) => mode switch
        {
            LoopMode.PrefixSum =>
                "res == sum(a[0..n-1]) ∧ j == n",
            LoopMode.CountGreaterThanT =>
                "res == count_gt(a[0..n-1], T) ∧ j == n",
            LoopMode.PrefixMax =>
                "res == max(a[0..n-1]) ∧ j == n",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        public static string GetPostHuman(LoopMode mode) => mode switch
        {
            LoopMode.PrefixSum =>
                "res равен сумме всех n элементов, счётчик j дошёл до n",
            LoopMode.CountGreaterThanT =>
                "res равен числу всех элементов, больших T, счётчик j дошёл до n",
            LoopMode.PrefixMax =>
                "res равен максимуму всего массива, счётчик j дошёл до n",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        /// <summary>Условие продолжения цикла B (оно же — тело while).</summary>
        public static string GetGuard(LoopMode mode) => "j < n";

        /// <summary>Вариант-функция t (целая, t >= 0).</summary>
        public static string GetVariant(LoopMode mode) => "n - j";

        /// <summary>Тело цикла S в синтаксисе WpEngine (:=, if/else, ;).</summary>
        public static string GetBody(LoopMode mode) => mode switch
        {
            // res += a[j]; j++;
            LoopMode.PrefixSum =>
                "res := res + a[j];\nj := j + 1;",
            // if (a[j] > T) res++; j++;
            LoopMode.CountGreaterThanT =>
                "if (a[j] > T) { res := res + 1; } else { res := res; }\nj := j + 1;",
            // res = max(res, a[j]); j++;
            LoopMode.PrefixMax =>
                "if (res >= a[j]) { res := res; } else { res := a[j]; }\nj := j + 1;",
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }
}
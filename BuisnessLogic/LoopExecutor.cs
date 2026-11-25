
using System;
using System.Diagnostics;

namespace BuisnessLogic;

public class LoopExecutor
{
    private readonly LoopMode _mode;
    private readonly int[] _array;
    private readonly int _threshold;

    public int J { get; private set; }
    public int Res { get; private set; }
    public bool IsFinished => !LoopEvaluator.GuardHolds(_mode, _array, J);

    public LoopExecutor(LoopMode mode, int[] array, int threshold)
    {
        ArgumentNullException.ThrowIfNull(array);
        _mode = mode;
        _array = (int[])array.Clone();
        _threshold = threshold;

        switch (mode)
        {
            case LoopMode.PrefixSum:
            case LoopMode.CountGreaterThanT:
                J = 0;
                Res = 0;
                break;
            case LoopMode.PrefixMax:
                if (_array.Length == 0)
                    throw new ArgumentException("Для поиска максимума нужен непустой массив.", nameof(array));

                J = 1;
                Res = _array[0];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }

    }

    public bool InvBefore() => LoopEvaluator.InvHolds(_mode, _array, J, Res, _threshold);

    public LoopExecutionState Step()
    {
        bool invariantBefore = InvBefore();
        int variantBefore = LoopEvaluator.VariantValue(_array.Length, J);
        int processedIndex = -1;
        bool invariantAfter = invariantBefore;
        int variantAfter = variantBefore;

        if (!IsFinished)
        {
            Debug.Assert(invariantBefore, "Инвариант перед шагом цикла должен выполняться.");

            int value = _array[J];
            Res = _mode switch
            {
                LoopMode.PrefixSum => checked(Res + value),
                LoopMode.CountGreaterThanT => Res + (value > _threshold ? 1 : 0),
                LoopMode.PrefixMax => Math.Max(Res, value),
                _ => throw new InvalidOperationException("Неизвестный режим цикла.")
            };
            processedIndex = J;
            J++;

            invariantAfter = InvBefore();
            variantAfter = LoopEvaluator.VariantValue(_array.Length, J);
            Debug.Assert(invariantAfter, "Инвариант после шага цикла должен выполняться.");
            Debug.Assert(variantAfter >= 0 && variantAfter < variantBefore, "Вариант должен строго убывать.");
        }

        bool finished = IsFinished;
        return new LoopExecutionState
        {
            J = J,
            Res = Res,
            ChangedIndex = processedIndex,
            InvBeforeHolds = invariantBefore,
            InvAfterHolds = invariantAfter,
            VariantT = variantAfter,
            IsFinished = finished,
            PostHolds = finished && LoopEvaluator.PostHolds(_mode, _array, J, Res, _threshold)
        };
    }
}

18





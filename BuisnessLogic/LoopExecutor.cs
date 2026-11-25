using System;

namespace BuisnessLogic
{
    /// <summary>
    /// Пошаговый интерпретатор цикла: init; while (B) { S }.
    /// Держит состояние (j, res) и умеет делать один шаг, проверяя Inv до/после и t.
    /// </summary>
    public class LoopExecutor
    {
        private readonly LoopMode _mode;
        private readonly int[] _array;
        private readonly int _threshold;
        private readonly int _n;

        public int J { get; private set; }
        public int Res { get; private set; }
        public bool IsFinished { get; private set; }

        public LoopExecutor(LoopMode mode, int[] array, int threshold)
        {
            _mode = mode;
            _array = array ?? Array.Empty<int>();
            _threshold = threshold;
            _n = _array.Length;

            // Инициализация согласно Pre/Inv режима
            switch (mode)
            {
                case LoopMode.PrefixSum:
                case LoopMode.CountGreaterThanT:
                    J = 0; Res = 0; break;
                case LoopMode.PrefixMax:
                    if (_n < 1) { IsFinished = true; J = 0; Res = 0; break; }
                    J = 1; Res = _array[0]; break;
            }
        }

        /// <summary>Инвариант истинен в текущем состоянии (до шага).</summary>
        public bool InvBefore() =>
            LoopEvaluator.InvHolds(_mode, _array, J, Res, _threshold);

        /// <summary>Один шаг: тело S + j++, с проверкой Inv после и лога t.</summary>
        public LoopExecutionState Step()
        {
            var state = new LoopExecutionState { J = J, Res = Res };

            if (IsFinished || !LoopEvaluator.GuardHolds(_mode, _array, J))
            {
                IsFinished = true;
                state.IsFinished = true;
                state.J = J; state.Res = Res;
                state.PostHolds = LoopEvaluator.PostHolds(_mode, _array, J, Res, _threshold);
                return state;
            }

            state.InvBeforeHolds = InvBefore();

            int changedIdx = J;

            switch (_mode)
            {
                case LoopMode.PrefixSum:
                    Res += _array[J]; break;
                case LoopMode.CountGreaterThanT:
                    if (_array[J] > _threshold) Res++; break;
                case LoopMode.PrefixMax:
                    if (_array[J] > Res) Res = _array[J]; break;
            }
            J++;

            state.J = J;
            state.Res = Res;
            state.ChangedIndex = changedIdx;
            state.InvAfterHolds = LoopEvaluator.InvHolds(_mode, _array, J, Res, _threshold);
            state.VariantT = LoopEvaluator.VariantValue(_n, J);

            if (!LoopEvaluator.GuardHolds(_mode, _array, J))
            {
                IsFinished = true;
                state.IsFinished = true;
                state.PostHolds = LoopEvaluator.PostHolds(_mode, _array, J, Res, _threshold);
            }

            return state;
        }
    }
}
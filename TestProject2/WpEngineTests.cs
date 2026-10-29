using System;
using System.Linq;
using Xunit;
using BuisnessLogic;

namespace Nightlight.Tests
{
    public class WpEngineTests
    {
        private readonly WpEngine _engine;

        public WpEngineTests()
        {
            _engine = new WpEngine();
        }

        // 1. Тесты валидации входных данных
        [Fact]
        public void CalculateWp_EmptyCode_ThrowsPreViolationException()
        {
            var ex = Assert.Throws<Guard.PreViolationException>(() => _engine.CalculateWp("", "x > 0"));
            Assert.Contains("Код фрагмента не должен быть пустым", ex.Message);
        }

        [Fact]
        public void CalculateWp_EmptyPost_ThrowsPreViolationException()
        {
            var ex = Assert.Throws<Guard.PreViolationException>(() => _engine.CalculateWp("x := 5", ""));
            Assert.Contains("Постусловие (Post) не должно быть пустым", ex.Message);
        }

        [Fact]
        public void CalculateWp_WhitespaceCode_ThrowsPreViolationException()
        {
            var ex = Assert.Throws<Guard.PreViolationException>(() => _engine.CalculateWp("   \n  ", "x > 0"));
            Assert.Contains("Код фрагмента не должен быть пустым", ex.Message);
        }

        [Fact]
        public void CalculateWp_WhitespacePost_ThrowsPreViolationException()
        {
            var ex = Assert.Throws<Guard.PreViolationException>(() => _engine.CalculateWp("x := 5", "   "));
            Assert.Contains("Постусловие (Post) не должно быть пустым", ex.Message);
        }

        [Fact]
        public void CalculateWp_CompletelyInvalidSyntax_ReturnsErrorMessage()
        {
            var result = _engine.CalculateWp("это просто какой-то бред", "x > 0");
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.ErrorMessage);
        }

        // 2. Тесты присваивания
        [Fact]
        public void CalculateWp_SimpleAssignment_SubstitutesCorrectly()
        {
            var result = _engine.CalculateWp("x := 5", "x > 0");
            Assert.True(result.IsSuccess);
            Assert.Contains("5 > 0", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_VariableExpression_SubstitutesCorrectly()
        {
            var result = _engine.CalculateWp("x := y + 1", "x > 5");
            Assert.True(result.IsSuccess);
            Assert.Contains("y + 1 > 5", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_DivisionExpression_AddsDefinednessCondition()
        {
            var result = _engine.CalculateWp("x := 10 / y", "x > 0");
            Assert.True(result.IsSuccess);
            Assert.Contains("y != 0", result.FinalWpShort);
            Assert.Contains("10 / y > 0", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_ComplexExpression_AddsParentheses()
        {
            var result = _engine.CalculateWp("x := a + b", "x > 10");
            Assert.True(result.IsSuccess);
            Assert.Contains("(a + b) > 10", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_MultipleVariables_OnlyTargetSubstituted()
        {
            var result = _engine.CalculateWp("x := y + z", "x > 0 ∧ y < 10");
            Assert.True(result.IsSuccess);
            Assert.Contains("y + z > 0", result.FinalWpShort);
            Assert.Contains("y < 10", result.FinalWpShort);
        }

        // 3. Тесты последовательности
        [Fact]
        public void CalculateWp_SequenceOfTwoAssignments_PullsBackCorrectly()
        {
            var result = _engine.CalculateWp("y := x + 1;\nx := x + 10;", "y == x - 9 ∧ x > 15");
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Steps.Count);
            Assert.Contains("x := x + 10", result.Steps[0].StatementText);
            Assert.Contains("y := x + 1", result.Steps[1].StatementText);
        }

        [Fact]
        public void CalculateWp_Sequence_SimplifiesLinearInequalitiesAddition()
        {
            var result = _engine.CalculateWp("x := x + 10", "x > 15");
            Assert.True(result.IsSuccess);
            Assert.Contains("x > 5", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_Sequence_SimplifiesLinearInequalitiesSubtraction()
        {
            var result = _engine.CalculateWp("x := x - 5", "x > 0");
            Assert.True(result.IsSuccess);
            Assert.Contains("x > 5", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_Sequence_SimplifiesTautology()
        {
            var result = _engine.CalculateWp("x := x + 1", "(x + 1) == x + 1");
            Assert.True(result.IsSuccess);
            Assert.Contains("true", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_SequenceOfThreeAssignments_ProcessesAll()
        {
            var result = _engine.CalculateWp("a := 1;\nb := a + 1;\nc := b + 1;", "c > 0");
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Steps.Count);
        }

        // 4. Тесты ветвления
        [Fact]
        public void CalculateWp_IfStatement_BuildsDisjunctionCorrectly()
        {
            var code = "if (x1 >= x2) {\n    max := x1;\n} else {\n    max := x2;\n}";
            var result = _engine.CalculateWp(code, "max > 100");
            Assert.True(result.IsSuccess);
            Assert.Contains("∧", result.FinalWpShort);
            Assert.Contains("∨", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_IfStatement_NegatesGreaterThanCorrectly()
        {
            var code = "if (a > 0) {\n    x := 1;\n} else {\n    x := -1;\n}";
            var result = _engine.CalculateWp(code, "x == 1");
            Assert.True(result.IsSuccess);
            Assert.Contains("a <= 0", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_IfStatement_NegatesLessThanCorrectly()
        {
            var code = "if (a < b) {\n    x := 1;\n} else {\n    x := 0;\n}";
            var result = _engine.CalculateWp(code, "x == 1");
            Assert.True(result.IsSuccess);
            Assert.Contains("a >= b", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_IfStatement_NegatesEqualityCorrectly()
        {
            var code = "if (a == b) {\n    x := 1;\n} else {\n    x := 0;\n}";
            var result = _engine.CalculateWp(code, "x == 1");
            Assert.True(result.IsSuccess);
            Assert.Contains("a != b", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_QuadraticEquationPreset_HandlesComplexBranchingAndODZ()
        {
            var code = "if (D >= 0) {\n    x := (-b + sqrtD) / (2 * a);\n} else {\n    x := -999;\n}";
            var post = "(D >= 0 ∧ x == (-b + sqrtD) / (2 * a)) ∨ (D < 0 ∧ x == -999)";
            var result = _engine.CalculateWp(code, post);
            Assert.True(result.IsSuccess);
            Assert.Contains("2 * a != 0", result.FinalWpShort);
        }

        // 5. Тесты интеграции и триады Хоара
        [Fact]
        public void CalculateWp_GeneratesValidHoareTriadString()
        {
            var result = _engine.CalculateWp("x := 5", "x > 0", "x строго больше нуля");
            Assert.True(result.IsSuccess);
            Assert.Contains("{ Pre:", result.HoareTriad);
            Assert.Contains("x := 5", result.HoareTriad);
            Assert.Contains("{ Post:", result.HoareTriad);
            Assert.Contains("x строго больше нуля", result.HoareTriad);
        }

        [Fact]
        public void CalculateWp_InvalidAssignmentSyntax_ReturnsErrorMessage()
        {
            var result = _engine.CalculateWp("x = 5", "x > 0");
            Assert.False(result.IsSuccess);
            Assert.Contains("Некорректный оператор присваивания", result.ErrorMessage);
        }

        [Fact]
        public void CalculateWp_MissingElseBranch_ReturnsErrorMessage()
        {
            var result = _engine.CalculateWp("if (x > 0) { x := 1; }", "x > 0");
            Assert.False(result.IsSuccess);
            Assert.Contains("должен содержать полную форму с веткой else", result.ErrorMessage);
        }

        [Fact]
        public void CalculateWp_MissingSemicolonInBranch_ReturnsErrorMessage()
        {
            var result = _engine.CalculateWp("if (x > 0) { x := 1 else { x := 0; }", "x > 0");
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void CalculateWp_FullWorkflow_SequenceAndBranchingCombined()
        {
            var code = "y := 0;\nif (x > 0) {\n    y := x;\n} else {\n    y := -x;\n}";
            var result = _engine.CalculateWp(code, "y >= 0");
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Steps.Count);
        }

        // 6. Тесты пресетов
        [Fact]
        public void GetDefaultPresets_ReturnsExactlyThreePresets()
        {
            var presets = WpPresets.GetDefaultPresets();
            Assert.Equal(3, presets.Count);
        }

        [Fact]
        public void GetDefaultPresets_ContainsMaxOfTwo()
        {
            var presets = WpPresets.GetDefaultPresets();
            var maxPreset = presets.FirstOrDefault(p => p.Name.Contains("Max из двух"));
            Assert.NotNull(maxPreset);
            Assert.Contains("if (x1 >= x2)", maxPreset.Code);
        }

        [Fact]
        public void GetDefaultPresets_ContainsQuadraticEquation()
        {
            var presets = WpPresets.GetDefaultPresets();
            var quadPreset = presets.FirstOrDefault(p => p.Name.Contains("Квадратное уравнение"));
            Assert.NotNull(quadPreset);
            Assert.Contains("D >= 0", quadPreset.Code);
        }

        [Fact]
        public void GetDefaultPresets_ContainsSequence()
        {
            var presets = WpPresets.GetDefaultPresets();
            var seqPreset = presets.FirstOrDefault(p => p.Name.Contains("Последовательность"));
            Assert.NotNull(seqPreset);
            Assert.Contains("y := x + 1", seqPreset.Code);
        }

        [Fact]
        public void GetDefaultPresets_AllPresetsHaveHumanReadablePost()
        {
            var presets = WpPresets.GetDefaultPresets();
            foreach (var preset in presets)
            {
                Assert.False(string.IsNullOrWhiteSpace(preset.PostHuman));
                Assert.False(string.IsNullOrWhiteSpace(preset.PostShort));
            }
        }

        // 7. Тесты перевода на человеческий язык
        [Fact]
        public void TranslateToHuman_ReplacesBasicLogicalOperators()
        {
            var result = _engine.TranslateToHuman("x > 5 ∧ y != 0 ∨ z == 10");
            Assert.Contains(" И ", result);
            Assert.Contains(" ИЛИ ", result);
        }

        [Fact]
        public void TranslateToHuman_ReplacesComparisonOperators()
        {
            var result = _engine.TranslateToHuman("a >= b ∧ c <= d");
            Assert.Contains(" больше или равно ", result);
            Assert.Contains(" меньше или равно ", result);
        }

        [Fact]
        public void TranslateToHuman_ReplacesEqualityAndInequality()
        {
            var result = _engine.TranslateToHuman("x == 5 ∧ y != 10");
            Assert.Contains(" равно ", result);
            Assert.Contains(" не равно ", result);
        }

        [Fact]
        public void TranslateToHuman_ReplacesNegation()
        {
            var result = _engine.TranslateToHuman("¬(x > 0)");
            Assert.Contains("НЕ ", result);
        }

        [Fact]
        public void TranslateToHuman_HandlesEmptyOrWhitespaceGracefully()
        {
            var resultEmpty = _engine.TranslateToHuman("");
            Assert.Equal(string.Empty, resultEmpty);

            var resultSpace = _engine.TranslateToHuman("   ");
            Assert.Equal(string.Empty, resultSpace);
        }
    }
}
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
        // 1. ТЕСТЫ ВАЛИДАЦИИ ВХОДНЫХ ДАННЫХ
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

        // 2. ТЕСТЫ ПРИСВАИВАНИЯ

        [Fact]
        public void CalculateWp_SimpleAssignment_SubstitutesCorrectly()
        {
            var result = _engine.CalculateWp("x := 5", "x > 0");

            Assert.True(result.IsSuccess);
            Assert.Contains("5 > 0", result.FinalWpShort);
            Assert.Single(result.Steps);
            Assert.Equal("x := 5", result.Steps[0].StatementText);
        }

        // 3. ТЕСТЫ ПОСЛЕДОВАТЕЛЬНОСТИ

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
        public void CalculateWp_Sequence_SimplifiesLinearInequalities()
        {
            var result = _engine.CalculateWp("x := x + 10", "x > 15");

            Assert.True(result.IsSuccess);
            Assert.Contains("x > 5", result.FinalWpShort);
        }

        // 4. ТЕСТЫ ВЕТВЛЕНИЯ
        [Fact]
        public void CalculateWp_IfStatement_BuildsDisjunctionCorrectly()
        {
            var code = "if (x1 >= x2) {\n    max := x1;\n} else {\n    max := x2;\n}";
            var result = _engine.CalculateWp(code, "max > 100");

            Assert.True(result.IsSuccess);
            Assert.Contains("∧", result.FinalWpShort);
            Assert.Contains("∨", result.FinalWpShort);
            Assert.Contains("x1 >= x2", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_IfStatement_NegatesConditionCorrectly()
        {
            var code = "if (a > 0) {\n    x := 1;\n} else {\n    x := -1;\n}";
            var result = _engine.CalculateWp(code, "x == 1");

            Assert.True(result.IsSuccess);
            Assert.Contains("a <= 0", result.FinalWpShort);
        }

        [Fact]
        public void CalculateWp_QuadraticEquationPreset_HandlesComplexBranching()
        {
            var code = "if (D >= 0) {\n    x := (-b + sqrtD) / (2 * a);\n} else {\n    x := -999;\n}";
            var post = "(D >= 0 ∧ x == (-b + sqrtD) / (2 * a)) ∨ (D < 0 ∧ x == -999)";
            var result = _engine.CalculateWp(code, post);
            Assert.True(result.IsSuccess);
            Assert.Contains("2 * a != 0", result.FinalWpShort);
        }
        // 5. ТЕСТЫ ИНТЕГРАЦИИ И ТРИАДЫ ХОАРА
        [Fact]
        public void CalculateWp_GeneratesValidHoareTriad()
        {
            var result = _engine.CalculateWp("x := 5", "x > 0", "x строго больше нуля");

            Assert.True(result.IsSuccess);
            Assert.Contains("{ Pre:", result.HoareTriad);
            Assert.Contains("x := 5", result.HoareTriad);
            Assert.Contains("{ Post:", result.HoareTriad);
            Assert.Contains("x строго больше нуля", result.HoareTriad);
        }

        [Fact]
        public void CalculateWp_InvalidCode_ReturnsErrorMessage()
        {
            var result = _engine.CalculateWp("x = 5", "x > 0");

            Assert.False(result.IsSuccess);
            Assert.Contains("Некорректный оператор присваивания", result.ErrorMessage);
        }

        // 6. ТЕСТЫ ПРЕСЕТОВ
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
            Assert.Contains("max > 100", maxPreset.PostShort);
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

        // 7. ТЕСТЫ ПЕРЕВОДА НА ЧЕЛОВЕЧЕСКИЙ ЯЗЫК
        [Fact]
        public void TranslateToHuman_ReplacesLogicalOperators()
        {
            var result = _engine.TranslateToHuman("x > 5 ∧ y != 0 ∨ z == 10");

            Assert.Contains(" И ", result);
            Assert.Contains(" ИЛИ ", result);
            Assert.Contains(" больше ", result);
            Assert.Contains(" не равно ", result);
            Assert.Contains(" равно ", result);
        }
    }
}
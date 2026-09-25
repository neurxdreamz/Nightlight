using System;
using Xunit;
using BuisnessLogic;

namespace Nightlight.Tests
{
    public class NightlightModelTests
    {
        // ТЕСТЫ ДЛЯ AutoToggle (Автоматическое включение)


        [Fact]
        public void AutoToggle_GranicaVkluchenia_AmbientBrightness29_MotionTrue_TurnsOn()
        {
            // Arrange
            var model = new NightlightModel();

            // Act: 29 - это верхняя граница условия включения (< 30)
            model.AutoToggle(IsMotion: true, AmbientBrightness: 29);

            // Assert: Проверка Post-условия
            Assert.True(model.IsOn, "Ночник должен включиться при яркости 29 и наличии движения");
        }

        [Fact]
        public void AutoToggle_GranicaOtkluchenia_AmbientBrightness30_MotionTrue_StaysOff()
        {
            // Arrange
            var model = new NightlightModel();

            // Act: 30 - это первое значение, при котором условие (< 30) ложно
            model.AutoToggle(IsMotion: true, AmbientBrightness: 30);

            // Assert: Проверка Post-условия
            Assert.False(model.IsOn, "Ночник должен остаться выключенным при яркости 30");
        }

        [Fact]
        public void AutoToggle_NegativnayaYarkost_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel();

            // Act & Assert: -1 нарушает Pre-условие (диапазон 0..100)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.AutoToggle(true, -1));
            Assert.Contains("Яркость должна быть в диапазоне от 0 до 100", exception.Message);
        }

        // ТЕСТЫ ДЛЯ SetBrightness (Ручная установка яркости)

        [Fact]
        public void SetBrightness_MinimalnayaYarkost_TargetBrightness1_SetsCorrectly()
        {
            // Arrange
            var model = new NightlightModel();

            // Act: 1 - нижняя граница допустимого диапазона (1..100)
            model.SetBrightness(1);

            // Assert: Проверка Post-условий
            Assert.Equal(1, model.CurrentBrightness);
            Assert.True(model.IsOn, "При установке яркости ночник должен автоматически включиться");
        }

        [Fact]
        public void SetBrightness_NulevayaYarkost_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel();

            // Act & Assert: 0 нарушает Pre-условие (должно быть > 0)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetBrightness(0));
            Assert.Contains("Яркость должна быть от 1 до 100", exception.Message);
        }

        // ТЕСТЫ ДЛЯ SetSleepTimer (Установка таймера сна)

        [Fact]
        public void SetSleepTimer_NochinkVikluchen_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel(); // По умолчанию IsOn = false

            // Act & Assert: Нарушение составного Pre-условия (ночник должен быть включен)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(30));
            Assert.Contains("Ночник должен быть включен для установки таймера", exception.Message);
        }

        [Fact]
        public void SetSleepTimer_MaximalnayaZaderzhka_DelayMinutes240_SetsCorrectly()
        {
            // Arrange
            var model = new NightlightModel();
            model.SetBrightness(50); // Выполняем первое Pre-условие (включаем ночник)

            // Act: 240 - верхняя граница допустимого диапазона (1..240)
            model.SetSleepTimer(240);

            // Assert: Проверка Post-условий
            Assert.Equal(240, model.TimerRemaining);
            Assert.True(model.IsTimerActive, "Таймер должен быть активен");
        }

        [Fact]
        public void SetSleepTimer_SverkhMaximalnayaZaderzhka_DelayMinutes241_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel();
            model.SetBrightness(50); // Включаем ночник

            // Act & Assert: 241 нарушает Pre-условие (максимум 240)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(241));
            Assert.Contains("Таймер должен быть от 1 до 240 минут", exception.Message);
        }
    }
}
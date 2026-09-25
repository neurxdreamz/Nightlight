using System;
using Xunit;
using BuisnessLogic;

namespace Nightlight.Tests
{
    public class NightlightModelTests
    {
        // ТЕСТЫ ДЛЯ AutoToggle

        [Fact]
        public void AutoToggle_GranicaVkluchenia_AmbientBrightness29_MotionTrue_TurnsOn()
        {
            // Arrange
            var model = new NightlightModel();

            // Act (Граничное значение: 29 - это последнее значение, при котором ночник включится)
            model.AutoToggle(IsMotion: true, AmbientBrightness: 29);

            // Assert (Проверка Post-условия)
            Assert.True(model.IsOn, "Ночник должен быть включен при яркости 29 и наличии движения");
        }

        [Fact]
        public void AutoToggle_GranicaOtkluchenia_AmbientBrightness30_MotionTrue_StaysOff()
        {
            // Arrange
            var model = new NightlightModel();

            // Act (Граничное значение: 30 - ночник уже не должен включаться)
            model.AutoToggle(IsMotion: true, AmbientBrightness: 30);

            // Assert (Проверка Post-условия)
            Assert.False(model.IsOn, "Ночник должен оставаться выключенным при яркости 30, даже при движении");
        }

        [Fact]
        public void AutoToggle_NegativnayaYarkost_ThrowsPreViolationException()
        // Граничное невалидное значение: -1
        {
            // Arrange
            var model = new NightlightModel();

            // Act & Assert
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.AutoToggle(true, -1));
            Assert.Contains("Яркость должна быть в диапазоне от 0 до 100", exception.Message);
        }

        // ТЕСТЫ ДЛЯ SetBrightness

        [Fact]
        public void SetBrightness_MinimalnayaYarkost_TargetBrightness1_SetsCorrectly()
        {
            // Arrange
            var model = new NightlightModel();

            // Act (Граничное валидное значение: 1)
            model.SetBrightness(1);

            // Assert (Проверка Post-условий)
            Assert.Equal(1, model.CurrentBrightness);
            Assert.True(model.IsOn, "При установке яркости ночник должен автоматически включиться");
        }

        [Fact]
        public void SetBrightness_NulevayaYarkost_ThrowsPreViolationException()
        // Граничное невалидное значение: 0 (должно быть > 0)
        {
            // Arrange
            var model = new NightlightModel();

            // Act & Assert
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetBrightness(0));
            Assert.Contains("Яркость должна быть от 1 до 100", exception.Message);
        }

        // ТЕСТЫ ДЛЯ SetSleepTimer

        [Fact]
        public void SetSleepTimer_NochinkVikluchen_ThrowsPreViolationException()
        // Проверка составного предусловия: таймер нельзя поставить на выключенный ночник
        {
            // Arrange
            var model = new NightlightModel();

            // Act & Assert
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(30));
            Assert.Contains("Ночник должен быть включен для установки таймера", exception.Message);
        }

        [Fact]
        public void SetSleepTimer_MaximalnayaZaderzhka_DelayMinutes240_SetsCorrectly()
        {
            // Arrange
            var model = new NightlightModel();
            model.SetBrightness(50); // Сначала включаем ночник, чтобы выполнить первое предусловие

            // Act (Граничное валидное значение: 240 минут)
            model.SetSleepTimer(240);

            // Assert (Проверка Post-условий)
            Assert.Equal(240, model.TimerRemaining);
            Assert.True(model.IsTimerActive);
        }
        [Fact]
        public void AutoToggle_VerkhnyayaGranicaYarkosti_AmbientBrightness100_MotionTrue_StaysOff()
        {
            // Arrange
            var model = new NightlightModel();

            // Act: 100 - это верхняя граница допустимого диапазона [0, 100]. 
            // Ночник не должен включаться при максимальной яркости, даже с датчиком движения.
            model.AutoToggle(IsMotion: true, AmbientBrightness: 100);

            // Assert: Исключения нет (вход валидный), но состояние выключено (логика верна)
            Assert.False(model.IsOn, "Ночник должен оставаться выключенным при максимальной яркости 100");
        }

        [Fact]
        public void SetBrightness_VerkhnyayaGranicaYarkosti_TargetBrightness100_SetsCorrectly()
        {
            // Arrange
            var model = new NightlightModel();

            // Act: 100 - верхняя граница допустимого диапазона [1, 100]
            model.SetBrightness(100);

            // Assert (Проверка Post-условий)
            Assert.Equal(100, model.CurrentBrightness);
            Assert.True(model.IsOn, "При установке яркости 100 ночник должен быть включен");
        }

        [Fact]
        public void SetSleepTimer_NulevayaZaderzhka_DelayMinutes0_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel();
            model.SetBrightness(50); // Сначала включаем, чтобы пройти проверку IsOn

            // Act & Assert: 0 - это граничное невалидное значение (должно быть > 0)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(0));
            Assert.Contains("Таймер должен быть от 1 до 240 минут", exception.Message);
        }

        [Fact]
        public void SetSleepTimer_SverkhMaximalnayaZaderzhka_DelayMinutes241_ThrowsPreViolationException()
        {
            // Arrange
            var model = new NightlightModel();
            model.SetBrightness(50); // Сначала включаем

            // Act & Assert: 241 - это первое невалидное значение за верхней границей (максимум 240)
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(241));
            Assert.Contains("Таймер должен быть от 1 до 240 минут", exception.Message);
        }
    }
}

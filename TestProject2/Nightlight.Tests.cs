using System;
using Xunit;
using BuisnessLogic;

namespace Nightlight.Tests
{
    public class NightlightModelTests
    {
        // 1. ТЕСТЫ GUARD

        [Fact]
        public void Guard_Requires_ConditionTrue_DoesNotThrow()
        {
            var exception = Record.Exception(() => Guard.Requires(true, "Test"));
            Assert.Null(exception);
        }

        [Fact]
        public void Guard_Requires_ConditionFalse_ThrowsException()
        {
            var exception = Assert.Throws<Guard.PreViolationException>(() => Guard.Requires(false, "Error message"));
            Assert.Equal("Error message", exception.Message);
        }

        [Fact]
        public void Guard_PreViolationException_CanCreateInstance()
        {
            var ex = new Guard.PreViolationException("Test");
            Assert.NotNull(ex);
            Assert.IsType<Guard.PreViolationException>(ex);
        }

        [Fact]
        public void Guard_PreViolationException_InheritsFromException()
        {
            var ex = new Guard.PreViolationException("Test");
            Assert.IsAssignableFrom<Exception>(ex);
        }

        // 2. ТЕСТЫ AutoToggle


        [Fact]
        public void AutoToggle_GranicaVkluchenia_AmbientBrightness29_MotionTrue_TurnsOn()
        {
            var model = new NightlightModel();
            model.AutoToggle(IsMotion: true, AmbientBrightness: 29);
            Assert.True(model.IsOn, "Ночник должен включиться при яркости 29 и наличии движения");
        }

        [Fact]
        public void AutoToggle_GranicaOtkluchenia_AmbientBrightness30_MotionTrue_StaysOff()
        {
            var model = new NightlightModel();
            model.AutoToggle(IsMotion: true, AmbientBrightness: 30);
            Assert.False(model.IsOn, "Ночник должен остаться выключенным при яркости 30");
        }

        [Fact]
        public void AutoToggle_NegativnayaYarkost_ThrowsPreViolationException()
        {
            var model = new NightlightModel();
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.AutoToggle(true, -1));
            Assert.Contains("Яркость должна быть в диапазоне от 0 до 100", exception.Message);
        }

        [Fact]
        public void AutoToggle_Brightness0_MotionTrue_TurnsOn()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 0);
            Assert.True(model.IsOn);
        }

        [Fact]
        public void AutoToggle_Brightness100_MotionTrue_StaysOff()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 100);
            Assert.False(model.IsOn);
        }

        [Fact]
        public void AutoToggle_Brightness101_ThrowsException()
        {
            var model = new NightlightModel();
            Assert.Throws<Guard.PreViolationException>(() => model.AutoToggle(true, 101));
        }

        [Fact]
        public void AutoToggle_NoMotion_BrightnessLow_StaysOff()
        {
            var model = new NightlightModel();
            model.AutoToggle(false, 10);
            Assert.False(model.IsOn);
        }

        [Fact]
        public void AutoToggle_NoMotion_BrightnessHigh_StaysOff()
        {
            var model = new NightlightModel();
            model.AutoToggle(false, 80);
            Assert.False(model.IsOn);
        }

        [Fact]
        public void AutoToggle_MotionTrue_Brightness28_TurnsOn()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 28);
            Assert.True(model.IsOn);
        }

        [Fact]
        public void AutoToggle_MotionTrue_Brightness31_StaysOff()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 31);
            Assert.False(model.IsOn);
        }

        [Fact]
        public void AutoToggle_MultipleCalls_StateUpdatesCorrectly()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 20);
            Assert.True(model.IsOn);

            model.AutoToggle(true, 50);
            Assert.False(model.IsOn);

            model.AutoToggle(true, 15);
            Assert.True(model.IsOn);
        }


        // 3. ТЕСТЫ SetBrightness

        [Fact]
        public void SetBrightness_MinimalnayaYarkost_TargetBrightness1_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(1);
            Assert.Equal(1, model.CurrentBrightness);
            Assert.True(model.IsOn, "При установке яркости ночник должен автоматически включиться");
        }

        [Fact]
        public void SetBrightness_NulevayaYarkost_ThrowsPreViolationException()
        {
            var model = new NightlightModel();
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetBrightness(0));
            Assert.Contains("Яркость должна быть от 1 до 100", exception.Message);
        }

        [Fact]
        public void SetBrightness_MaximalnayaYarkost_100_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(100);
            Assert.Equal(100, model.CurrentBrightness);
            Assert.True(model.IsOn);
        }

        [Fact]
        public void SetBrightness_Brightness101_ThrowsException()
        {
            var model = new NightlightModel();
            Assert.Throws<Guard.PreViolationException>(() => model.SetBrightness(101));
        }

        [Fact]
        public void SetBrightness_Brightness50_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            Assert.Equal(50, model.CurrentBrightness);
            Assert.True(model.IsOn);
        }

        [Fact]
        public void SetBrightness_NegativeValue_ThrowsException()
        {
            var model = new NightlightModel();
            Assert.Throws<Guard.PreViolationException>(() => model.SetBrightness(-10));
        }

        [Fact]
        public void SetBrightness_MultipleChanges_StateUpdatesCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(30);
            Assert.Equal(30, model.CurrentBrightness);

            model.SetBrightness(75);
            Assert.Equal(75, model.CurrentBrightness);

            model.SetBrightness(1);
            Assert.Equal(1, model.CurrentBrightness);
        }

        [Fact]
        public void SetBrightness_AlwaysTurnsOn_RegardlessOfPreviousState()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            Assert.True(model.IsOn);
        }

        // 4. ТЕСТЫ SetSleepTimer

        [Fact]
        public void SetSleepTimer_NochinkVikluchen_ThrowsPreViolationException()
        {
            var model = new NightlightModel();
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(30));
            Assert.Contains("Ночник должен быть включен для установки таймера", exception.Message);
        }

        [Fact]
        public void SetSleepTimer_MaximalnayaZaderzhka_DelayMinutes240_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            model.SetSleepTimer(240);
            Assert.Equal(240, model.TimerRemaining);
            Assert.True(model.IsTimerActive, "Таймер должен быть активен");
        }

        [Fact]
        public void SetSleepTimer_SverkhMaximalnayaZaderzhka_DelayMinutes241_ThrowsPreViolationException()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            var exception = Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(241));
            Assert.Contains("Таймер должен быть от 1 до 240 минут", exception.Message);
        }

        [Fact]
        public void SetSleepTimer_MinimalDelay_1Minute_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            model.SetSleepTimer(1);
            Assert.Equal(1, model.TimerRemaining);
            Assert.True(model.IsTimerActive);
        }

        [Fact]
        public void SetSleepTimer_ZeroDelay_ThrowsException()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(0));
        }

        [Fact]
        public void SetSleepTimer_NegativeDelay_ThrowsException()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(-5));
        }

        [Fact]
        public void SetSleepTimer_MediumDelay_60Minutes_SetsCorrectly()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            model.SetSleepTimer(60);
            Assert.Equal(60, model.TimerRemaining);
            Assert.True(model.IsTimerActive);
        }

        [Fact]
        public void SetSleepTimer_ValidInput_InvokesStateChangedEvent()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            bool eventFired = false;
            model.StateChanged += () => eventFired = true;

            model.SetSleepTimer(30);
            Assert.True(eventFired);
        }

        [Fact]
        public void SetSleepTimer_CanSetMultipleTimes_UpdatesState()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);

            model.SetSleepTimer(10);
            Assert.Equal(10, model.TimerRemaining);

            model.SetSleepTimer(120);
            Assert.Equal(120, model.TimerRemaining);
        }


        // 5. ТЕСТЫ КОНСТРУКТОРА И СОСТОЯНИЯ ПО УМОЛЧАНИЮ

        [Fact]
        public void Constructor_DefaultState_IsOnFalse()
        {
            var model = new NightlightModel();
            Assert.False(model.IsOn);
        }

        [Fact]
        public void Constructor_DefaultState_IsTimerActiveFalse()
        {
            var model = new NightlightModel();
            Assert.False(model.IsTimerActive);
        }

        [Fact]
        public void Constructor_DefaultState_CurrentBrightnessZero()
        {
            var model = new NightlightModel();
            Assert.Equal(0, model.CurrentBrightness);
        }

        [Fact]
        public void Constructor_DefaultState_TimerRemainingZero()
        {
            var model = new NightlightModel();
            Assert.Equal(0, model.TimerRemaining);
        }

        [Fact]
        public void Constructor_CreatesValidInstance()
        {
            var model = new NightlightModel();
            Assert.NotNull(model);
        }

        // 6. ТЕСТЫ СОЧЕТАНИЯ ОПЕРАЦИЙ

        [Fact]
        public void Integration_AutoToggleThenSetBrightness_StateCorrect()
        {
            var model = new NightlightModel();
            model.AutoToggle(true, 20);
            Assert.True(model.IsOn);

            model.SetBrightness(75);
            Assert.Equal(75, model.CurrentBrightness);
            Assert.True(model.IsOn);
        }

        [Fact]
        public void Integration_SetBrightnessThenSetTimer_StateCorrect()
        {
            var model = new NightlightModel();
            model.SetBrightness(50);
            Assert.True(model.IsOn);

            model.SetSleepTimer(30);
            Assert.True(model.IsTimerActive);
            Assert.Equal(30, model.TimerRemaining);
        }

        [Fact]
        public void Integration_FullWorkflow_AllOperationsWork()
        {
            var model = new NightlightModel();

            // Автоматическое включение
            model.AutoToggle(true, 15);
            Assert.True(model.IsOn);

            // Ручная установка яркости
            model.SetBrightness(80);
            Assert.Equal(80, model.CurrentBrightness);

            // Установка таймера
            model.SetSleepTimer(45);
            Assert.Equal(45, model.TimerRemaining);
            Assert.True(model.IsTimerActive);
        }

        [Fact]
        public void Integration_AutoToggleOff_CannotSetTimer()
        {
            var model = new NightlightModel();
            model.AutoToggle(false, 80); // Выключено
            Assert.False(model.IsOn);

            Assert.Throws<Guard.PreViolationException>(() => model.SetSleepTimer(30));
        }
    }
}
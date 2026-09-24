using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BuisnessLogic
{
    public class NightlightModel
    {
        public bool IsOn { get; private set; }
        public bool IsTimerActive { get; private set; }
        public int CurrentBrightness { get; private set; }
        public int Timer { get; private set; }


        /// <summary>
        /// Метод отвечающий за автоматическое включение ночника
        /// </summary>
        public void AutoToggle(bool IsMotion, int AmbientBrightness)
        {
            //Pre
            Guard.Requires(AmbientBrightness >= 0 && AmbientBrightness <= 100, "Яркость должна быть в диапазоне от 0 до 100");

            //Method
            IsOn = IsMotion && (AmbientBrightness < 30);

            // Post
            Debug.Assert(IsOn == (IsMotion && AmbientBrightness < 30), "PostViolation: Состояние ночника не соответствует датчикам");
        }

        /// <summary>
        /// метод для ручной установки яркости
        /// </summary>
        public void SetBrightness(int TargetBrightness)
        {
            //Pre
            Guard.Requires(TargetBrightness > 0 && TargetBrightness <= 100, "Яркость должна быть от 1 до 100");

            //method
            CurrentBrightness = TargetBrightness;
            IsOn = true;

            //Post
            Debug.Assert(CurrentBrightness == TargetBrightness && IsOn == true, "PostViolation: Яркость не установлена");
        }
        /// <summary>
        /// Метод для установки таймера
        /// </summary>
        public void SetTimer(int DelayMinutes)
        {
            //Pre
            Guard.Requires(IsOn == true, "Ночник должен быть включен");
            Guard.Requires(DelayMinutes > 0 && DelayMinutes <= 240, "Таймер должен быть от 1 до 240 минут");

            //Method
            IsTimerActive = true;
            Timer = DelayMinutes;

            //Post
            Debug.Assert(Timer == DelayMinutes && IsTimerActive, "PostViolation: Таймер не установлен");

        }
    }
}

using System.Collections.Generic;

namespace BuisnessLogic
{
    public static class WpPresets
    {
        public static List<WpPreset> GetDefaultPresets()
        {
            return new List<WpPreset>
            {
                // 1. Обязательный пресет «Max из двух» -> Выбор яркости из двух датчиков
                new WpPreset
                {
                    Name = "1. Max из двух (Выбор яркости по 2 датчикам)",
                    Code = "if (x1 >= x2) {\n    max := x1;\n} else {\n    max := x2;\n}",
                    PostShort = "max > 50",
                    PostHuman = "Итоговая яркость ночника (max) должна быть больше 50%"
                },

                // 2. Обязательный пресет «Квадратное уравнение» -> Расчет мощности свечения по D
                new WpPreset
                {
                    Name = "2. Квадратное уравнение (Импульс яркости по D)",
                    Code = "if (D >= 0) {\n    x := (-b + sqrtD) / (2 * a);\n} else {\n    x := -999;\n}",
                    PostShort = "(D >= 0 ∧ x == (-b + sqrtD) / (2 * a)) ∨ (D < 0 ∧ x == -999)",
                    PostHuman = "Либо яркость x вычислена по формуле при D >= 0 (и a != 0), либо аварийное отключение (-999)"
                },

                // 3. Обязательный пресет «Последовательность присваиваний» -> Ступенчатый розжиг лампы
                new WpPreset
                {
                    Name = "3. Последовательность (Ступенчатый розжиг лампы)",
                    Code = "y := x + 1;\nx := x + 10;",
                    PostShort = "y == x - 9 ∧ x > 15",
                    PostHuman = "Контрольное значение y синхронизировано, а итоговая яркость x строго больше 15%"
                },

                // 4. Сюжетный пресет ночника: Ночной режим по датчику освещенности
                new WpPreset
                {
                    Name = "4. Авто-яркость ночника от освещенности",
                    Code = "if (lux <= 20) {\n    brightness := 85;\n} else {\n    brightness := 15;\n}",
                    PostShort = "brightness >= 50",
                    PostHuman = "Ночник включается на высокую яркость (не менее 50%) в темноте"
                }
            };
        }
    }
}
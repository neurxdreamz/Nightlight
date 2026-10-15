using System.Collections.Generic;

namespace BuisnessLogic
{
    public static class WpPresets
    {
        public static List<WpPreset> GetDefaultPresets()
        {
            return new List<WpPreset>
            {
                // 1. Max из двух
                new WpPreset
                {
                    Name = "1. Max из двух",
                    Code = "if (x1 >= x2) {\n    max := x1;\n} else {\n    max := x2;\n}",
                    PostShort = "max > 100",
                    PostHuman = "Максимальное из двух чисел строго больше 100"
                },

                // 2. Квадратное уравнение (ветвление по дискриминанту D + проверка деления на 2*a)
                new WpPreset
                {
                    Name = "2. Квадратное уравнение (ветвление по D)",
                    Code = "if (D >= 0) {\n    x := (-b + sqrtD) / (2 * a);\n} else {\n    x := -999;\n}",
                    PostShort = "(D >= 0 ∧ x == (-b + sqrtD) / (2 * a)) ∨ (D < 0 ∧ x == -999)",
                    PostHuman = "Либо корень вычислен по формуле при D >= 0 (и a != 0), либо записано специальное значение -999"
                },

                // 3. Последовательность присваиваний
                new WpPreset
                {
                    Name = "3. Последовательность присваиваний",
                    Code = "y := x + 1;\nx := x + 10;",
                    PostShort = "y == x - 9 ∧ x > 15",
                    PostHuman = "y равен x минус 9, при этом итоговый x строго больше 15"
                }
            };
        }
    }
}
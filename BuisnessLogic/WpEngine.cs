using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static BuisnessLogic.WpStatements;

namespace BuisnessLogic
{
    public class WpEngine
    {
        /// <summary>
        /// Главный метод: рассчитывает слабейшее предусловие (wp) для фрагмента кода и постусловия
        /// </summary>
        public WpCalculationResult CalculateWp(string code, string postShort, string postHuman = "")
        {
            Guard.Requires(!string.IsNullOrWhiteSpace(code), "Код фрагмента не должен быть пустым");
            Guard.Requires(!string.IsNullOrWhiteSpace(postShort), "Постусловие (Post) не должно быть пустым");

            var result = new WpCalculationResult();

            // 1. Защита от незаконченных инструкций (обрывов на знаках присваивания или операторах)
            var rawLines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawLine in rawLines)
            {
                var trimmed = rawLine.Trim().TrimEnd(';');
                if (trimmed.EndsWith(":=") || trimmed.EndsWith("=") || trimmed.EndsWith("+") || trimmed.EndsWith("-"))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Ошибка синтаксиса: обнаружена незавершенная инструкция.";
                    return result;
                }
            }

            try
            {
                List<Statement> statements = ParseStatements(code);
                if (statements.Count == 0)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Не удалось распознать операторы в коде.";
                    return result;
                }

                string currentCondition = NormalizeLogicalOperators(postShort.Trim());
                int stepCounter = 1;

                currentCondition = PullBackSequence(statements, currentCondition, result.Steps, ref stepCounter);

                result.IsSuccess = true;
                result.FinalWpShort = currentCondition;
                result.FinalWpHuman = TranslateToHuman(currentCondition);

                string readablePost = string.IsNullOrWhiteSpace(postHuman)
                    ? TranslateToHuman(postShort)
                    : postHuman;

                result.HoareTriad =
                    $"{{ Pre: {result.FinalWpShort} ({result.FinalWpHuman}) }}\n" +
                    $"{code.Trim()}\n" +
                    $"{{ Post: {postShort.Trim()} ({readablePost}) }}";

                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Ошибка разбора: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Протаскивание условия назад через последовательность операторов: wp(S1; S2, R) = wp(S1, wp(S2, R))
        /// </summary>
        private string PullBackSequence(List<Statement> statements, string postCondition, List<WpStep> trace, ref int stepCounter)
        {
            string current = postCondition;

            for (int i = statements.Count - 1; i >= 0; i--)
            {
                Statement stmt = statements[i];
                string before = current;

                if (stmt is AssignmentStatement assign)
                {
                    current = ApplyAssignmentRule(assign, current, out string? defCond);
                    trace.Add(new WpStep
                    {
                        StepNumber = stepCounter++,
                        StatementText = assign.RawText,
                        ConditionBefore = before,
                        ConditionAfter = current,
                        DefinednessCondition = defCond
                    });
                }
                else if (stmt is IfStatement ifStmt)
                {
                    current = ApplyIfRule(ifStmt, current, trace, ref stepCounter);
                }
            }

            return current;
        }

        /// <summary>
        /// Правило присваивания: wp(x := e, R) = R[x -> e] ∧ ОДЗ(e)
        /// </summary>
        private string ApplyAssignmentRule(AssignmentStatement assign, string condition, out string? definedness)
        {
            definedness = ExtractDefinednessCondition(assign.Expression);

            
            string pattern = $@"\b{Regex.Escape(assign.Variable)}\b";
            string replacement = NeedsParentheses(assign.Expression)
                ? $"({assign.Expression})"
                : assign.Expression;

            string substituted = Regex.Replace(condition, pattern, replacement);
            string simplified = SimplifyCondition(substituted);

            if (!string.IsNullOrEmpty(definedness))
            {
               
                if (!simplified.Contains(definedness))
                {
                    simplified = $"{definedness} ∧ ({simplified})";
                }
            }

            return simplified;
        }

        /// <summary>
        /// Правило ветвления: wp(if B then S1 else S2, R) = (B ∧ wp(S1, R)) ∨ (¬B ∧ wp(S2, R))
        /// </summary>
        private string ApplyIfRule(IfStatement ifStmt, string condition, List<WpStep> trace, ref int stepCounter)
        {
            var dummyTraceThen = new List<WpStep>();
            var dummyTraceElse = new List<WpStep>();
            int dummyCounter = 1;

            string wpThen = PullBackSequence(ifStmt.ThenBranch, condition, dummyTraceThen, ref dummyCounter);
            string wpElse = PullBackSequence(ifStmt.ElseBranch, condition, dummyTraceElse, ref dummyCounter);

            string condB = NormalizeLogicalOperators(ifStmt.Condition);
            string notB = NegateCondition(condB);

            string combined = $"({condB} ∧ {wpThen}) ∨ ({notB} ∧ {wpElse})";

            trace.Add(new WpStep
            {
                StepNumber = stepCounter++,
                StatementText = $"if ({condB}) {{ then: {FormatBranch(ifStmt.ThenBranch)} }} else {{ else: {FormatBranch(ifStmt.ElseBranch)} }}",
                ConditionBefore = condition,
                ConditionAfter = combined
            });

            return combined;
        }

        /// <summary>
        /// Проверка определенности выражений (защита от деления на 0)
        /// </summary>
        private string? ExtractDefinednessCondition(string expr)
        {
            var conditions = new List<string>();

            // Ищем знаменатели после знака '/'
            var divMatches = Regex.Matches(expr, @"\/\s*(\([^)]+\)|[a-zA-Z0-9_]+)");
            foreach (Match m in divMatches)
            {
                string denom = m.Groups[1].Value.Trim();
                if (denom.StartsWith("(") && denom.EndsWith(")"))
                {
                    denom = denom.Substring(1, denom.Length - 2).Trim();
                }

                // Если знаменатель не является просто ненулевым числом
                if (!double.TryParse(denom, out double num) || num == 0)
                {
                    conditions.Add($"{denom} != 0");
                }
            }

            return conditions.Count > 0 ? string.Join(" ∧ ", conditions.Distinct()) : null;
        }

        /// <summary>
        /// Базовое алгебраическое упрощение линейных выражений и тождеств
        /// </summary>
        private string SimplifyCondition(string cond)
        {
            string result = cond.Trim();

            // Если это конъюнкция нескольких условий (через ∧), упрощаем каждую часть отдельно
            if (result.Contains("∧") && !result.Contains("∨"))
            {
                var parts = result.Split('∧').Select(p => SimplifySingleRelation(p.Trim())).ToList();
                // Убираем тождественную истину (например, x + 1 == x + 1)
                parts = parts.Where(p => p != "true" && p != "Истина").ToList();
                return parts.Count > 0 ? string.Join(" ∧ ", parts) : "true";
            }

            return SimplifySingleRelation(result);
        }

        private string SimplifySingleRelation(string rel)
        {
            string s = rel.Trim();

            // 1. Упрощение вида: y == (x + 10) - 9  -->  y == x + 1
            var matchAddSub = Regex.Match(s, @"^([a-zA-Z0-9_]+)\s*==\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*([+\-])\s*(\d+)\s*\)\s*([+\-])\s*(\d+)$");
            if (matchAddSub.Success)
            {
                string leftVar = matchAddSub.Groups[1].Value;
                string rightVar = matchAddSub.Groups[2].Value;
                int c1 = int.Parse(matchAddSub.Groups[4].Value) * (matchAddSub.Groups[3].Value == "-" ? -1 : 1);
                int c2 = int.Parse(matchAddSub.Groups[6].Value) * (matchAddSub.Groups[5].Value == "-" ? -1 : 1);
                int total = c1 + c2;
                if (total == 0) return $"{leftVar} == {rightVar}";
                return total > 0 ? $"{leftVar} == {rightVar} + {total}" : $"{leftVar} == {rightVar} - {Math.Abs(total)}";
            }

            // 2. Упрощение тождеств вида: (x + 1) == x + 1  -->  true
            string cleanLeftRight = s.Replace("(", "").Replace(")", "").Replace(" ", "");
            var eqParts = cleanLeftRight.Split(new[] { "==" }, StringSplitOptions.None);
            if (eqParts.Length == 2 && eqParts[0] == eqParts[1])
            {
                return "true";
            }

            // 3. Упрощение неравенств вида: (x + 10) > 15  -->  x > 5
            var matchLinearIneq = Regex.Match(s, @"^\(?\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*([+\-])\s*(\d+)\s*\)?\s*(>=|<=|>|<|==)\s*(-?\d+)$");
            if (matchLinearIneq.Success)
            {
                string v = matchLinearIneq.Groups[1].Value;
                int offset = int.Parse(matchLinearIneq.Groups[3].Value) * (matchLinearIneq.Groups[2].Value == "-" ? -1 : 1);
                string op = matchLinearIneq.Groups[4].Value;
                int rhs = int.Parse(matchLinearIneq.Groups[5].Value);
                int newRhs = rhs - offset;
                return $"{v} {op} {newRhs}";
            }

            // 4. Упрощение вида: (2*x + 5) > 15  -->  x > 5
            var matchMulLinear = Regex.Match(s, @"^\(?\s*(\d+)\s*\*\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*([+\-])\s*(\d+)\s*\)?\s*(>=|<=|>|<|==)\s*(-?\d+)$");
            if (matchMulLinear.Success)
            {
                int coef = int.Parse(matchMulLinear.Groups[1].Value);
                string v = matchMulLinear.Groups[2].Value;
                int offset = int.Parse(matchMulLinear.Groups[4].Value) * (matchMulLinear.Groups[3].Value == "-" ? -1 : 1);
                string op = matchMulLinear.Groups[5].Value;
                int rhs = int.Parse(matchMulLinear.Groups[6].Value);

                if (coef > 0 && (rhs - offset) % coef == 0)
                {
                    int finalRhs = (rhs - offset) / coef;
                    return $"{v} {op} {finalRhs}";
                }
            }

            return s;
        }

        /// <summary>
        /// Построение отрицания условия (¬B) в красивом виде
        /// </summary>
        private string NegateCondition(string cond)
        {
            string c = cond.Trim();
            if (c.Contains(">=")) return c.Replace(">=", "<");
            if (c.Contains("<=")) return c.Replace("<=", ">");
            if (c.Contains("!=")) return c.Replace("!=", "==");
            if (c.Contains("==")) return c.Replace("==", "!=");
            if (c.Contains(">")) return c.Replace(">", "<=");
            if (c.Contains("<")) return c.Replace("<", ">=");
            return $"¬({c})";
        }

        /// <summary>
        /// Перевод краткой математической записи на человеческий язык (для отчета)
        /// </summary>
        public string TranslateToHuman(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula)) return string.Empty;

            string text = formula
                .Replace("∧", " И ")
                .Replace("&&", " И ")
                .Replace("∨", " ИЛИ ")
                .Replace("||", " ИЛИ ")
                .Replace(">=", " больше или равно ")
                .Replace("<=", " меньше или равно ")
                .Replace("!=", " не равно ")
                .Replace("==", " равно ")
                .Replace(">", " больше ")
                .Replace("<", " меньше ")
                .Replace("¬", "НЕ ");

            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        #region Парсер псевдокода

        private List<Statement> ParseStatements(string code)
        {
            var result = new List<Statement>();
            int index = 0;
            string cleanCode = code.Trim();

            while (index < cleanCode.Length)
            {
                SkipWhitespaceAndSemicolons(cleanCode, ref index);
                if (index >= cleanCode.Length) break;

                // Проверяем, начинается ли оператор с "if"
                if (cleanCode.Substring(index).StartsWith("if", StringComparison.OrdinalIgnoreCase) &&
                    (index + 2 >= cleanCode.Length || !char.IsLetterOrDigit(cleanCode[index + 2])))
                {
                    index += 2;
                    string condition = ReadBalancedParentheses(cleanCode, ref index, '(', ')');
                    List<Statement> thenBranch = ReadBranchStatements(cleanCode, ref index);

                    SkipWhitespaceAndSemicolons(cleanCode, ref index);

                    if (!cleanCode.Substring(index).StartsWith("else", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FormatException("Оператор if должен содержать полную форму с веткой else.");
                    }
                    index += 4; // пропускаем "else"

                    List<Statement> elseBranch = ReadBranchStatements(cleanCode, ref index);
                    result.Add(new IfStatement(condition, thenBranch, elseBranch));
                }
                else
                {
                    // Читаем оператор присваивания до ';' или конца строки
                    int semiIndex = cleanCode.IndexOf(';', index);
                    int lineEndIndex = cleanCode.IndexOf('\n', index);

                    int end = semiIndex;
                    if (end == -1 || (lineEndIndex != -1 && lineEndIndex < end))
                        end = lineEndIndex;
                    if (end == -1)
                        end = cleanCode.Length;

                    string stmtText = cleanCode.Substring(index, end - index).Trim();
                    index = end + 1;

                    if (!string.IsNullOrWhiteSpace(stmtText))
                    {
                        result.Add(ParseAssignment(stmtText));
                    }
                }
            }

            return result;
        }

        private List<Statement> ReadBranchStatements(string code, ref int index)
        {
            SkipWhitespace(code, ref index);
            if (index < code.Length && code[index] == '{')
            {
                string blockContent = ReadBalancedParentheses(code, ref index, '{', '}');
                return ParseStatements(blockContent);
            }
            else
            {
                int semi = code.IndexOf(';', index);
                if (semi == -1) throw new FormatException("Пропущена ';' в ветке оператора if.");
                string singleStmt = code.Substring(index, semi - index).Trim();
                index = semi + 1;
                return new List<Statement> { ParseAssignment(singleStmt) };
            }
        }

        private AssignmentStatement ParseAssignment(string text)
        {
            // Проверяем, есть ли вообще :=
            if (!text.Contains(":="))
            {
                throw new FormatException($"Синтаксическая ошибка: в операторе '{text}' отсутствует знак присваивания ':='. Используйте формат 'x := выражение'.");
            }

            var parts = text.Split(new[] { ":=" }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new FormatException($"Некорректный оператор присваивания: '{text}'. Проверьте, что знак ':=' стоит один раз.");

            return new AssignmentStatement(parts[0], parts[1]);
        }

        private string ReadBalancedParentheses(string text, ref int index, char open, char close)
        {
            SkipWhitespace(text, ref index);
            if (index >= text.Length || text[index] != open)
                throw new FormatException($"Ожидался символ '{open}'.");

            int depth = 0;
            int start = index + 1;

            while (index < text.Length)
            {
                if (text[index] == open) depth++;
                else if (text[index] == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        string content = text.Substring(start, index - start);
                        index++;
                        return content.Trim();
                    }
                }
                index++;
            }

            throw new FormatException($"Не закрыта скобка '{close}'.");
        }

        private void SkipWhitespace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
        }

        private void SkipWhitespaceAndSemicolons(string text, ref int index)
        {
            while (index < text.Length && (char.IsWhiteSpace(text[index]) || text[index] == ';')) index++;
        }

        private bool NeedsParentheses(string expr)
        {
            return expr.Contains('+') || expr.Contains('-') || expr.Contains('*') || expr.Contains('/');
        }

        private string NormalizeLogicalOperators(string input)
        {
            return input
                .Replace("&&", "∧")
                .Replace("||", "∨")
                .Replace(" and ", " ∧ ", StringComparison.OrdinalIgnoreCase)
                .Replace(" or ", " ∨ ", StringComparison.OrdinalIgnoreCase);
        }

        private string FormatBranch(List<Statement> branch)
        {
            return string.Join("; ", branch.Select(s => s.RawText));
        }

        #endregion

        #region Выполнение сценария над ночником (интерпретатор сверху вниз)

        /// <summary>
        /// Выполняет скрипт сверху вниз на заданных входных параметрах и возвращает итоговую яркость ночника (0..100)
        /// </summary>
        public int ExecuteScenario(string code, Dictionary<string, double> variables)
        {
            List<Statement> statements = ParseStatements(code);
            ExecuteStatementList(statements, variables);

            // Ищем переменную, отвечающую за итоговую яркость ночника (по приоритету имени)
            string[] targetNames = { "brightness", "max", "y", "x" };
            foreach (string name in targetNames)
            {
                if (variables.TryGetValue(name, out double val))
                {
                    // Если в пресете вернулся код ошибки (например, -999), выключаем ночник (0)
                    if (val < 0) return 0;
                    return (int)Math.Round(val);
                }
            }

            return 0;
        }

        private void ExecuteStatementList(List<Statement> statements, Dictionary<string, double> vars)
        {
            // В отличие от wp, выполнение кода идет строго СВЕРХУ ВНИЗ (от 0 до Count - 1)
            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i] is AssignmentStatement assign)
                {
                    double value = EvaluateMathExpression(assign.Expression, vars);
                    vars[assign.Variable] = value;
                }
                else if (statements[i] is IfStatement ifStmt)
                {
                    bool conditionMet = EvaluateBooleanCondition(ifStmt.Condition, vars);
                    if (conditionMet)
                        ExecuteStatementList(ifStmt.ThenBranch, vars);
                    else
                        ExecuteStatementList(ifStmt.ElseBranch, vars);
                }
            }
        }

        private double EvaluateMathExpression(string expr, Dictionary<string, double> vars)
        {
            string prepared = SubstituteVariables(expr, vars);

            // Поддержка функции abs(...)
            prepared = Regex.Replace(prepared, @"abs\s*\(([^)]+)\)", m =>
            {
                double inner = Convert.ToDouble(new DataTable().Compute(m.Groups[1].Value, ""));
                return Math.Abs(inner).ToString(CultureInfo.InvariantCulture);
            }, RegexOptions.IgnoreCase);

            object result = new DataTable().Compute(prepared, "");
            double doubleResult = Convert.ToDouble(result, CultureInfo.InvariantCulture);

            if (double.IsInfinity(doubleResult) || double.IsNaN(doubleResult))
                throw new DivideByZeroException("Сценарий привел к делению на ноль (нарушено ОДЗ wp)!");

            return doubleResult;
        }

        private bool EvaluateBooleanCondition(string condition, Dictionary<string, double> vars)
        {
            string prepared = SubstituteVariables(condition, vars)
                .Replace("==", "=")
                .Replace("!=", "<>")
                .Replace("&&", " AND ")
                .Replace("∧", " AND ")
                .Replace("||", " OR ")
                .Replace("∨", " OR ");

            object result = new DataTable().Compute(prepared, "");
            return Convert.ToBoolean(result);
        }

        private string SubstituteVariables(string text, Dictionary<string, double> vars)
        {
            string result = text;
            foreach (var kvp in vars)
            {
                string pattern = $@"\b{Regex.Escape(kvp.Key)}\b";
                string numStr = kvp.Value.ToString(CultureInfo.InvariantCulture);
                result = Regex.Replace(result, pattern, $"({numStr})");
            }
            return result;
        }

        #endregion
    }
}
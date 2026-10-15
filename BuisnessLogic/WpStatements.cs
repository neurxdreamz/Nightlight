using System;
using System.Collections.Generic;
using System.Text;

namespace BuisnessLogic
{
    public class WpStatements
    {
        public abstract class Statement
        {
            public abstract string RawText { get; }
        }

        /// <summary>
        /// Оператор присваивания: x := expr
        /// </summary>
        public class AssignmentStatement : Statement
        {
            public string Variable { get; }
            public string Expression { get; }
            public override string RawText => $"{Variable} := {Expression}";

            public AssignmentStatement(string variable, string expression)
            {
                Variable = variable.Trim();
                Expression = expression.Trim();
            }
        }

        /// <summary>
        /// Оператор ветвления: if (B) { S1 } else { S2 }
        /// </summary>
        public class IfStatement : Statement
        {
            public string Condition { get; }
            public List<Statement> ThenBranch { get; }
            public List<Statement> ElseBranch { get; }

            public override string RawText => $"if ({Condition}) {{ ... }} else {{ ... }}";

            public IfStatement(string condition, List<Statement> thenBranch, List<Statement> elseBranch)
            {
                Condition = condition.Trim();
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }
        }
    }
}

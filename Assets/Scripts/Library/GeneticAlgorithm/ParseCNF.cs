using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library.GeneticAlgorithm
{
    [CreateAssetMenu]
    public class ParseCNF : ScriptableObject
    {
        [Serializable]
        public class Delimiter
        {
            public char open, close, escape;

            public static readonly Delimiter _default =
                new Delimiter { open = '(', close = ')', escape = '\\' };
        }

        [Serializable]
        public class Expression
        {
            public string value;
            public List<string> clauses = new List<string>();

            public List<char> literals = new List<char>();
        }

        private const string _or = "||";
        private const string _and = "&&";
        private const string _not = "!";

        [TextArea]
        public string cnfExpression;

        public Delimiter delimiter;

        [Space]
        public char or;
        public char and;
        public char not;

        [Space, Space]
        public List<Expression> parsedExpressions = new List<Expression>();

        [Space, Space]
        public List<Library.Expression> expressions = new List<Library.Expression>();

        [ContextMenu("Parse")]
        public void Parse()
        {
            parsedExpressions.Clear();
            expressions.Clear();

            var currentClause = new List<Clause>();

            var clauseStack = new List<string>(1);
            var clauses = new List<string>();

            var nestLevel = 0;
            var index = 0;
            while (index < cnfExpression.Length)
            {
                if (cnfExpression[index] == delimiter.escape)
                {
                    index += 2;
                    continue;
                }

                foreach (var clause in currentClause)
                    clause.stringValue += cnfExpression[index];

                for (var i = 0; i < clauseStack.Count; ++i)
                    clauseStack[i] += cnfExpression[index];

                if (cnfExpression[index] == delimiter.open)
                {
                    clauseStack.Add(cnfExpression[index].ToString());

                    ++nestLevel;

                    currentClause.Add(
                        new Clause
                        {
                            expressionObjects = new List<ExpressionObject>
                            {
                                new Library.Delimiter{ type = Library.Delimiter.Type.Open }
                            },
                            stringValue = delimiter.open.ToString(),
                        });

                    if (nestLevel > parsedExpressions.Count)
                    {
                        parsedExpressions.Add(new Expression());

                        expressions.Add(
                            new Library.Expression
                            {
                                expressionObjects = new List<ExpressionObject>()
                            });
                    }
                }
                else if (cnfExpression[index] == delimiter.close)
                {
                    clauses.Add(clauseStack.Last());
                    clauseStack.RemoveAt(clauseStack.Count - 1);

                    parsedExpressions[nestLevel - 1].clauses.Add(clauses.Last());

                    if (currentClause.Count >= 2)
                        currentClause[currentClause.Count - 2].expressionObjects.Add(expressions.Last());

                    currentClause.Last().expressionObjects.Add(
                        new Library.Delimiter { type = Library.Delimiter.Type.Close });

                    expressions[nestLevel - 1].expressionObjects.Add(currentClause.Last());
                    expressions[nestLevel - 1].stringValue += currentClause.Last().stringValue;

                    currentClause.Remove(currentClause.Last());

                    --nestLevel;
                }
                else if (
                    cnfExpression[index] != or &&
                    cnfExpression[index] != and &&
                    cnfExpression[index] != not &&
                    cnfExpression[index] != ' ')
                {
                    Variable existingVariable = null;
                    foreach (var expression in expressions)
                    {
                        foreach (var expressionObject in expression.expressionObjects)
                        {
                            var variable = expressionObject as Variable;
                            if (variable == null || variable.stringValue != cnfExpression[index].ToString())
                                continue;

                            existingVariable = variable;
                            break;
                        }
                    }

                    if (existingVariable == null)
                        currentClause.Last().expressionObjects.Add(
                            new Variable { stringValue = cnfExpression[index].ToString() });
                    else
                        currentClause.Last().expressionObjects.Add(existingVariable);

                    if (!parsedExpressions[nestLevel - 1].literals.Contains(cnfExpression[index]))
                    {
                        parsedExpressions[nestLevel - 1].literals.Add(cnfExpression[index]);
                    }
                }
                else if (cnfExpression[index] != ' ')
                {
                    Operator newOperator;

                    if (cnfExpression[index] == or)
                        newOperator = new Or { stringValue = cnfExpression[index].ToString() };
                    else if (cnfExpression[index] == and)
                        newOperator = new And { stringValue = cnfExpression[index].ToString() };
                    else
                        newOperator = new Not { stringValue = cnfExpression[index].ToString() };

                    var trueNestLevel = nestLevel - 1;
                    if (trueNestLevel < 0)
                        trueNestLevel = 0;

                    if (currentClause.Count != 0)
                        currentClause.Last().expressionObjects.Add(newOperator);
                    else
                    {
                        expressions[trueNestLevel].expressionObjects.Add(newOperator);
                        expressions[trueNestLevel].stringValue += " " + cnfExpression[index] + " ";
                    }
                }

                ++index;
            }

            foreach (var parsedExpression in parsedExpressions)
            {
                foreach (var clause in parsedExpression.clauses)
                    parsedExpression.value += clause + " " + and + " ";

                parsedExpression.value = parsedExpression.value.Trim(' ', and);
            }
        }

        private void ConvertToDefault()
        {
            foreach (var parsedExpression in parsedExpressions)
            {
                for (var i = 0; i < parsedExpression.clauses.Count; ++i)
                {
                    parsedExpression.clauses[i] =
                        parsedExpression.clauses[i].Replace(delimiter.open, Delimiter._default.open);
                    parsedExpression.clauses[i] =
                        parsedExpression.clauses[i].Replace(delimiter.close, Delimiter._default.close);

                    parsedExpression.clauses[i] = parsedExpression.clauses[i].Replace(and.ToString(), _and);
                    parsedExpression.clauses[i] = parsedExpression.clauses[i].Replace(or.ToString(), _or);
                    parsedExpression.clauses[i] = parsedExpression.clauses[i].Replace(not.ToString(), _not);
                }
            }
        }
    }
}

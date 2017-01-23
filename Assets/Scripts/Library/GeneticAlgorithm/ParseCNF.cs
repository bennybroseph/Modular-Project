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
        public class DelimiterChar
        {
            public char open, close, escape;
        }

        [TextArea]
        public string cnfExpression;

        public DelimiterChar delimiter;

        [Space]
        public char or;
        public char and;
        public char not;

        [Space, Space]
        public Expression expression;

        [ContextMenu("Parse")]
        public void Parse()
        {
            expression.expressionObjects.Clear();

            var currentClause = new List<Clause>();

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

                if (cnfExpression[index] == delimiter.open)
                {
                    ++nestLevel;

                    currentClause.Add(
                        new Clause
                        {
                            expressionObjects = new List<ExpressionObject>
                            {
                                new Delimiter
                                {
                                    stringValue = cnfExpression[index].ToString(),
                                    type = Delimiter.Type.Open
                                }
                            },
                            stringValue = delimiter.open.ToString(),
                        });

                    if (nestLevel == 1)
                        expression.expressionObjects.Add(currentClause.Last());
                }
                else if (cnfExpression[index] == delimiter.close)
                {
                    if (currentClause.Count >= 2)
                        currentClause[currentClause.Count - 2].expressionObjects.Add(currentClause.Last());

                    currentClause.Last().expressionObjects.Add(
                        new Delimiter
                        {
                            stringValue = cnfExpression[index].ToString(),
                            type = Delimiter.Type.Close
                        });

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
                    foreach (var variable in expression.GetVariables())
                    {
                        if (variable.stringValue == cnfExpression[index].ToString())
                            existingVariable = variable;
                    }

                    if (existingVariable == null)
                        currentClause.Last().expressionObjects.Add(
                            new Variable { stringValue = cnfExpression[index].ToString() });
                    else
                        currentClause.Last().expressionObjects.Add(existingVariable);
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

                    if (currentClause.Count != 0)
                        currentClause.Last().expressionObjects.Add(newOperator);
                    else
                    {
                        expression.expressionObjects.Add(newOperator);
                        expression.stringValue += " " + cnfExpression[index] + " ";
                    }
                }

                ++index;
            }

            expression.UpdateStringValue();
        }
    }
}

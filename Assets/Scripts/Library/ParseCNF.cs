using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[CreateAssetMenu]
public class ParseCNF : ScriptableObject
{
    [TextArea]
    public string expression;

    [Serializable]
    public class Delimiter
    {
        public char open, close, escape;
    }

    [Serializable]
    public class Expression
    {
        public List<string> clauses = new List<string>();

        public List<char> literals = new List<char>();
    }

    public Delimiter delimiter;

    [Space]
    public char or;
    public char and;
    public char not;

    [Space, Space]
    public List<Expression> expressions = new List<Expression>();

    [ContextMenu("Parse")]
    private void Parse()
    {
        expressions.Clear();

        var clauseStack = new List<string>();
        var clauses = new List<string>();

        var nestLevel = 0;
        var index = 0;
        while (index < expression.Length)
        {
            if (expression[index] == delimiter.escape)
            {
                index += 2;
                continue;
            }

            for (var i = 0; i < clauseStack.Count; ++i)
                clauseStack[i] += expression[index];

            if (expression[index] == delimiter.open)
            {
                clauseStack.Add(expression[index].ToString());

                ++nestLevel;

                if (nestLevel > expressions.Count)
                    expressions.Add(new Expression());
            }
            else if (expression[index] == delimiter.close)
            {
                clauses.Add(clauseStack.Last());
                clauseStack.RemoveAt(clauseStack.Count - 1);

                expressions[nestLevel - 1].clauses.Add(clauses.Last());

                --nestLevel;
            }
            else if (
                expression[index] != or &&
                expression[index] != and &&
                expression[index] != not &&
                expression[index] != ' ')
            {
                if (!expressions[nestLevel - 1].literals.Contains(expression[index]))
                    expressions[nestLevel - 1].literals.Add(expression[index]);
            }

            ++index;
        }
    }
}

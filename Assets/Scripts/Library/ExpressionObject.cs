namespace Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;

    [Serializable]
    public abstract class ExpressionObject
    {
        public string stringValue;

        // Attempts to deep copy the expression object
        public abstract ExpressionObject Copy();
    }

    [Serializable]
    public class Expression : ExpressionObject
    {
        private static bool m_YieldEvaluation = true;
        private static bool m_PauseEvaluation = true;

        public delegate void OnYieldChanged(bool newValue);
        public static event OnYieldChanged onYieldChanged;

        public delegate void OnPauseChanged(bool newValue);
        public static event OnPauseChanged onPauseChanged;

        public List<ExpressionObject> expressionObjects = new List<ExpressionObject>();

        public List<ExpressionObject> currentlyEvaluatedObjects = new List<ExpressionObject>();

        public static bool yieldEvaluation
        {
            get { return m_YieldEvaluation; }
            set
            {
                m_YieldEvaluation = value;
                if (m_YieldEvaluation == false)
                    pauseEvaluation = false;

                if (onYieldChanged != null)
                    onYieldChanged.Invoke(value);
            }
        }
        public static bool pauseEvaluation
        {
            get { return m_PauseEvaluation; }
            set
            {
                m_PauseEvaluation = value;

                if (onPauseChanged != null)
                    onPauseChanged.Invoke(value);
            }
        }

        public IEnumerator Evaluate()
        {
            currentlyEvaluatedObjects.Clear();

            for (var i = 0; i < expressionObjects.Count; i++)
            {
                var expression = expressionObjects[i] as Expression;
                if (expression == null)
                    continue;

                currentlyEvaluatedObjects.Clear();
                currentlyEvaluatedObjects.Add(expression);

                if (yieldEvaluation)
                    yield return expression.Evaluate();
                else
                {
                    var enumerator = expression.Evaluate();
                    while (enumerator.MoveNext()) { }
                }

                while (pauseEvaluation)
                    yield return null;

                expressionObjects[i] = expression.expressionObjects.First();
            }

            var foundIndex = expressionObjects.FindIndex(obj => obj is Not);
            while (foundIndex != -1)
            {
                var nextObject = foundIndex + 1 < expressionObjects.Count ?
                    expressionObjects[foundIndex + 1] : null;

                currentlyEvaluatedObjects.Clear();
                currentlyEvaluatedObjects.AddRange(
                    new[] { expressionObjects[foundIndex], nextObject });

                if (yieldEvaluation)
                    yield return null;
                while (pauseEvaluation)
                    yield return null;

                var newObject = ((Not)expressionObjects[foundIndex]).Operation(nextObject);

                expressionObjects.Remove(expressionObjects[foundIndex]);
                expressionObjects.Insert(foundIndex, newObject);

                expressionObjects.Remove(nextObject);

                foundIndex = expressionObjects.FindIndex(obj => obj is Not);
            }

            foundIndex = expressionObjects.FindIndex(obj => obj is ConditionalOperator);
            while (foundIndex != -1)
            {
                var previousObject = foundIndex - 1 >= 0 ?
                    expressionObjects[foundIndex - 1] : null;

                var nextObject = foundIndex + 1 < expressionObjects.Count ?
                    expressionObjects[foundIndex + 1] : null;

                currentlyEvaluatedObjects.Clear();
                currentlyEvaluatedObjects.AddRange(
                    new[] { previousObject, expressionObjects[foundIndex], nextObject });

                if (yieldEvaluation)
                    yield return null;
                while (pauseEvaluation)
                    yield return null;

                var newObject =
                    ((ConditionalOperator)expressionObjects[foundIndex]).
                        Operation(previousObject, nextObject);

                expressionObjects.Remove(expressionObjects[foundIndex]);
                expressionObjects.Insert(foundIndex, newObject);

                if (!(previousObject is Delimiter))
                    expressionObjects.Remove(previousObject);
                expressionObjects.Remove(nextObject);

                foundIndex = expressionObjects.FindIndex(obj => obj is ConditionalOperator);
            }

            if (yieldEvaluation)
                yield return null;
            while (pauseEvaluation)
                yield return null;

            var delimiters = expressionObjects.OfType<Delimiter>().ToList();
            foreach (var delimiter in delimiters)
                expressionObjects.Remove(delimiter);

            currentlyEvaluatedObjects.Clear();

            UpdateStringValue();

            yield return null;

            while (pauseEvaluation)
                yield return null;
        }

        public IEnumerable<Variable> GetVariables()
        {
            var variables = new List<Variable>();
            foreach (var nestedExpression in expressionObjects.OfType<Expression>())
                foreach (var variable in nestedExpression.GetVariables())
                    if (!variables.Contains(variable))
                        variables.Add(variable);

            foreach (var variable in expressionObjects.OfType<Variable>())
                if (!variables.Contains(variable))
                    variables.Add(variable);

            return variables;
        }

        public void UpdateStringValue()
        {
            stringValue = string.Empty;

            foreach (var nestedExpression in expressionObjects.OfType<Expression>())
                nestedExpression.UpdateStringValue();

            for (var i = 0; i < expressionObjects.Count; ++i)
            {
                var nextObject =
                    i + 1 < expressionObjects.Count ? expressionObjects[i + 1] : null;

                stringValue += expressionObjects[i].stringValue;

                if (expressionObjects[i] is Delimiter || nextObject is Delimiter ||
                    expressionObjects[i] is Not)
                    continue;

                stringValue += " ";
            }

            stringValue = stringValue.Trim(' ');
        }

        public override ExpressionObject Copy()
        {
            var copyObjects =
                expressionObjects.Select(expressionObject => expressionObject.Copy()).ToList();

            return new Expression { expressionObjects = copyObjects };
        }
    }

    [Serializable]
    public class Clause : Expression
    {
        public override ExpressionObject Copy()
        {
            var copyObjects =
                 expressionObjects.Select(expressionObject => expressionObject.Copy()).ToList();

            return new Clause { expressionObjects = copyObjects };
        }
    }

    [Serializable]
    public class Variable : ExpressionObject
    {
        public object value;

        // This is only a shallow copy
        public override ExpressionObject Copy()
        {
            return new Variable { value = value, stringValue = stringValue };
        }
    }

    [Serializable]
    public abstract class Operator : ExpressionObject { }

    [Serializable]
    public abstract class ConditionalOperator : Operator
    {
        public ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs)
        {
            var varA = lhs as Variable;
            var varB = rhs as Variable;

            if (varA == null || varB == null ||
                varA.value == null || varB.value == null)
            {
                var logText =
                    "Cannot perform " + GetType() + " operation on " + lhs.stringValue + " and " +
                    rhs.stringValue + "of type " + lhs.GetType() + " and " + rhs.GetType() + " respectively.";

                if (varA != null && varA.value == null)
                    logText += "varA.value was null";
                if (varB != null && varB.value == null)
                    logText += "varB.value was null";

                Debug.LogError(logText);

                return null;
            }

            var valueA = (bool)varA.value;
            var valueB = (bool)varB.value;

            var result = CustomOperation(valueA, valueB);

            return new Variable { stringValue = result.ToString(), value = result };
        }

        protected abstract bool CustomOperation(bool lhs, bool rhs);
    }

    [Serializable]
    public class Or : ConditionalOperator
    {
        protected override bool CustomOperation(bool lhs, bool rhs)
        {
            return lhs || rhs;
        }

        public override ExpressionObject Copy()
        {
            return new Or { stringValue = stringValue };
        }
    }

    [Serializable]
    public class And : ConditionalOperator
    {
        protected override bool CustomOperation(bool lhs, bool rhs)
        {
            return lhs && rhs;
        }

        public override ExpressionObject Copy()
        {
            return new And { stringValue = stringValue };
        }
    }

    public class Not : Operator
    {
        public ExpressionObject Operation(ExpressionObject rhs)
        {
            var varB = rhs as Variable;

            if (varB == null || varB.value == null)
            {
                Debug.LogError("Incorrect rhs for Not operator");
                return null;
            }

            var valueB = (bool)varB.value;

            var result = !valueB;

            return new Variable { stringValue = result.ToString(), value = result };
        }

        public override ExpressionObject Copy()
        {
            return new Not { stringValue = stringValue };
        }
    }

    [Serializable]
    public abstract class ArithmeticOperator : Operator { }

    [Serializable]
    public class Delimiter : ExpressionObject
    {
        public enum Type
        {
            Open,
            Close,
            Escape,
        }

        public Type type;

        public override ExpressionObject Copy()
        {
            return new Delimiter { type = type, stringValue = stringValue };
        }
    }
}
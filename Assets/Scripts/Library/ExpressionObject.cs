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
        public List<ExpressionObject> expressionObjects = new List<ExpressionObject>();

        public List<ExpressionObject> currentlyEvaluatedObjects = new List<ExpressionObject>();

        public static bool yieldEvaluation = true;
        public static bool pauseEvaluation = true;

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

            //ExpressionObject previousObject = null;
            //for (var i = 0; i < expressionObjects.Count; ++i)
            //{
            //    var nextObject = i + 1 < expressionObjects.Count ?
            //        expressionObjects[i + 1] : null;

            //    currentlyEvaluatedObjects.Clear();
            //    currentlyEvaluatedObjects.AddRange(
            //        new[] { previousObject, expressionObjects[i], nextObject });

            //    if (yieldEvaluation)
            //        yield return null;
            //    while (pauseEvaluation)
            //        yield return null;

            //    if (expressionObjects[i] is ConditionalOperator)
            //    {
            //        var newObject =
            //            ((ConditionalOperator)expressionObjects[i]).Operation(previousObject, nextObject);

            //        expressionObjects.Remove(expressionObjects[i]);
            //        expressionObjects.Insert(i, newObject);

            //        if (!(previousObject is Delimiter))
            //            expressionObjects.Remove(previousObject);
            //        expressionObjects.Remove(nextObject);

            //        i--;
            //    }

            //    previousObject = expressionObjects[i];
            //}

            if (yieldEvaluation)
                yield return null;
            while (pauseEvaluation)
                yield return null;

            var delimiters = expressionObjects.OfType<Delimiter>().ToList();
            foreach (var delimiter in delimiters)
                expressionObjects.Remove(delimiter);

            currentlyEvaluatedObjects.Clear();

            UpdateStringValue();

            if (yieldEvaluation)
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
        public abstract ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs);
    }

    [Serializable]
    public class Or : ConditionalOperator
    {
        public override ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs)
        {
            var varA = lhs as Variable;
            var varB = rhs as Variable;

            if (varA != null && varB != null)
            {
                var valueA = (bool)varA.value;
                var valueB = (bool)varB.value;

                var result = valueA || valueB;

                return new Variable { stringValue = result.ToString(), value = result };
            }

            Debug.LogError("Cannot perform Or operation on " + lhs.stringValue + " and " + rhs.stringValue);
            return null;
        }

        public override ExpressionObject Copy()
        {
            return new Or { stringValue = stringValue };
        }
    }

    [Serializable]
    public class And : ConditionalOperator
    {
        public override ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs)
        {
            var varA = lhs as Variable;
            var varB = rhs as Variable;

            if (varA != null && varB != null)
            {
                var valueA = (bool)varA.value;
                var valueB = (bool)varB.value;

                var result = valueA && valueB;

                return new Variable { stringValue = result.ToString(), value = result };
            }

            Debug.LogError("Cannot perform And operation on " + lhs.stringValue + " and " + rhs.stringValue);
            return null;
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

            if (varB != null)
            {
                var valueB = (bool)varB.value;

                var result = !valueB;

                return new Variable { stringValue = result.ToString(), value = result };
            }

            Debug.LogError("Incorrect rhs for Not operator");
            return null;
        }

        public override ExpressionObject Copy()
        {
            return new Not { stringValue = stringValue };
        }
    }

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
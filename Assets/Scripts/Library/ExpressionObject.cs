namespace Library
{
    using System;
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

        public void Evaluate()
        {
            foreach (var nestedExpression in expressionObjects.OfType<Expression>())
                nestedExpression.Evaluate();

            ExpressionObject previousObject = null;
            for (var i = 0; i < expressionObjects.Count; ++i)
            {
                var nextObject = i + 1 < expressionObjects.Count ?
                    expressionObjects[i + 1] : null;

                if (expressionObjects[i] is Operator)
                {
                    var newObject = ((Operator)expressionObjects[i]).Operation(previousObject, nextObject);

                    if (!(previousObject is Delimiter))
                        expressionObjects.Remove(previousObject);
                    expressionObjects.Remove(expressionObjects[i]);
                    expressionObjects.Remove(nextObject);

                    expressionObjects.Insert(i, newObject);
                }

                previousObject = expressionObjects[i];
            }
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
        public List<ExpressionObject> expressionObjects = new List<ExpressionObject>();

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
            return new Variable { value = value };
        }
    }

    [Serializable]
    public abstract class Operator : ExpressionObject
    {
        public abstract ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs);
    }

    [Serializable]
    public class Or : Operator
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
    public class And : Operator
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
            return new Or { stringValue = stringValue };
        }
    }

    public class Not : Operator
    {
        public override ExpressionObject Operation(ExpressionObject lhs, ExpressionObject rhs)
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
            return new Or { stringValue = stringValue };
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
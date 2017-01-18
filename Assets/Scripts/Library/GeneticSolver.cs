namespace Library
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEngine;

    using Random = UnityEngine.Random;

    public class GeneticSolver : MonoBehaviour
    {
        [Serializable]
        private class Chromosome
        {
            public bool value;
            public string name;
        }
        [Serializable]
        private class Candidate
        {
            public List<Chromosome> chromosomes = new List<Chromosome>();
        }
        [Serializable]
        private class Generation
        {
            public List<Candidate> candidates = new List<Candidate>();
        }

        [Serializable]
        private class GeneticEquation
        {
            public List<Generation> generations = new List<Generation>();

            public Expression expression;

            public bool solved;
        }

        [SerializeField]
        private ParseCNF m_ParseCNF;

        [SerializeField]
        private List<GeneticEquation> m_GeneticEquations = new List<GeneticEquation>();

        // Use this for initialization
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond);

            m_ParseCNF.Parse();
            foreach (var expression in m_ParseCNF.expressions)
            {
                var newEquation =
                    new GeneticEquation
                    {
                        expression = expression
                    };

                var newGeneration = new Generation();

                newGeneration.candidates.Add(new Candidate());
                newGeneration.candidates.Add(new Candidate());

                foreach (var candidate in newGeneration.candidates)
                    foreach (var variable in expression.expressionObjects.OfType<Variable>())
                        candidate.chromosomes.Add(new Chromosome
                        {
                            value = Random.Range(0, 2) == 1,
                            name = variable.stringValue,
                        });

                newEquation.generations.Add(newGeneration);
                m_GeneticEquations.Add(newEquation);
            }

            StartCoroutine(RunSolver());
        }

        private IEnumerator RunSolver()
        {
            while (true)
            {
                foreach (var equation in m_GeneticEquations)
                {
                    equation.solved =
                        equation.generations.Last().candidates.Any(
                            candidate => Evaluate(equation.expression, candidate));

                    if (equation.solved)
                    {
                        Debug.Log("Solved");
                        Debug.Log("Generations: " + equation.generations.Count);
                        Debug.Log(equation.expression.stringValue);

                        var solvingCandidate =
                            equation.generations.Last().candidates.First(
                                candidate => Evaluate(equation.expression, candidate));

                        foreach (var chromosome in solvingCandidate.chromosomes)
                            Debug.Log(chromosome.name + " = " + chromosome.value);

                        continue;
                    }

                    var newGeneration = new Generation();

                    newGeneration.candidates.Add(new Candidate());
                    newGeneration.candidates.Add(new Candidate());

                    foreach (var candidate in newGeneration.candidates)
                        foreach (var variable in equation.expression.expressionObjects.OfType<Variable>())
                            candidate.chromosomes.Add(new Chromosome
                            {
                                value = Random.Range(0, 2) == 1,
                                name = variable.stringValue
                            });

                    equation.generations.Add(newGeneration);

                    yield return new WaitForSeconds(1f);
                }
            }
        }

        private bool Evaluate(Expression expression, Candidate candidate)
        {
            expression = expression.Copy() as Expression;

            var variables = expression.expressionObjects.OfType<Variable>().ToList();

            foreach (var variable in variables)
                foreach (var chromosome in candidate.chromosomes)
                    if (chromosome.name == variable.stringValue)
                        variable.value = chromosome.value;

            expression.Evaluate();

            return false;
        }
    }
}

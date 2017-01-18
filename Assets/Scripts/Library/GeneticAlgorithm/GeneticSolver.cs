using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library.GeneticAlgorithm
{
    using Random = UnityEngine.Random;

    [Serializable]
    public class Chromosome
    {
        public bool value;
        public string name;
    }
    [Serializable]
    public class Candidate
    {
        public List<Chromosome> chromosomes = new List<Chromosome>();
    }
    [Serializable]
    public class Generation
    {
        public List<Candidate> candidates = new List<Candidate>();
    }

    [Serializable]
    public class GeneticEquation
    {
        public List<Generation> generations = new List<Generation>();

        public Expression expression;

        public bool solved;
    }

    public class GeneticSolver : MonoBehaviour
    {
        

        [SerializeField]
        private ParseCNF m_ParseCNF;

        [SerializeField]
        private List<GeneticEquation> m_GeneticEquations = new List<GeneticEquation>();

        public List<GeneticEquation> geneticEquations
        {
            get { return m_GeneticEquations; }
        }

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
                    foreach (var variable in expression.GetVariables())
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

                        yield return new WaitForSeconds(1f);

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
            if (expression == null)
                return false;

            foreach (var variable in expression.GetVariables())
                foreach (var chromosome in candidate.chromosomes)
                    if (chromosome.name == variable.stringValue)
                        variable.value = chromosome.value;

            var result = expression.Evaluate() as Variable;

            return (bool)result.value;
        }
    }
}

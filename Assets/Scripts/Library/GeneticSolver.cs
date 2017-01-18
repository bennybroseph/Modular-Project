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
            public char name;
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
        private class Equation
        {
            public List<Generation> generations = new List<Generation>();

            public string value;
            public List<char> literals = new List<char>();

            public bool solved;
        }

        [SerializeField]
        private ParseCNF m_ParseCNF;

        [SerializeField]
        private List<Equation> m_Equations = new List<Equation>();

        // Use this for initialization
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond);

            foreach (var expression in m_ParseCNF.parsedExpressions)
            {
                var newEquation =
                    new Equation
                    {
                        value = expression.value,
                        literals = expression.literals
                    };

                var newGeneration = new Generation();

                newGeneration.candidates.Add(new Candidate());
                newGeneration.candidates.Add(new Candidate());

                foreach (var candidate in newGeneration.candidates)
                    foreach (var literal in expression.literals)
                        candidate.chromosomes.Add(new Chromosome
                        {
                            value = Random.Range(0, 2) == 1,
                            name = literal
                        });

                newEquation.generations.Add(newGeneration);
                m_Equations.Add(newEquation);
            }

            StartCoroutine(RunSolver());
        }

        private IEnumerator RunSolver()
        {
            while (!m_Equations.All(equation => equation.solved))
            {
                foreach (var equation in m_Equations)
                {
                    equation.solved =
                        equation.generations.Last().candidates.Any(
                            candidate => Evaluate(equation.value, candidate.chromosomes.ToArray()));

                    if (equation.solved)
                    {
                        Debug.Log("Solved");
                        Debug.Log("Generations: " + equation.generations.Count);
                        Debug.Log(equation.value);

                        var solvingCandidate =
                            equation.generations.Last().candidates.First(
                                candidate => Evaluate(equation.value, candidate.chromosomes.ToArray()));

                        foreach (var chromosome in solvingCandidate.chromosomes)
                            Debug.Log(chromosome.name + " = " + chromosome.value);

                        continue;
                    }

                    var newGeneration = new Generation();

                    newGeneration.candidates.Add(new Candidate());
                    newGeneration.candidates.Add(new Candidate());

                    foreach (var candidate in newGeneration.candidates)
                        foreach (var literal in equation.literals)
                            candidate.chromosomes.Add(new Chromosome
                            {
                                value = Random.Range(0, 2) == 1,
                                name = literal
                            });

                    equation.generations.Add(newGeneration);

                    yield return new WaitForSeconds(1f);
                }
            }


        }

        private bool Evaluate(string expression, params Chromosome[] variables)
        {
            var previousValue = false;

            var not = false;

            var or = false;
            var and = false;

            var index = 0;
            while (index < expression.Length)
            {
                if (expression[index] == m_ParseCNF.not)
                    not = true;
                else if (expression[index] == m_ParseCNF.or)
                    or = true;
                else if (expression[index] == m_ParseCNF.and)
                    and = true;
                else if (variables.Any(x => x.name == expression[index]))
                {
                    var currentValue = variables.First(x => x.name == expression[index]).value;
                    if (not)
                        currentValue = !currentValue;
                    not = false;

                    if (or)
                        previousValue = previousValue || currentValue;
                    else if (and)
                        previousValue = previousValue && currentValue;
                    else
                        previousValue = currentValue;

                    or = false;
                    and = false;
                }

                ++index;
            }

            return previousValue;
        }
    }
}

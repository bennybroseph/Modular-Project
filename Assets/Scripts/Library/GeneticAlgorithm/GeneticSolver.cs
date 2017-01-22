using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library.GeneticAlgorithm
{
    using UnityEngine.UI;
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
        private GeneticEquation m_GeneticEquation;

        public GeneticEquation geneticEquation
        {
            get { return m_GeneticEquation; }
        }

        // Use this for initialization
        private void Awake()
        {
            Random.InitState(DateTime.Now.Millisecond);

            m_ParseCNF.Parse();
            m_GeneticEquation =
                new GeneticEquation
                {
                    expression = m_ParseCNF.expression
                };

            var newGeneration = new Generation();

            newGeneration.candidates.Add(new Candidate());
            newGeneration.candidates.Add(new Candidate());

            foreach (var candidate in newGeneration.candidates)
                foreach (var variable in m_ParseCNF.expression.GetVariables())
                    candidate.chromosomes.Add(new Chromosome
                    {
                        value = Random.Range(0, 2) == 1,
                        name = variable.stringValue,
                    });

            m_GeneticEquation.generations.Add(newGeneration);


            StartCoroutine(RunSolver());
        }

        private IEnumerator RunSolver()
        {
            while (true)
            {
                foreach (var candidate in m_GeneticEquation.generations.Last().candidates)
                    yield return Evaluate(m_GeneticEquation, candidate);

                if (m_GeneticEquation.solved)
                {
                    yield return null;

                    continue;
                }

                var newGeneration = new Generation();

                newGeneration.candidates.Add(new Candidate());
                newGeneration.candidates.Add(new Candidate());

                foreach (var candidate in newGeneration.candidates)
                    foreach (var variable in m_GeneticEquation.expression.expressionObjects.OfType<Variable>())
                        candidate.chromosomes.Add(new Chromosome
                        {
                            value = Random.Range(0, 2) == 1,
                            name = variable.stringValue
                        });

                m_GeneticEquation.generations.Add(newGeneration);

                yield return new WaitForSeconds(1f);

            }
        }

        private IEnumerator Evaluate(GeneticEquation equation, Candidate candidate)
        {
            var expression = equation.expression.Copy() as Expression;
            if (expression == null)
                yield break;

            var expressionSolverVisualizer = new GameObject().AddComponent<ExpressionSolverVisualizer>();
            var panel = FindObjectOfType<VerticalLayoutGroup>();

            expressionSolverVisualizer.transform.SetParent(panel.transform);
            expressionSolverVisualizer.transform.SetAsFirstSibling();

            expressionSolverVisualizer.expression = expression;

            foreach (var variable in expression.GetVariables())
                foreach (var chromosome in candidate.chromosomes)
                    if (chromosome.name == variable.stringValue)
                        variable.value = chromosome.value;

            yield return expression.Evaluate();
            var result = expression.expressionObjects.First() as Variable;

            equation.solved = (bool)result.value;

            Destroy(expressionSolverVisualizer.gameObject);
        }
    }
}

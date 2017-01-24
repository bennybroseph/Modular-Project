namespace Library.GeneticAlgorithm
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    using Random = UnityEngine.Random;

    [Serializable]
    public class Chromosome
    {
        public bool value;
        public string name;

        public bool inherited;
    }
    [Serializable]
    public class Candidate
    {
        public List<Chromosome> chromosomes = new List<Chromosome>();

        public float fitness;
    }
    [Serializable]
    public class Generation
    {
        public List<Candidate> candidates = new List<Candidate>();
    }

    [Serializable]
    public class GeneticEquation
    {
        public Generation currentGeneration;

        public Expression expression;

        public bool solved;
        public Candidate solvingCandidate;

        public int generations;

        public int solvingCandidateIndex
        {
            get
            {
                return currentGeneration.candidates.FindIndex(candidate => candidate == solvingCandidate);
            }
        }
    }

    public class GeneticSolver : MonoBehaviour
    {
        [SerializeField]
        private List<ParseCNF> m_ParseCNFs;

        [SerializeField]
        private GeneticEquation m_CurrentGeneticEquation;

        private Candidate m_CurrentlyEvaluatedCandidate;
        private Expression m_CurrentlyEvaluatedExpression;

        private float m_NextStepHeldTime;

        public GeneticEquation currentGeneticEquation
        {
            get { return m_CurrentGeneticEquation; }
        }

        public Candidate currentlyEvaluatedCandidate
        {
            get { return m_CurrentlyEvaluatedCandidate; }
        }
        public Expression currentlyEvaluatedExpression
        {
            get { return m_CurrentlyEvaluatedExpression; }
        }

        // Use this for initialization
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;

            Random.InitState(DateTime.Now.Millisecond);

            foreach (var parseCNF in m_ParseCNFs)
                parseCNF.Parse();

            StartCoroutine(RunSolver());
        }

        private void Update()
        {
            m_NextStepHeldTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.P))
                Expression.pauseEvaluation = !Expression.pauseEvaluation;

            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                m_NextStepHeldTime >= 0.25f && Input.GetKey(KeyCode.RightArrow))
                StartCoroutine(NextStepEvaluation());

            if (!Input.GetKey(KeyCode.RightArrow))
                m_NextStepHeldTime = 0f;

            if (!Expression.yieldEvaluation)
                Expression.pauseEvaluation = false;
        }

        private IEnumerator RunSolver()
        {
            foreach (var parseCNF in m_ParseCNFs)
            {
                m_CurrentGeneticEquation = new GeneticEquation { expression = parseCNF.expression };

                var initialGeneration = new Generation();

                for (var i = 0; i < Random.Range(20, 30); ++i)
                    initialGeneration.candidates.Add(new Candidate());

                foreach (var candidate in initialGeneration.candidates)
                    foreach (var variable in parseCNF.expression.GetVariables())
                        candidate.chromosomes.Add(new Chromosome
                        {
                            value = Random.Range(0, 2) == 1,
                            name = variable.stringValue,
                        });

                ++m_CurrentGeneticEquation.generations;
                m_CurrentGeneticEquation.currentGeneration = initialGeneration;

                while (!m_CurrentGeneticEquation.solved)
                {
                    foreach (var candidate in m_CurrentGeneticEquation.currentGeneration.candidates)
                    {
                        if (m_CurrentGeneticEquation.solved)
                            continue;

                        yield return Evaluate(m_CurrentGeneticEquation, candidate);
                    }

                    if (m_CurrentGeneticEquation.solved)
                        continue;

                    var newGeneration = new Generation();

                    var currentGeneration = m_CurrentGeneticEquation.currentGeneration;

                    currentGeneration.candidates.Sort(SortCandidates);
                    var sortedCandidates = currentGeneration.candidates.ToList();

                    var parent1 = sortedCandidates.First();

                    sortedCandidates.RemoveAt(0);
                    var parent2 = sortedCandidates.First();

                    for (var i = 0; i < Random.Range(5, 10); ++i)
                    {
                        newGeneration.candidates.Add(new Candidate());
                        for (var j = 0; j < currentGeneration.candidates[0].chromosomes.Count; ++j)
                        {
                            var shouldInherit = Random.Range(1f, 100f) <= 40f;

                            newGeneration.candidates[i].chromosomes.Add(
                                new Chromosome
                                {
                                    value =
                                        shouldInherit
                                            ? Random.Range(0, 2) == 1
                                                ? parent1.chromosomes[j].value
                                                : parent2.chromosomes[j].value
                                            : Random.Range(0, 2) == 1,
                                    name = currentGeneration.candidates[0].chromosomes[j].name,
                                    inherited = shouldInherit
                                });
                        }
                    }

                    ++m_CurrentGeneticEquation.generations;
                    m_CurrentGeneticEquation.currentGeneration = newGeneration;

                    yield return null;
                }
                yield return new WaitForSeconds(3f);
            }
        }

        private IEnumerator Evaluate(GeneticEquation equation, Candidate candidate)
        {
            var expression = equation.expression.Copy() as Expression;
            if (expression == null)
                yield break;

            m_CurrentlyEvaluatedCandidate = candidate;
            m_CurrentlyEvaluatedExpression = expression;

            foreach (var chromosome in candidate.chromosomes)
                foreach (var variable in expression.GetVariables())
                    if (chromosome.name == variable.stringValue)
                        variable.value = chromosome.value;

            var fitnessEvaluation = expression.Copy() as Expression;

            yield return expression.Evaluate();
            var result = expression.expressionObjects.First() as Variable;

            equation.solved = (bool)result.value;
            if (equation.solved)
            {
                candidate.fitness = 1f;

                equation.solvingCandidate = candidate;
            }
            else
            {
                if (fitnessEvaluation == null)
                    yield break;

                var expressions = fitnessEvaluation.expressionObjects.OfType<Expression>().ToList();

                var trueClauses = 0f;
                foreach (var expr in expressions)
                {
                    var enumerator = expr.Evaluate();
                    while (enumerator.MoveNext()) { }

                    var evaluation = expr.expressionObjects.First() as Variable;
                    if (evaluation == null)
                        continue;

                    if ((bool)evaluation.value)
                        ++trueClauses;
                }

                candidate.fitness = trueClauses / expressions.Count;
            }
        }

        private IEnumerator NextStepEvaluation()
        {
            Expression.pauseEvaluation = false;
            yield return new WaitForEndOfFrame();
            Expression.pauseEvaluation = true;
        }

        private int SortCandidates(Candidate lhs, Candidate rhs)
        {
            if (lhs.fitness < rhs.fitness)
                return -1;

            if (lhs.fitness > rhs.fitness)
                return 1;

            return 0;
        }
    }
}

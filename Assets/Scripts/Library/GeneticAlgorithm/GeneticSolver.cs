namespace Library.GeneticAlgorithm
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

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

        public int age;
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
        public enum PopulationGenerationType
        {
            DoubleCrossover,
            BasicInheritance,
            SurvivalOfTheFittest,
        }

        [Header("UI Elements"), SerializeField]
        private Button m_NextStepEvaluationButton;
        [SerializeField]
        private Button m_NextCandidateButton;

        [Header("Algorithm"), SerializeField]
        private List<ParseCNF> m_ParseCNFs;

        [SerializeField]
        private GeneticEquation m_CurrentGeneticEquation;

        private Candidate m_CurrentlyEvaluatedCandidate;
        private Expression m_CurrentlyEvaluatedExpression;

        private float m_NextStepHeldTime;

        private PopulationGenerationType m_PopulationGenerationType =
            PopulationGenerationType.BasicInheritance;

        private bool m_ButtonPressed;

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

        public PopulationGenerationType populationGenerationType
        {
            get { return m_PopulationGenerationType; }
            set { m_PopulationGenerationType = value; }
        }

        // Use this for initialization
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;

            Random.InitState(DateTime.Now.Millisecond);

            if (m_NextStepEvaluationButton != null)
                m_NextStepEvaluationButton.onClick.AddListener(OnNextStepEvaluationButtonPress);
            if (m_NextCandidateButton != null)
                m_NextCandidateButton.onClick.AddListener(OnNextCandidateButtonPress);

            foreach (var parseCNF in m_ParseCNFs)
                parseCNF.Parse();

            StartCoroutine(RunSolver());
        }

        private void Update()
        {
            m_NextStepHeldTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                m_NextStepHeldTime >= 0.25f && (Input.GetKey(KeyCode.RightArrow) || m_ButtonPressed))
                OnNextStepEvaluationButtonPress();

            if (!Input.GetKey(KeyCode.RightArrow) && !m_ButtonPressed)
                m_NextStepHeldTime = 0f;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                OnNextCandidateButtonPress();

            if (m_ButtonPressed && !Input.GetMouseButton(0))
                m_ButtonPressed = false;
        }

        private void OnNextStepEvaluationButtonPress()
        {
            m_ButtonPressed = true;

            StartCoroutine(NextStepEvaluation());
        }
        private void OnNextCandidateButtonPress()
        {
            StartCoroutine(NextCandidate());
        }

        public void Restart()
        {
            StopAllCoroutines();

            Expression.pauseEvaluation = true;
            Expression.yieldEvaluation = true;

            StartCoroutine(RunSolver());
        }

        private IEnumerator RunSolver()
        {
            foreach (var parseCNF in m_ParseCNFs)
            {
                m_CurrentGeneticEquation = new GeneticEquation { expression = parseCNF.expression };

                var initialGeneration = new Generation();

                for (var i = 0; i < Random.Range(15, 25); ++i)
                    initialGeneration.candidates.Add(new Candidate());

                var sortedVariables =
                    parseCNF.expression.GetVariables().OrderBy(variable => variable.stringValue).ToList();

                foreach (var candidate in initialGeneration.candidates)
                    foreach (var variable in sortedVariables)
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

                    Generation newGeneration = null;
                    switch (m_PopulationGenerationType)
                    {
                    case PopulationGenerationType.DoubleCrossover:
                        newGeneration = DoubleCrossover();
                        break;
                    case PopulationGenerationType.BasicInheritance:
                        newGeneration = BasicInheritance();
                        break;
                    case PopulationGenerationType.SurvivalOfTheFittest:
                        newGeneration = SurvivalOfTheFittest();
                        break;

                    default:
                        break;
                    }

                    m_CurrentGeneticEquation.currentGeneration = newGeneration;

                    ++m_CurrentGeneticEquation.generations;

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

        private IEnumerator NextCandidate()
        {
            Expression.pauseEvaluation = false;
            Expression.yieldEvaluation = false;

            yield return new WaitForEndOfFrame();

            Expression.pauseEvaluation = true;
            Expression.yieldEvaluation = true;
        }

        private IEnumerator NextStepEvaluation()
        {
            Expression.pauseEvaluation = false;

            yield return new WaitForEndOfFrame();

            Expression.pauseEvaluation = true;
        }

        private Generation DoubleCrossover()
        {
            var newGeneration = new Generation();

            var currentGeneration = m_CurrentGeneticEquation.currentGeneration;

            Candidate parent1;
            Candidate parent2;
            if (currentGeneration.candidates.Count > 2)
            {
                var sortedCandidates =
                    currentGeneration.candidates.OrderByDescending(candidate => candidate.fitness).ToList();

                parent1 = sortedCandidates.First();

                sortedCandidates.RemoveAt(0);
                parent2 = sortedCandidates.First();
            }
            else
            {
                parent1 = currentGeneration.candidates.First();
                parent2 = currentGeneration.candidates.Last();
            }

            for (var i = 0; i < 2; ++i)
            {
                newGeneration.candidates.Add(new Candidate());
                for (var j = 0; j < currentGeneration.candidates[0].chromosomes.Count; ++j)
                {
                    var shouldInherit =
                        j < currentGeneration.candidates[0].chromosomes.Count / 2 &&
                        Random.Range(1f, 100f) <= 75f;

                    newGeneration.candidates[i].chromosomes.Add(
                        new Chromosome
                        {
                            value =
                                shouldInherit
                                    ? i == 0
                                        ? parent2.chromosomes[j].value
                                        : parent1.chromosomes[j].value
                                    : Random.Range(0, 2) == 1,
                            name = currentGeneration.candidates[0].chromosomes[j].name,
                            inherited = shouldInherit
                        });
                }
            }

            return newGeneration;
        }

        private Generation BasicInheritance()
        {
            var newGeneration = new Generation();

            var currentGeneration = m_CurrentGeneticEquation.currentGeneration;

            var sortedCandidates =
                currentGeneration.candidates.OrderByDescending(candidate => candidate.fitness).ToList();

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

            return newGeneration;
        }

        private Generation SurvivalOfTheFittest()
        {
            var newGeneration = new Generation();

            var currentGeneration = m_CurrentGeneticEquation.currentGeneration;

            var sortedCandidates =
                currentGeneration.candidates.OrderByDescending(candidate => candidate.fitness).ToList();

            var parents = new List<Candidate> { sortedCandidates.First() };

            sortedCandidates.RemoveAt(0);
            parents.Add(sortedCandidates.First());

            for (var i = 0; i < Random.Range(5, 10) + 2; ++i)
            {
                if (i < 2 && parents[i].age < 10)
                {
                    ++parents[i].age;
                    newGeneration.candidates.Add(parents[i]);

                    continue;
                }

                newGeneration.candidates.Add(new Candidate());
                for (var j = 0; j < currentGeneration.candidates[0].chromosomes.Count; ++j)
                {
                    var shouldInherit =
                        Random.Range(1f, 100f) <= 75f || parents.Any(parent => parent.age > 10);

                    newGeneration.candidates[i].chromosomes.Add(
                        new Chromosome
                        {
                            value =
                                shouldInherit
                                    ? parents[Random.Range(0, parents.Count)].chromosomes[j].value
                                    : Random.Range(0, 2) == 1,
                            name = currentGeneration.candidates[0].chromosomes[j].name,
                            inherited = shouldInherit
                        });
                }
            }

            return newGeneration;
        }
    }
}

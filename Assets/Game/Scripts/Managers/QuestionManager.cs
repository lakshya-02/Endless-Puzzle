using System.Collections.Generic;
using System.Text;
using EndlessPuzzle.Data;
using EndlessPuzzle.Gameplay;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class QuestionManager : MonoBehaviour
    {
        private readonly List<QuestionCandidate> _candidates = new List<QuestionCandidate>(32);
        private readonly StringBuilder _builder = new StringBuilder(64);

        private GameConfigDatabase _config;
        private QuestionTemplateDefinition _currentTemplate;
        private TargetObject _currentCorrectObject;

        public event System.Action<string> QuestionChanged;

        public string CurrentPrompt { get; private set; } = string.Empty;

        public void Initialize(GameConfigDatabase config)
        {
            _config = config;
            ClearQuestion();
        }

        public bool EnsureValidQuestion(IReadOnlyList<TargetObject> activeObjects, int score, bool forceRegenerate)
        {
            if (!forceRegenerate && IsCurrentQuestionStillValid(activeObjects))
            {
                return true;
            }

            return GenerateNewQuestion(activeObjects, score);
        }

        public bool IsCorrectAnswer(TargetObject targetObject)
        {
            return targetObject != null && targetObject == _currentCorrectObject;
        }

        public bool IsCurrentQuestionStillValid(IReadOnlyList<TargetObject> activeObjects)
        {
            if (_currentCorrectObject == null || _currentTemplate == null || activeObjects == null || activeObjects.Count == 0)
            {
                return false;
            }

            int matchCount = CountMatches(activeObjects, _currentCorrectObject, _currentTemplate.RequiredMask);
            if (matchCount != 1)
            {
                return false;
            }

            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (activeObjects[i] == _currentCorrectObject)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearQuestion()
        {
            _currentTemplate = null;
            _currentCorrectObject = null;
            CurrentPrompt = string.Empty;
            QuestionChanged?.Invoke(CurrentPrompt);
        }

        private bool GenerateNewQuestion(IReadOnlyList<TargetObject> activeObjects, int score)
        {
            _candidates.Clear();

            if (activeObjects == null || activeObjects.Count == 0)
            {
                ClearQuestion();
                return false;
            }

            float safeMinY = GetQuestionSafetyMinY(score, 2.4f);
            float relaxedMinY = GetQuestionSafetyMinY(score, 1.5f);

            BuildCandidates(activeObjects, score, safeMinY);
            if (_candidates.Count == 0)
            {
                BuildCandidates(activeObjects, score, relaxedMinY);
            }

            if (_candidates.Count == 0)
            {
                BuildCandidates(activeObjects, score, float.NegativeInfinity);
            }

            if (_candidates.Count == 0)
            {
                ClearQuestion();
                return false;
            }

            QuestionCandidate selected = ChooseWeightedCandidate();
            _currentCorrectObject = selected.Target;
            _currentTemplate = selected.Template;
            CurrentPrompt = BuildPrompt(selected.Target, selected.Template);
            QuestionChanged?.Invoke(CurrentPrompt);
            return true;
        }

        private void BuildCandidates(IReadOnlyList<TargetObject> activeObjects, int score, float minimumY)
        {
            QuestionTemplateDefinition[] templates = _config.QuestionTemplates;

            for (int objectIndex = 0; objectIndex < activeObjects.Count; objectIndex++)
            {
                TargetObject target = activeObjects[objectIndex];
                if (target.CachedTransform.position.y < minimumY)
                {
                    continue;
                }

                for (int templateIndex = 0; templateIndex < templates.Length; templateIndex++)
                {
                    QuestionTemplateDefinition template = templates[templateIndex];
                    if (score < template.MinScore)
                    {
                        continue;
                    }

                    if (CountMatches(activeObjects, target, template.RequiredMask) == 1)
                    {
                        int effectiveWeight = GetEffectiveWeight(activeObjects, target, template);
                        _candidates.Add(new QuestionCandidate(target, template, effectiveWeight));
                    }
                }
            }
        }

        private int CountMatches(IReadOnlyList<TargetObject> activeObjects, TargetObject templateTarget, QuestionAttributeMask mask)
        {
            int count = 0;
            int colorIndex = templateTarget.ActualColorIndex;
            int shapeIndex = templateTarget.ShapeIndex;

            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (activeObjects[i].Matches(colorIndex, shapeIndex, mask))
                {
                    count++;
                }
            }

            return count;
        }

        private QuestionCandidate ChooseWeightedCandidate()
        {
            int totalWeight = 0;
            for (int i = 0; i < _candidates.Count; i++)
            {
                totalWeight += _candidates[i].EffectiveWeight;
            }

            int roll = Random.Range(0, totalWeight);
            for (int i = 0; i < _candidates.Count; i++)
            {
                roll -= _candidates[i].EffectiveWeight;
                if (roll < 0)
                {
                    return _candidates[i];
                }
            }

            return _candidates[_candidates.Count - 1];
        }

        private string BuildPrompt(TargetObject targetObject, QuestionTemplateDefinition template)
        {
            string format = template.Format;
            int displayedColorIndex = GetDisplayedQuestionColorIndex(targetObject.ActualColorIndex);
            _builder.Clear();

            for (int i = 0; i < format.Length; i++)
            {
                char currentChar = format[i];
                if (currentChar != '{')
                {
                    _builder.Append(currentChar);
                    continue;
                }

                int closingIndex = format.IndexOf('}', i);
                if (closingIndex < 0)
                {
                    _builder.Append(currentChar);
                    continue;
                }

                string token = format.Substring(i + 1, closingIndex - i - 1);
                switch (token)
                {
                    case "color":
                        AppendRichColorWord(targetObject.ActualColorIndex, displayedColorIndex);
                        break;
                    case "shape":
                        _builder.Append(_config.GetShapeName(targetObject.ShapeIndex));
                        break;
                    default:
                        _builder.Append('{').Append(token).Append('}');
                        break;
                }

                i = closingIndex;
            }

            return _builder.ToString();
        }

        private void AppendRichColorWord(int actualColorIndex, int displayedColorIndex)
        {
            _builder
                .Append("<color=#")
                .Append(_config.GetColorRichTextHex(displayedColorIndex))
                .Append('>')
                .Append(_config.GetColorName(actualColorIndex))
                .Append("</color>");
        }

        private int GetDisplayedQuestionColorIndex(int actualColorIndex)
        {
            if (_config.ColorCount <= 1)
            {
                return actualColorIndex;
            }

            if (Random.value < 0.5f)
            {
                return actualColorIndex;
            }

            int displayedColorIndex = actualColorIndex;
            while (displayedColorIndex == actualColorIndex)
            {
                displayedColorIndex = Random.Range(0, _config.ColorCount);
            }

            return displayedColorIndex;
        }

        private DifficultyTierDefinition GetDifficultyTierForScore(int score)
        {
            DifficultyTierDefinition[] tiers = _config.DifficultyTiers;
            int tierIndex = 0;

            for (int i = 1; i < tiers.Length; i++)
            {
                if (score >= tiers[i].ScoreThreshold)
                {
                    tierIndex = i;
                }
                else
                {
                    break;
                }
            }

            return tiers[tierIndex];
        }

        private int GetEffectiveWeight(IReadOnlyList<TargetObject> activeObjects, TargetObject target, QuestionTemplateDefinition template)
        {
            int effectiveWeight = Mathf.Max(1, template.Weight);
            int sameColorDifferentShapeCount = 0;
            int sameShapeDifferentColorCount = 0;

            for (int i = 0; i < activeObjects.Count; i++)
            {
                TargetObject other = activeObjects[i];
                if (other == target)
                {
                    continue;
                }

                if (other.ActualColorIndex == target.ActualColorIndex && other.ShapeIndex != target.ShapeIndex)
                {
                    sameColorDifferentShapeCount++;
                }

                if (other.ShapeIndex == target.ShapeIndex && other.ActualColorIndex != target.ActualColorIndex)
                {
                    sameShapeDifferentColorCount++;
                }
            }

            bool targetLabelIsMisleading = target.LabelColorIndex != target.ActualColorIndex;

            if ((template.RequiredMask & QuestionAttributeMask.Color) != 0 && targetLabelIsMisleading)
            {
                effectiveWeight += 2;
            }

            if (template.RequiredMask == (QuestionAttributeMask.Color | QuestionAttributeMask.Shape))
            {
                effectiveWeight += sameColorDifferentShapeCount * 2;
                effectiveWeight += sameShapeDifferentColorCount * 2;

                if (sameColorDifferentShapeCount > 0 && sameShapeDifferentColorCount > 0)
                {
                    effectiveWeight += 4;
                }
            }

            float heightAboveFail = target.CachedTransform.position.y - _config.SpawnSettings.FailY;
            effectiveWeight += Mathf.Clamp(Mathf.RoundToInt(heightAboveFail), 0, 6);

            return Mathf.Max(1, effectiveWeight);
        }

        private float GetQuestionSafetyMinY(int score, float leadTimeSeconds)
        {
            DifficultyTierDefinition tier = GetDifficultyTierForScore(score);
            float dynamicSafetyDistance = tier.FallSpeed * leadTimeSeconds;
            float staticSafetyDistance = _config.SpawnSettings.SafeQuestionDistanceFromFailY;
            return _config.SpawnSettings.FailY + Mathf.Max(staticSafetyDistance, dynamicSafetyDistance);
        }

        private readonly struct QuestionCandidate
        {
            public QuestionCandidate(TargetObject target, QuestionTemplateDefinition template, int effectiveWeight)
            {
                Target = target;
                Template = template;
                EffectiveWeight = effectiveWeight;
            }

            public TargetObject Target { get; }
            public QuestionTemplateDefinition Template { get; }
            public int EffectiveWeight { get; }
        }
    }
}

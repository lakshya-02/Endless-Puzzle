using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessPuzzle.Data
{
    public static class JsonDataLoader
    {
        private const string ColorsPath = "JSON/Colors";
        private const string ShapesPath = "JSON/Shapes";
        private const string QuestionsPath = "JSON/QuestionTemplates";
        private const string DifficultyPath = "JSON/Difficulty";
        private const string SpawnSettingsPath = "JSON/SpawnSettings";

        public static GameConfigDatabase Load()
        {
            ColorCatalogDto colorCatalog = LoadResource<ColorCatalogDto>(ColorsPath);
            ShapeCatalogDto shapeCatalog = LoadResource<ShapeCatalogDto>(ShapesPath);
            QuestionTemplateCatalogDto questionCatalog = LoadResource<QuestionTemplateCatalogDto>(QuestionsPath);
            DifficultyCatalogDto difficultyCatalog = LoadResource<DifficultyCatalogDto>(DifficultyPath);
            SpawnSettingsDto spawnSettingsDto = LoadResource<SpawnSettingsDto>(SpawnSettingsPath);

            ValidateCatalog(colorCatalog != null && colorCatalog.colors != null && colorCatalog.colors.Length > 0, "Colors.json must contain at least one color.");
            ValidateCatalog(shapeCatalog != null && shapeCatalog.shapes != null && shapeCatalog.shapes.Length > 0, "Shapes.json must contain at least one shape.");
            ValidateCatalog(questionCatalog != null && questionCatalog.templates != null && questionCatalog.templates.Length > 0, "QuestionTemplates.json must contain at least one template.");
            ValidateCatalog(difficultyCatalog != null && difficultyCatalog.tiers != null && difficultyCatalog.tiers.Length > 0, "Difficulty.json must contain at least one tier.");

            ColorDefinition[] colors = BuildColors(colorCatalog.colors, out Dictionary<string, int> colorLookup);
            ShapeDefinition[] shapes = BuildShapes(shapeCatalog.shapes, out Dictionary<string, int> shapeLookup);
            QuestionTemplateDefinition[] questionTemplates = BuildQuestionTemplates(questionCatalog.templates);
            DifficultyTierDefinition[] difficultyTiers = BuildDifficultyTiers(difficultyCatalog.tiers);
            SpawnSettingsDefinition spawnSettings = BuildSpawnSettings(spawnSettingsDto);

            return new GameConfigDatabase(colors, shapes, questionTemplates, difficultyTiers, spawnSettings, colorLookup, shapeLookup);
        }

        private static T LoadResource<T>(string resourcePath) where T : class
        {
            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                throw new InvalidOperationException($"Missing JSON resource at Resources/{resourcePath}.json");
            }

            T instance = JsonUtility.FromJson<T>(textAsset.text);
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to parse JSON resource at Resources/{resourcePath}.json");
            }

            return instance;
        }

        private static ColorDefinition[] BuildColors(ColorDefinitionDto[] source, out Dictionary<string, int> colorLookup)
        {
            colorLookup = new Dictionary<string, int>(source.Length, StringComparer.OrdinalIgnoreCase);
            ColorDefinition[] colors = new ColorDefinition[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                ColorDefinitionDto dto = source[i];
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.id), $"Color entry at index {i} is missing an id.");
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.displayName), $"Color '{dto.id}' is missing a displayName.");
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.htmlColor), $"Color '{dto.id}' is missing an htmlColor.");
                ValidateCatalog(!colorLookup.ContainsKey(dto.id), $"Duplicate color id '{dto.id}' found in Colors.json.");
                ValidateCatalog(ColorUtility.TryParseHtmlString(dto.htmlColor, out Color unityColor), $"Color '{dto.id}' has invalid htmlColor '{dto.htmlColor}'.");

                string normalizedId = dto.id.Trim();
                colors[i] = new ColorDefinition(i, normalizedId, dto.displayName.Trim(), unityColor);
                colorLookup.Add(normalizedId, i);
            }

            return colors;
        }

        private static ShapeDefinition[] BuildShapes(ShapeDefinitionDto[] source, out Dictionary<string, int> shapeLookup)
        {
            shapeLookup = new Dictionary<string, int>(source.Length, StringComparer.OrdinalIgnoreCase);
            ShapeDefinition[] shapes = new ShapeDefinition[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                ShapeDefinitionDto dto = source[i];
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.id), $"Shape entry at index {i} is missing an id.");
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.displayName), $"Shape '{dto.id}' is missing a displayName.");
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.primitive), $"Shape '{dto.id}' is missing a primitive value.");
                ValidateCatalog(!shapeLookup.ContainsKey(dto.id), $"Duplicate shape id '{dto.id}' found in Shapes.json.");

                string normalizedId = dto.id.Trim();
                ShapePrimitiveType primitiveType = ParsePrimitiveType(dto.primitive, normalizedId);
                float uniformScale = Mathf.Max(0.25f, dto.uniformScale);

                shapes[i] = new ShapeDefinition(
                    i,
                    normalizedId,
                    dto.displayName.Trim(),
                    primitiveType,
                    uniformScale,
                    Mathf.Max(0f, dto.timeBonusSeconds));
                shapeLookup.Add(normalizedId, i);
            }

            return shapes;
        }

        private static QuestionTemplateDefinition[] BuildQuestionTemplates(QuestionTemplateDto[] source)
        {
            QuestionTemplateDefinition[] templates = new QuestionTemplateDefinition[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                QuestionTemplateDto dto = source[i];
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.id), $"Question template at index {i} is missing an id.");
                ValidateCatalog(!string.IsNullOrWhiteSpace(dto.format), $"Question template '{dto.id}' is missing a format.");
                ValidateCatalog(dto.requiredAttributes != null && dto.requiredAttributes.Length > 0, $"Question template '{dto.id}' must declare requiredAttributes.");

                QuestionAttributeMask requiredMask = ParseAttributeMask(dto.requiredAttributes, dto.id);
                ValidateCatalog(requiredMask != QuestionAttributeMask.None, $"Question template '{dto.id}' must map to at least one attribute.");

                templates[i] = new QuestionTemplateDefinition(
                    dto.id.Trim(),
                    dto.format.Trim(),
                    requiredMask,
                    Mathf.Max(0, dto.minScore),
                    Mathf.Max(1, dto.weight));
            }

            return templates;
        }

        private static DifficultyTierDefinition[] BuildDifficultyTiers(DifficultyTierDto[] source)
        {
            DifficultyTierDefinition[] tiers = new DifficultyTierDefinition[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                DifficultyTierDto dto = source[i];
                int previousThreshold = i == 0 ? -1 : tiers[i - 1].ScoreThreshold;
                ValidateCatalog(dto.scoreThreshold > previousThreshold, "Difficulty tiers must be ordered by strictly increasing scoreThreshold values.");
                ValidateCatalog(dto.maxActiveObjects > 0, $"Difficulty tier at score {dto.scoreThreshold} must use maxActiveObjects > 0.");
                ValidateCatalog(dto.fallSpeed > 0f, $"Difficulty tier at score {dto.scoreThreshold} must use fallSpeed > 0.");
                ValidateCatalog(dto.spawnInterval > 0.05f, $"Difficulty tier at score {dto.scoreThreshold} must use spawnInterval > 0.05.");

                tiers[i] = new DifficultyTierDefinition(
                    dto.scoreThreshold,
                    dto.maxActiveObjects,
                    dto.fallSpeed,
                    Mathf.Clamp01(dto.misleadingChance),
                    dto.spawnInterval);
            }

            return tiers;
        }

        private static SpawnSettingsDefinition BuildSpawnSettings(SpawnSettingsDto source)
        {
            ValidateCatalog(source.horizontalMin < source.horizontalMax, "SpawnSettings horizontalMin must be smaller than horizontalMax.");
            ValidateCatalog(source.depthMin < source.depthMax, "SpawnSettings depthMin must be smaller than depthMax.");
            ValidateCatalog(source.failY < source.spawnY, "SpawnSettings failY must be below spawnY.");
            ValidateCatalog(source.poolPrewarmCount > 0, "SpawnSettings poolPrewarmCount must be greater than zero.");
            ValidateCatalog(source.absoluteMaxActiveObjects > 0, "SpawnSettings absoluteMaxActiveObjects must be greater than zero.");
            ValidateCatalog(source.startTimeSeconds > 0f, "SpawnSettings startTimeSeconds must be greater than zero.");
            ValidateCatalog(source.maxTimeSeconds >= source.startTimeSeconds, "SpawnSettings maxTimeSeconds must be greater than or equal to startTimeSeconds.");

            return new SpawnSettingsDefinition(
                source.spawnY,
                source.failY,
                source.horizontalMin,
                source.horizontalMax,
                source.depthMin,
                source.depthMax,
                Mathf.Max(0.1f, source.minSpacingX),
                Mathf.Max(0.1f, source.minSpacingZ),
                Mathf.Max(0.5f, source.spawnStackSpacingY),
                Mathf.Max(0.5f, source.safeQuestionDistanceFromFailY),
                Mathf.Max(1, source.maxSpawnAttempts),
                source.poolPrewarmCount,
                source.absoluteMaxActiveObjects,
                Mathf.Max(0.25f, source.targetScale),
                Mathf.Max(1f, source.startTimeSeconds),
                Mathf.Max(source.startTimeSeconds, source.maxTimeSeconds),
                Mathf.Max(0f, source.correctTimeRewardSeconds),
                Mathf.Max(0f, source.wrongTapTimePenaltySeconds),
                Mathf.Max(0f, source.missTimePenaltySeconds),
                Mathf.Max(0, source.wrongTapScorePenalty),
                Mathf.Max(0, source.missScorePenalty),
                Mathf.Max(0.1f, source.labelColorCycleSeconds));
        }

        private static ShapePrimitiveType ParsePrimitiveType(string value, string shapeId)
        {
            string normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "cube":
                    return ShapePrimitiveType.Cube;
                case "sphere":
                    return ShapePrimitiveType.Sphere;
                case "capsule":
                    return ShapePrimitiveType.Capsule;
                case "cylinder":
                    return ShapePrimitiveType.Cylinder;
                default:
                    throw new InvalidOperationException($"Shape '{shapeId}' uses unsupported primitive '{value}'.");
            }
        }

        private static QuestionAttributeMask ParseAttributeMask(string[] attributes, string templateId)
        {
            QuestionAttributeMask mask = QuestionAttributeMask.None;

            for (int i = 0; i < attributes.Length; i++)
            {
                string attribute = attributes[i].Trim().ToLowerInvariant();
                switch (attribute)
                {
                    case "color":
                        mask |= QuestionAttributeMask.Color;
                        break;
                    case "shape":
                        mask |= QuestionAttributeMask.Shape;
                        break;
                    default:
                        throw new InvalidOperationException($"Question template '{templateId}' uses unsupported requiredAttribute '{attributes[i]}'.");
                }
            }

            return mask;
        }

        private static void ValidateCatalog(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

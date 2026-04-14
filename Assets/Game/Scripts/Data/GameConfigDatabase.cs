using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessPuzzle.Data
{
    [Flags]
    public enum QuestionAttributeMask
    {
        None = 0,
        Color = 1 << 0,
        Shape = 1 << 1
    }

    public enum ShapePrimitiveType
    {
        Cube = 0,
        Sphere = 1,
        Capsule = 2,
        Cylinder = 3
    }

    public sealed class ColorDefinition
    {
        public ColorDefinition(int index, string id, string displayName, Color unityColor)
        {
            Index = index;
            Id = id;
            DisplayName = displayName;
            UnityColor = unityColor;
        }

        public int Index { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public Color UnityColor { get; }
    }

    public sealed class ShapeDefinition
    {
        public ShapeDefinition(int index, string id, string displayName, ShapePrimitiveType primitiveType, float uniformScale, float timeBonusSeconds)
        {
            Index = index;
            Id = id;
            DisplayName = displayName;
            PrimitiveType = primitiveType;
            UniformScale = uniformScale;
            TimeBonusSeconds = timeBonusSeconds;
        }

        public int Index { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public ShapePrimitiveType PrimitiveType { get; }
        public float UniformScale { get; }
        public float TimeBonusSeconds { get; }
    }

    public sealed class QuestionTemplateDefinition
    {
        public QuestionTemplateDefinition(string id, string format, QuestionAttributeMask requiredMask, int minScore, int weight)
        {
            Id = id;
            Format = format;
            RequiredMask = requiredMask;
            MinScore = minScore;
            Weight = weight;
        }

        public string Id { get; }
        public string Format { get; }
        public QuestionAttributeMask RequiredMask { get; }
        public int MinScore { get; }
        public int Weight { get; }
    }

    public readonly struct DifficultyTierDefinition
    {
        public DifficultyTierDefinition(int scoreThreshold, int maxActiveObjects, float fallSpeed, float misleadingChance, float spawnInterval)
        {
            ScoreThreshold = scoreThreshold;
            MaxActiveObjects = maxActiveObjects;
            FallSpeed = fallSpeed;
            MisleadingChance = misleadingChance;
            SpawnInterval = spawnInterval;
        }

        public int ScoreThreshold { get; }
        public int MaxActiveObjects { get; }
        public float FallSpeed { get; }
        public float MisleadingChance { get; }
        public float SpawnInterval { get; }
    }

    public readonly struct SpawnSettingsDefinition
    {
        public SpawnSettingsDefinition(
            float spawnY,
            float failY,
            float horizontalMin,
            float horizontalMax,
            float depthMin,
            float depthMax,
            float minSpacingX,
            float minSpacingZ,
            float spawnStackSpacingY,
            float safeQuestionDistanceFromFailY,
            int maxSpawnAttempts,
            int poolPrewarmCount,
            int absoluteMaxActiveObjects,
            float targetScale,
            float startTimeSeconds,
            float maxTimeSeconds,
            float correctTimeRewardSeconds,
            float wrongTapTimePenaltySeconds,
            float missTimePenaltySeconds,
            int wrongTapScorePenalty,
            int missScorePenalty,
            float labelColorCycleSeconds)
        {
            SpawnY = spawnY;
            FailY = failY;
            HorizontalMin = horizontalMin;
            HorizontalMax = horizontalMax;
            DepthMin = depthMin;
            DepthMax = depthMax;
            MinSpacingX = minSpacingX;
            MinSpacingZ = minSpacingZ;
            SpawnStackSpacingY = spawnStackSpacingY;
            SafeQuestionDistanceFromFailY = safeQuestionDistanceFromFailY;
            MaxSpawnAttempts = maxSpawnAttempts;
            PoolPrewarmCount = poolPrewarmCount;
            AbsoluteMaxActiveObjects = absoluteMaxActiveObjects;
            TargetScale = targetScale;
            StartTimeSeconds = startTimeSeconds;
            MaxTimeSeconds = maxTimeSeconds;
            CorrectTimeRewardSeconds = correctTimeRewardSeconds;
            WrongTapTimePenaltySeconds = wrongTapTimePenaltySeconds;
            MissTimePenaltySeconds = missTimePenaltySeconds;
            WrongTapScorePenalty = wrongTapScorePenalty;
            MissScorePenalty = missScorePenalty;
            LabelColorCycleSeconds = labelColorCycleSeconds;
        }

        public float SpawnY { get; }
        public float FailY { get; }
        public float HorizontalMin { get; }
        public float HorizontalMax { get; }
        public float DepthMin { get; }
        public float DepthMax { get; }
        public float MinSpacingX { get; }
        public float MinSpacingZ { get; }
        public float SpawnStackSpacingY { get; }
        public float SafeQuestionDistanceFromFailY { get; }
        public int MaxSpawnAttempts { get; }
        public int PoolPrewarmCount { get; }
        public int AbsoluteMaxActiveObjects { get; }
        public float TargetScale { get; }
        public float StartTimeSeconds { get; }
        public float MaxTimeSeconds { get; }
        public float CorrectTimeRewardSeconds { get; }
        public float WrongTapTimePenaltySeconds { get; }
        public float MissTimePenaltySeconds { get; }
        public int WrongTapScorePenalty { get; }
        public int MissScorePenalty { get; }
        public float LabelColorCycleSeconds { get; }
    }

    public readonly struct RuntimeDifficulty
    {
        public RuntimeDifficulty(int tierIndex, int maxActiveObjects, float fallSpeed, float misleadingChance, float spawnInterval)
        {
            TierIndex = tierIndex;
            MaxActiveObjects = maxActiveObjects;
            FallSpeed = fallSpeed;
            MisleadingChance = misleadingChance;
            SpawnInterval = spawnInterval;
        }

        public int TierIndex { get; }
        public int MaxActiveObjects { get; }
        public float FallSpeed { get; }
        public float MisleadingChance { get; }
        public float SpawnInterval { get; }
    }

    public sealed class GameConfigDatabase
    {
        private readonly Dictionary<string, int> _colorLookup;
        private readonly Dictionary<string, int> _shapeLookup;

        public GameConfigDatabase(
            ColorDefinition[] colors,
            ShapeDefinition[] shapes,
            QuestionTemplateDefinition[] questionTemplates,
            DifficultyTierDefinition[] difficultyTiers,
            SpawnSettingsDefinition spawnSettings,
            Dictionary<string, int> colorLookup,
            Dictionary<string, int> shapeLookup)
        {
            Colors = colors;
            Shapes = shapes;
            QuestionTemplates = questionTemplates;
            DifficultyTiers = difficultyTiers;
            SpawnSettings = spawnSettings;
            _colorLookup = colorLookup;
            _shapeLookup = shapeLookup;
        }

        public ColorDefinition[] Colors { get; }
        public ShapeDefinition[] Shapes { get; }
        public QuestionTemplateDefinition[] QuestionTemplates { get; }
        public DifficultyTierDefinition[] DifficultyTiers { get; }
        public SpawnSettingsDefinition SpawnSettings { get; }

        public int ColorCount => Colors.Length;
        public int ShapeCount => Shapes.Length;
        public int TotalUniqueTargetCombos => ColorCount * ShapeCount;

        public string GetColorName(int colorIndex)
        {
            return Colors[colorIndex].DisplayName;
        }

        public string GetShapeName(int shapeIndex)
        {
            return Shapes[shapeIndex].DisplayName;
        }

        public Color GetUnityColor(int colorIndex)
        {
            return Colors[colorIndex].UnityColor;
        }

        public string GetColorRichTextHex(int colorIndex)
        {
            return ColorUtility.ToHtmlStringRGB(Colors[colorIndex].UnityColor);
        }

        public ShapeDefinition GetShapeDefinition(int shapeIndex)
        {
            return Shapes[shapeIndex];
        }

        public bool TryGetColorIndex(string id, out int index)
        {
            return _colorLookup.TryGetValue(id, out index);
        }

        public bool TryGetShapeIndex(string id, out int index)
        {
            return _shapeLookup.TryGetValue(id, out index);
        }
    }
}

using System;

namespace EndlessPuzzle.Data
{
    [Serializable]
    public sealed class ColorCatalogDto
    {
        public ColorDefinitionDto[] colors;
    }

    [Serializable]
    public sealed class ColorDefinitionDto
    {
        public string id;
        public string displayName;
        public string htmlColor;
    }

    [Serializable]
    public sealed class ShapeCatalogDto
    {
        public ShapeDefinitionDto[] shapes;
    }

    [Serializable]
    public sealed class ShapeDefinitionDto
    {
        public string id;
        public string displayName;
        public string primitive;
        public float uniformScale = 1f;
        public float timeBonusSeconds;
    }

    [Serializable]
    public sealed class QuestionTemplateCatalogDto
    {
        public QuestionTemplateDto[] templates;
    }

    [Serializable]
    public sealed class QuestionTemplateDto
    {
        public string id;
        public string format;
        public string[] requiredAttributes;
        public int minScore;
        public int weight = 1;
    }

    [Serializable]
    public sealed class DifficultyCatalogDto
    {
        public DifficultyTierDto[] tiers;
    }

    [Serializable]
    public sealed class DifficultyTierDto
    {
        public int scoreThreshold;
        public int maxActiveObjects;
        public float fallSpeed;
        public float misleadingChance;
        public float spawnInterval;
    }

    [Serializable]
    public sealed class SpawnSettingsDto
    {
        public float spawnY = 6.5f;
        public float failY = -6.5f;
        public float horizontalMin = -2.4f;
        public float horizontalMax = 2.4f;
        public float depthMin = -1.1f;
        public float depthMax = 1.1f;
        public float minSpacingX = 0.95f;
        public float minSpacingZ = 0.9f;
        public float spawnStackSpacingY = 1.35f;
        public float safeQuestionDistanceFromFailY = 2.5f;
        public int maxSpawnAttempts = 10;
        public int poolPrewarmCount = 18;
        public int absoluteMaxActiveObjects = 10;
        public float targetScale = 1f;
        public float startTimeSeconds = 15f;
        public float maxTimeSeconds = 20f;
        public float correctTimeRewardSeconds = 0.45f;
        public float wrongTapTimePenaltySeconds = 1.2f;
        public float missTimePenaltySeconds = 0.9f;
        public int wrongTapScorePenalty = 1;
        public int missScorePenalty = 1;
        public float labelColorCycleSeconds = 0.65f;
    }
}

using UnityEngine;

namespace EndlessPuzzle.Data
{
    public sealed class MaterialLibrary
    {
        private readonly Material[] _materials;

        public MaterialLibrary(GameConfigDatabase config, ColorMaterialPalette palette)
        {
            Shader shader = ResolveFallbackShader();
            if (shader == null)
            {
                throw new UnityException("No supported shader found for runtime material creation.");
            }

            _materials = new Material[config.ColorCount];

            for (int i = 0; i < config.ColorCount; i++)
            {
                if (palette != null && palette.TryGetMaterial(config.Colors[i].Id, out Material paletteMaterial))
                {
                    _materials[i] = paletteMaterial;
                    continue;
                }

                Material material = palette != null && palette.FallbackBaseMaterial != null
                    ? new Material(palette.FallbackBaseMaterial)
                    : new Material(shader);

                material.name = $"Runtime_{config.Colors[i].DisplayName}_Material";
                material.color = config.GetUnityColor(i);
                if (material.HasFloat("_Smoothness"))
                {
                    material.SetFloat("_Smoothness", 0f);
                }

                if (material.HasFloat("_Metallic"))
                {
                    material.SetFloat("_Metallic", 0f);
                }

                material.enableInstancing = true;
                _materials[i] = material;
            }
        }

        public Material GetMaterial(int colorIndex)
        {
            return _materials[colorIndex];
        }

        private static Shader ResolveFallbackShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }
    }
}

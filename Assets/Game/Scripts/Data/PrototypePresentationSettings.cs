using TMPro;
using UnityEngine;

namespace EndlessPuzzle.Data
{
    [CreateAssetMenu(fileName = "PrototypePresentationSettings", menuName = "Endless Puzzle/Prototype Presentation Settings")]
    public sealed class PrototypePresentationSettings : ScriptableObject
    {
        [SerializeField] private TMP_FontAsset _fontAsset;
        [SerializeField] private Font _worldLabelFont;
        [SerializeField] private ColorMaterialPalette _colorMaterialPalette;

        public TMP_FontAsset FontAsset => _fontAsset;
        public Font WorldLabelFont => _worldLabelFont;
        public ColorMaterialPalette ColorMaterialPalette => _colorMaterialPalette;

        public void SetReferences(TMP_FontAsset fontAsset, ColorMaterialPalette colorMaterialPalette)
        {
            _fontAsset = fontAsset;
            _colorMaterialPalette = colorMaterialPalette;
        }
    }
}

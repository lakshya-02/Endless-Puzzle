using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessPuzzle.Data
{
    [CreateAssetMenu(fileName = "ColorMaterialPalette", menuName = "Endless Puzzle/Color Material Palette")]
    public sealed class ColorMaterialPalette : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string colorId;
            public Material material;
        }

        [SerializeField] private Material _fallbackBaseMaterial;
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        private Dictionary<string, Material> _lookup;

        public Material FallbackBaseMaterial => _fallbackBaseMaterial;
        public Entry[] Entries => _entries;

        public bool TryGetMaterial(string colorId, out Material material)
        {
            EnsureLookup();
            return _lookup.TryGetValue(colorId, out material) && material != null;
        }

        public void SetFallbackBaseMaterial(Material material)
        {
            _fallbackBaseMaterial = material;
        }

        public void SetEntries(Entry[] entries)
        {
            _entries = entries ?? Array.Empty<Entry>();
            _lookup = null;
        }

        private void EnsureLookup()
        {
            if (_lookup != null)
            {
                return;
            }

            _lookup = new Dictionary<string, Material>(_entries.Length, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _entries.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(_entries[i].colorId) || _entries[i].material == null)
                {
                    continue;
                }

                _lookup[_entries[i].colorId.Trim()] = _entries[i].material;
            }
        }
    }
}

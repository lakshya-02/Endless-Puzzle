using UnityEngine;
using UnityEngine.Rendering;

namespace EndlessPuzzle.Gameplay
{
    public static class TargetObjectFactory
    {
        public static TargetObject CreatePrototype(Transform parent, Font worldLabelFont)
        {
            GameObject root = new GameObject("TargetObject");
            root.SetActive(false);
            root.transform.SetParent(parent, false);

            TargetObject targetObject = root.AddComponent<TargetObject>();
            BoxCollider boxCollider = root.AddComponent<BoxCollider>();
            SphereCollider sphereCollider = root.AddComponent<SphereCollider>();
            CapsuleCollider capsuleCollider = root.AddComponent<CapsuleCollider>();

            boxCollider.size = Vector3.one;
            sphereCollider.radius = 0.5f;
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 2f;

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            MeshFilter meshFilter = visual.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = visual.AddComponent<MeshRenderer>();

            GameObject label = new GameObject("Label");
            label.transform.SetParent(root.transform, false);
            TextMesh labelText = label.AddComponent<TextMesh>();
            labelText.anchor = TextAnchor.LowerCenter;
            labelText.alignment = TextAlignment.Center;
            labelText.fontSize = 96;
            labelText.characterSize = 0.055f;
            labelText.color = Color.white;
            labelText.text = string.Empty;
            if (worldLabelFont != null)
            {
                labelText.font = worldLabelFont;
            }

            MeshRenderer labelRenderer = label.GetComponent<MeshRenderer>();
            labelRenderer.shadowCastingMode = ShadowCastingMode.Off;
            labelRenderer.receiveShadows = false;
            if (worldLabelFont != null && worldLabelFont.material != null)
            {
                labelRenderer.sharedMaterial = worldLabelFont.material;
            }

            targetObject.CacheReferences(
                visual.transform,
                label.transform,
                meshFilter,
                meshRenderer,
                labelText,
                boxCollider,
                sphereCollider,
                capsuleCollider);

            return targetObject;
        }
    }
}

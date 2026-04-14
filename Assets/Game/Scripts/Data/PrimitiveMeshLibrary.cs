using UnityEngine;

namespace EndlessPuzzle.Data
{
    public sealed class PrimitiveMeshLibrary
    {
        private readonly Mesh[] _meshes;

        public PrimitiveMeshLibrary()
        {
            _meshes = new Mesh[4];
            _meshes[(int)ShapePrimitiveType.Cube] = ExtractMesh(PrimitiveType.Cube);
            _meshes[(int)ShapePrimitiveType.Sphere] = ExtractMesh(PrimitiveType.Sphere);
            _meshes[(int)ShapePrimitiveType.Capsule] = ExtractMesh(PrimitiveType.Capsule);
            _meshes[(int)ShapePrimitiveType.Cylinder] = ExtractMesh(PrimitiveType.Cylinder);
        }

        public Mesh GetMesh(ShapePrimitiveType primitiveType)
        {
            return _meshes[(int)primitiveType];
        }

        private static Mesh ExtractMesh(PrimitiveType primitiveType)
        {
            GameObject temp = GameObject.CreatePrimitive(primitiveType);
            temp.hideFlags = HideFlags.HideAndDontSave;
            Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            return mesh;
        }
    }
}

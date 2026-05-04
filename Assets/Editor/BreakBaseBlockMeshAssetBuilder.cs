using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class BreakBaseBlockMeshAssetBuilder
{
    const string SourceAssetPath = "Assets/06_Prefab/Block/BreakBaseBlockSource.fbx";
    const string TargetMeshPath = "Assets/06_Prefab/Block/BreakBaseBlockCombined.asset";
    const float DefaultRange = 1f;

    [InitializeOnLoadMethod]
    static void AutoBuildMissingAsset()
    {
        EditorApplication.delayCall += TryBuildDefaultAssetIfMissing;
    }

    [MenuItem("Tools/Break Block/Rebuild Combined Mesh Asset")]
    public static void RebuildCombinedMeshAsset()
    {
        BuildCombinedMeshAsset(DefaultRange);
    }

    static void TryBuildDefaultAssetIfMissing()
    {
        if (AssetDatabase.LoadAssetAtPath<Mesh>(TargetMeshPath) != null)
            return;

        BuildCombinedMeshAsset(DefaultRange);
    }

    static void BuildCombinedMeshAsset(float range)
    {
        GameObject sourceRoot = AssetDatabase.LoadAssetAtPath<GameObject>(SourceAssetPath);
        if (sourceRoot == null)
        {
            Debug.LogError($"Source break block asset not found: {SourceAssetPath}");
            return;
        }

        MeshFilter[] meshFilters = sourceRoot.GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> validFilters = new List<MeshFilter>(meshFilters.Length);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter filter = meshFilters[i];
            if (filter == null || filter.sharedMesh == null)
                continue;

            Renderer renderer = filter.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            validFilters.Add(filter);
        }

        if (validFilters.Count == 0)
        {
            Debug.LogError($"No shard meshes found under {SourceAssetPath}");
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uv0 = new List<Vector2>();
        List<Vector4> uv2 = new List<Vector4>();
        List<Vector4> uv3 = new List<Vector4>();
        List<Color32> colors = new List<Color32>();
        List<int> triangles = new List<int>();

        Matrix4x4 worldToRoot = sourceRoot.transform.worldToLocalMatrix;

        for (int shardIndex = 0; shardIndex < validFilters.Count; shardIndex++)
        {
            MeshFilter filter = validFilters[shardIndex];
            Mesh sourceMesh = filter.sharedMesh;
            Matrix4x4 localToRoot = worldToRoot * filter.transform.localToWorldMatrix;
            Matrix4x4 directionMatrix = localToRoot.inverse.transpose;
            int vertexOffset = vertices.Count;

            Vector3 shardCenter = sourceRoot.transform.InverseTransformPoint(filter.transform.position);
            float magnitude = shardCenter.magnitude;
            Vector3 direction = magnitude > 0f ? shardCenter / magnitude : Vector3.zero;
            Vector3 moveVector = direction * magnitude * range;

            Vector3[] sourceVertices = sourceMesh.vertices;
            Vector3[] sourceNormals = sourceMesh.normals;
            Vector4[] sourceTangents = sourceMesh.tangents;
            Vector2[] sourceUv0 = sourceMesh.uv;
            Color32[] sourceColors = sourceMesh.colors32;

            for (int vertexIndex = 0; vertexIndex < sourceVertices.Length; vertexIndex++)
            {
                vertices.Add(localToRoot.MultiplyPoint3x4(sourceVertices[vertexIndex]));

                if (sourceNormals != null && sourceNormals.Length == sourceVertices.Length)
                    normals.Add(directionMatrix.MultiplyVector(sourceNormals[vertexIndex]).normalized);
                else
                    normals.Add(Vector3.up);

                if (sourceTangents != null && sourceTangents.Length == sourceVertices.Length)
                {
                    Vector4 tangent = sourceTangents[vertexIndex];
                    Vector3 tangentDirection = directionMatrix.MultiplyVector(new Vector3(tangent.x, tangent.y, tangent.z)).normalized;
                    tangents.Add(new Vector4(tangentDirection.x, tangentDirection.y, tangentDirection.z, tangent.w));
                }
                else
                {
                    tangents.Add(new Vector4(1f, 0f, 0f, 1f));
                }

                if (sourceUv0 != null && sourceUv0.Length == sourceVertices.Length)
                    uv0.Add(sourceUv0[vertexIndex]);
                else
                    uv0.Add(Vector2.zero);

                uv2.Add(new Vector4(shardCenter.x, shardCenter.y, shardCenter.z, 1f));
                uv3.Add(new Vector4(moveVector.x, moveVector.y, moveVector.z, shardIndex));

                if (sourceColors != null && sourceColors.Length == sourceVertices.Length)
                    colors.Add(sourceColors[vertexIndex]);
                else
                    colors.Add(new Color32(255, 255, 255, 255));
            }

            for (int subMeshIndex = 0; subMeshIndex < sourceMesh.subMeshCount; subMeshIndex++)
            {
                int[] sourceTriangles = sourceMesh.GetTriangles(subMeshIndex);
                for (int triangleIndex = 0; triangleIndex < sourceTriangles.Length; triangleIndex++)
                    triangles.Add(vertexOffset + sourceTriangles[triangleIndex]);
            }
        }

        Mesh combinedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(TargetMeshPath);
        if (combinedMesh == null)
        {
            combinedMesh = new Mesh
            {
                name = "BreakBaseBlockCombined"
            };
            AssetDatabase.CreateAsset(combinedMesh, TargetMeshPath);
        }
        else
        {
            combinedMesh.Clear();
        }

        combinedMesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        combinedMesh.SetVertices(vertices);
        combinedMesh.SetNormals(normals);
        combinedMesh.SetTangents(tangents);
        combinedMesh.SetUVs(0, uv0);
        combinedMesh.SetUVs(1, uv2);
        combinedMesh.SetUVs(2, uv3);
        combinedMesh.SetColors(colors);
        combinedMesh.SetTriangles(triangles, 0);
        combinedMesh.RecalculateBounds();

        EditorUtility.SetDirty(combinedMesh);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Built combined break block mesh asset: {TargetMeshPath}");
    }
}

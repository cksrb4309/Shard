using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Test_Break_6_Builder : MonoBehaviour
{
    [SerializeField] Transform shardRoot;
    [SerializeField] MeshFilter combinedMeshFilter;
    [SerializeField] MeshRenderer combinedMeshRenderer;
    [SerializeField] MeshCollider combinedMeshCollider;
    [SerializeField] float range = 1f;
    [SerializeField] bool disableSourceRenderers;
    [SerializeField] bool disableSourceColliders;
    [SerializeField, ReadOnly] int shardCount;
    [SerializeField, ReadOnly] int combinedVertexCount;
    [SerializeField, ReadOnly] Vector3[] shardCenters;
    [SerializeField, ReadOnly] Vector3[] shardMoveVectors;

    Mesh generatedMesh;

    [Button]
    public void BuildCombinedMesh()
    {
        if (!ResolveTargets())
            return;

        MeshFilter[] sourceFilters = shardRoot.GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> validFilters = new List<MeshFilter>(sourceFilters.Length);
        List<Renderer> sourceRenderers = new List<Renderer>(sourceFilters.Length);
        List<Collider> sourceColliders = new List<Collider>(sourceFilters.Length);

        for (int i = 0; i < sourceFilters.Length; i++)
        {
            MeshFilter filter = sourceFilters[i];
            if (filter == null || filter == combinedMeshFilter || filter.sharedMesh == null)
                continue;

            if (combinedMeshFilter != null && filter.transform == combinedMeshFilter.transform)
                continue;

            Renderer renderer = filter.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            validFilters.Add(filter);
            sourceRenderers.Add(renderer);
            sourceColliders.Add(filter.GetComponent<Collider>());
        }

        if (validFilters.Count == 0)
        {
            Debug.LogWarning("No shard MeshFilter found for combined mesh build.", this);
            return;
        }

        shardCount = validFilters.Count;
        shardCenters = new Vector3[shardCount];
        shardMoveVectors = new Vector3[shardCount];

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uv0 = new List<Vector2>();
        List<Vector4> uv2 = new List<Vector4>();
        List<Vector4> uv3 = new List<Vector4>();
        List<Color32> colors = new List<Color32>();
        List<int> triangles = new List<int>();

        Matrix4x4 worldToCombined = combinedMeshFilter.transform.worldToLocalMatrix;
        Material[] sharedMaterials = sourceRenderers[0].sharedMaterials;

        for (int shardIndex = 0; shardIndex < validFilters.Count; shardIndex++)
        {
            MeshFilter filter = validFilters[shardIndex];
            Mesh sourceMesh = filter.sharedMesh;
            Matrix4x4 localToCombined = worldToCombined * filter.transform.localToWorldMatrix;
            Matrix4x4 directionMatrix = localToCombined.inverse.transpose;
            int vertexOffset = vertices.Count;

            Vector3 shardCenter = combinedMeshFilter.transform.InverseTransformPoint(filter.transform.position);
            float magnitude = shardCenter.magnitude;
            Vector3 direction = magnitude > 0f ? shardCenter / magnitude : Vector3.zero;
            Vector3 moveVector = direction * magnitude * range;

            shardCenters[shardIndex] = shardCenter;
            shardMoveVectors[shardIndex] = moveVector;

            Vector3[] sourceVertices = sourceMesh.vertices;
            Vector3[] sourceNormals = sourceMesh.normals;
            Vector4[] sourceTangents = sourceMesh.tangents;
            Vector2[] sourceUv0 = sourceMesh.uv;
            Color32[] sourceColors = sourceMesh.colors32;

            for (int vertexIndex = 0; vertexIndex < sourceVertices.Length; vertexIndex++)
            {
                vertices.Add(localToCombined.MultiplyPoint3x4(sourceVertices[vertexIndex]));

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

                // uv2.xyz = shard center OS, uv3.xyz = shard move vector OS
                uv2.Add(new Vector4(shardCenter.x, shardCenter.y, shardCenter.z, 1f));
                uv3.Add(new Vector4(moveVector.x, moveVector.y, moveVector.z, shardIndex));

                if (sourceColors != null && sourceColors.Length == sourceVertices.Length)
                    colors.Add(sourceColors[vertexIndex]);
                else
                    colors.Add(new Color32(255, 255, 255, 255));
            }

            int subMeshCount = sourceMesh.subMeshCount;
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                int[] sourceTriangles = sourceMesh.GetTriangles(subMeshIndex);
                for (int triangleIndex = 0; triangleIndex < sourceTriangles.Length; triangleIndex++)
                    triangles.Add(vertexOffset + sourceTriangles[triangleIndex]);
            }
        }

        if (generatedMesh == null)
        {
            generatedMesh = new Mesh
            {
                name = $"{name}_BreakCombined"
            };
        }
        else
        {
            generatedMesh.Clear();
        }

        generatedMesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        generatedMesh.SetVertices(vertices);
        generatedMesh.SetNormals(normals);
        generatedMesh.SetTangents(tangents);
        generatedMesh.SetUVs(0, uv0);
        generatedMesh.SetUVs(1, uv2);
        generatedMesh.SetUVs(2, uv3);
        generatedMesh.SetColors(colors);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateBounds();

        combinedVertexCount = vertices.Count;
        combinedMeshFilter.sharedMesh = generatedMesh;
        combinedMeshRenderer.sharedMaterials = sharedMaterials;

        if (combinedMeshCollider != null)
            combinedMeshCollider.sharedMesh = generatedMesh;

        for (int i = 0; i < sourceRenderers.Count; i++)
        {
            if (disableSourceRenderers)
                sourceRenderers[i].enabled = false;

            if (disableSourceColliders && sourceColliders[i] != null)
                sourceColliders[i].enabled = false;
        }
    }

    [Button]
    public void RestoreSourceState()
    {
        if (shardRoot == null)
            shardRoot = transform;

        Renderer[] renderers = shardRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (combinedMeshRenderer != null && renderers[i] == combinedMeshRenderer)
                continue;

            renderers[i].enabled = true;
        }

        Collider[] colliders = shardRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (combinedMeshCollider != null && colliders[i] == combinedMeshCollider)
                continue;

            colliders[i].enabled = true;
        }
    }

    bool ResolveTargets()
    {
        if (shardRoot == null)
            shardRoot = transform;

        if (combinedMeshFilter == null)
            combinedMeshFilter = GetComponent<MeshFilter>();

        if (combinedMeshFilter == null)
            combinedMeshFilter = gameObject.AddComponent<MeshFilter>();

        if (combinedMeshRenderer == null)
            combinedMeshRenderer = GetComponent<MeshRenderer>();

        if (combinedMeshRenderer == null)
            combinedMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        return shardRoot != null && combinedMeshFilter != null && combinedMeshRenderer != null;
    }
}

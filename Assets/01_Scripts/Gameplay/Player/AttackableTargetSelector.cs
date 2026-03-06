using System.Collections.Generic;
using UnityEngine;

public class AttackableTargetSelector : MonoBehaviour
{
    private static AttackableTargetSelector s_instance;

    [Header("Physics Query")]
    [SerializeField] private LayerMask attackableLayers;

    [Tooltip("OverlapSphereNonAlloc 결과를 담는 고정 버퍼 크기. 꽉 차면(=동일) 결과가 잘릴 수 있음.")]
    [SerializeField, Min(1)] private int overlapBufferSize = 300;

    private static Collider[] s_overlapBuffer;

    [Header("Expanding Ring Search (Self / Position)")]
    [SerializeField, Min(0f)] private float ringMinRadius = 5f;
    [SerializeField, Min(0f)] private float ringMaxRadiusExclusive = 15f;
    [SerializeField, Min(0.001f)] private float ringStep = 4.5f;

    [Header("Expanding Ring Search (Exclude One)")]
    [SerializeField, Min(0f)] private float excludeMinRadius = 1f;
    [SerializeField, Min(0.001f)] private float excludeStep = 2f;

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Debug.LogError($"{nameof(AttackableTargetSelector)} has multiple instances. Destroying duplicate.", this);
            Destroy(this);
            return;
        }

        s_instance = this;

        // 버퍼는 "프로젝트 전체 1개"로 공유. (설정한 크기 기준으로 최초 1회만 생성)
        if (s_overlapBuffer == null || s_overlapBuffer.Length != overlapBufferSize)
        {
            s_overlapBuffer = new Collider[overlapBufferSize];
        }
    }

    // ----------------------------------------------------------------------
    // Instance guard
    // ----------------------------------------------------------------------

    private static bool HasInstance()
    {
        if (s_instance != null) return true;
        Debug.LogError($"{nameof(AttackableTargetSelector)} instance is missing in scene.");
        return false;
    }

    private static void WarnIfTruncated(int hitCount)
    {
        // hitCount == bufferSize면 결과가 잘렸을 가능성이 큼 (Unity 문서 관례)
        if (s_overlapBuffer != null && hitCount == s_overlapBuffer.Length)
        {
            Debug.LogWarning(
                $"{nameof(AttackableTargetSelector)} overlap buffer is full (size={s_overlapBuffer.Length}). " +
                "Some results may have been truncated. Consider increasing overlapBufferSize.");
        }
    }

    // ----------------------------------------------------------------------
    // Public API 
    // ----------------------------------------------------------------------

    /// <summary>
    /// 내 위치 기준으로, "가장 가까운 반지름 링"에서 살아있는 타겟을 균등확률로 1개 뽑는다.
    /// (링 내부 선택은 reservoir sampling으로 List 할당 없이 처리)
    /// </summary>
    public static IAttackable GetRandomAliveTargetNearSelf()
    {
        if (!HasInstance()) return null;
        return s_instance.GetRandomAliveTargetNearPosition(s_instance.transform.position);
    }

    /// <summary>
    /// position 기준으로 링을 확장해가며, 처음 발견된 링에서 살아있는 타겟을 "첫 번째로" 반환한다.
    /// </summary>
    public static IAttackable GetFirstAliveTargetInExpandingRings(Vector3 position)
    {
        if (!HasInstance()) return null;

        for (float radius = s_instance.ringMinRadius; radius < s_instance.ringMaxRadiusExclusive; radius += s_instance.ringStep)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(position, radius, s_overlapBuffer, s_instance.attackableLayers);
            WarnIfTruncated(hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                var attackable = s_overlapBuffer[i].GetComponent<IAttackable>();
                if (attackable != null && attackable.IsAlive())
                    return attackable;
            }
        }

        return null;
    }

    /// <summary>
    /// position 중심, maxRadius 반경 내 IAttackable을 results에 채운다.
    /// 반환값: results에 채워진 개수
    /// </summary>
    public static int CollectTargetsInRadiusNonAlloc(Vector3 position, float maxRadius, List<IAttackable> results)
    {
        if (!HasInstance()) return 0;

        if (results == null)
        {
            Debug.LogError("results list is null.");
            return 0;
        }

        results.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(
            position, maxRadius, s_overlapBuffer, s_instance.attackableLayers);

        WarnIfTruncated(hitCount);

        for (int i = 0; i < hitCount; i++)
        {
            var attackable = s_overlapBuffer[i].GetComponent<IAttackable>();
            if (attackable != null)
                results.Add(attackable);
        }

        return results.Count;
    }

    /// <summary>
    /// position 중심, maxRadius 반경 내에서 ignored(집합)을 제외하고 살아있는 타겟 중 랜덤 1개.
    /// (ignored가 null이면 무시 처리 없이 동작)
    /// </summary>
    public static IAttackable GetRandomAliveTargetInRadiusExcludingSet(
        Vector3 position,
        float maxRadius,
        HashSet<IAttackable> ignored)
    {
        if (!HasInstance()) return null;

        int hitCount = Physics.OverlapSphereNonAlloc(position, maxRadius, s_overlapBuffer, s_instance.attackableLayers);
        WarnIfTruncated(hitCount);

        // List 할당 없이 reservoir sampling으로 후보 중 랜덤 1개 선택
        IAttackable chosen = null;
        int candidates = 0;

        for (int i = 0; i < hitCount; i++)
        {
            var attackable = s_overlapBuffer[i].GetComponent<IAttackable>();
            if (attackable == null || !attackable.IsAlive())
                continue;

            if (ignored != null && ignored.Contains(attackable))
                continue;

            candidates++;
            if (Random.Range(0, candidates) == 0)
                chosen = attackable;
        }

        return chosen;
    }

    /// <summary>
    /// position 기준으로 반지름을 확장해가며(ignoreTarget 제외) 살아있는 타겟 중 랜덤 1개를 반환.
    /// (처음으로 후보가 생긴 링에서 즉시 reservoir sampling 결과를 반환)
    /// </summary>
    public static IAttackable GetRandomAliveTargetInExpandingRingsExcluding(
        Vector3 position,
        float maxRadius,
        IAttackable ignoreTarget)
    {
        if (!HasInstance()) return null;

        for (float radius = s_instance.excludeMinRadius; radius <= maxRadius; radius += s_instance.excludeStep)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(position, radius, s_overlapBuffer, s_instance.attackableLayers);
            WarnIfTruncated(hitCount);

            IAttackable chosen = null;
            int candidates = 0;

            for (int i = 0; i < hitCount; i++)
            {
                var attackable = s_overlapBuffer[i].GetComponent<IAttackable>();
                if (attackable == null || !attackable.IsAlive())
                    continue;

                if (ReferenceEquals(ignoreTarget, attackable))
                    continue;

                candidates++;
                if (Random.Range(0, candidates) == 0)
                    chosen = attackable;
            }

            if (chosen != null)
                return chosen;
        }

        return null;
    }

    // ----------------------------------------------------------------------
    // Internal
    // ----------------------------------------------------------------------

    private IAttackable GetRandomAliveTargetNearPosition(Vector3 position)
    {
        // 링을 확장해가며 "처음 타겟이 발견된 링" 안에서 reservoir sampling으로 균등확률 1개 선택
        for (float radius = ringMinRadius; radius < ringMaxRadiusExclusive; radius += ringStep)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(position, radius, s_overlapBuffer, attackableLayers);
            WarnIfTruncated(hitCount);

            if (hitCount <= 0)
                continue;

            IAttackable chosen = null;
            int aliveCandidateCount = 0;

            for (int i = 0; i < hitCount; i++)
            {
                var attackable = s_overlapBuffer[i].GetComponent<IAttackable>();
                if (attackable == null || !attackable.IsAlive())
                    continue;

                aliveCandidateCount++;

                // reservoir sampling: 지금까지 본 후보들 중 1개를 균등확률로 선택
                if (Random.Range(0, aliveCandidateCount) == 0)
                    chosen = attackable;
            }

            if (chosen != null)
                return chosen;
        }

        return null;
    }
}
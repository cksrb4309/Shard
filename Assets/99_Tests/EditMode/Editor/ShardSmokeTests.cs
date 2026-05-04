using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Shard 핵심 타입 존재 확인 및 순수 로직 스모크 테스트.
/// reflection 기반으로 Assembly-CSharp 의존 없이 실행 가능.
/// </summary>
public class ShardSmokeTests
{
    // AppDomain.CurrentDomain.Load()는 에디터 환경에서 FileLoadException을 던질 수 있다.
    // GetAssemblies()로 이미 로드된 어셈블리를 탐색하는 것이 안전하다.
    private static Assembly GetRuntimeAssembly()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
    }

    // ── 타입 존재 확인 ─────────────────────────────────────

    [Test]
    public void CoreTypes_ExistInAssembly()
    {
        var assembly = GetRuntimeAssembly();
        Assert.IsNotNull(assembly, "Assembly-CSharp 어셈블리가 도메인에 로드되지 않음");

        Assert.IsNotNull(assembly.GetType("AbilityManager"),       "AbilityManager 타입이 없음");
        Assert.IsNotNull(assembly.GetType("SpawnManager"),         "SpawnManager 타입이 없음");
        Assert.IsNotNull(assembly.GetType("PoolingManager"),       "PoolingManager 타입이 없음");
        Assert.IsNotNull(assembly.GetType("AbilityNameToIdMapper"), "AbilityNameToIdMapper 타입이 없음");
    }

    [Test]
    public void AbilityEventType_HasExpectedValues()
    {
        var assembly = GetRuntimeAssembly();
        Assert.IsNotNull(assembly, "Assembly-CSharp 어셈블리가 도메인에 로드되지 않음");

        var enumType = assembly.GetType("AbilityEventType");

        Assert.IsNotNull(enumType, "AbilityEventType enum이 없음");
        Assert.IsTrue(enumType.IsEnum, "AbilityEventType이 enum이 아님");

        var names = Enum.GetNames(enumType);
        Assert.Contains("Attack",   names, "AbilityEventType.Attack가 없음");
        Assert.Contains("Critical", names, "AbilityEventType.Critical가 없음");
        Assert.Contains("Kill",     names, "AbilityEventType.Kill가 없음");
        Assert.Contains("Passive",  names, "AbilityEventType.Passive가 없음");
    }

    // ── 순수 로직 테스트 ───────────────────────────────────

    [Test]
    public void XPLoop_MultiLevelUp_Math()
    {
        // PlayerLevelManager.AddExperienceApply 와 동일한 루프 로직을 순수 C#으로 검증한다.
        float xp = 0f;
        float xpToNext = 100f;
        int level = 1;

        xp += 350f; // 100 + 120 < 350 < 100 + 120 + 144 → 레벨업 2회 (Lv1→Lv3)

        while (xp > xpToNext)
        {
            xp -= xpToNext;
            xpToNext *= 1.2f;
            level++;
        }

        Assert.AreEqual(2, level - 1, "350XP로 레벨업이 정확히 2회 발생해야 한다 (Lv1→Lv3)");
        Assert.Greater(xpToNext, 100f, "레벨업마다 필요 XP가 증가해야 한다");
    }

    [Test]
    public void CostMultiplier_CoreUpgrade_Escalates()
    {
        // CoreInteractUI의 강화 비용 곱셈을 순수 로직으로 검증한다.
        int cost = 10;
        float multiplier = 1.5f;

        int firstUpgradeCost = cost;
        cost = (int)(cost * multiplier);   // 15
        int secondUpgradeCost = cost;
        cost = (int)(cost * multiplier);   // 22

        Assert.Less(firstUpgradeCost, secondUpgradeCost, "강화 비용이 누적될수록 증가해야 한다");
        Assert.Less(secondUpgradeCost, cost,              "세 번째 강화 비용도 두 번째보다 커야 한다");
    }
}

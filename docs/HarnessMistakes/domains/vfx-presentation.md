# Domain: VFX Presentation

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/domains/README]]

## Scope

VFX, view manager, presentation bridge, runtime visual event 소비, 렌더링 표현.
Shard: ParticleManager, VFX prefab, 이펙트 풀링.

## Typical Areas

- `Assets/01_Scripts/Manager/ParticleManager.cs`
- `Assets/04_Effects/**`
- `Assets/04_Material/**`

## Routing Metadata

- Preload extra mistake context: no
- Preload category files: none

## Known Recurring Mistakes

- none recorded

## Update Rule

시각 이벤트 흐름, view lifecycle, 표현 계층 경계 실수가 반복되면 이 파일을 갱신하고 preload를 켠다.

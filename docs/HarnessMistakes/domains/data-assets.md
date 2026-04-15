# Domain: Data Assets

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/domains/README]]

## Scope

ScriptableObject, config asset, unlock/stat/tuning 데이터, 참조 일관성.
Shard: `Assets/12.ScriptableObject/` 전체 (Ability, Spawn, Ship).

## Typical Areas

- `Assets/12.ScriptableObject/Ability/**`
- `Assets/12.ScriptableObject/Spawn/**`
- `Assets/12.ScriptableObject/Ship/**`

## Routing Metadata

- Preload extra mistake context: no
- Preload category files: none

## Known Recurring Mistakes

- none recorded

## Update Rule

데이터 자산 경로, 참조, 수치 적용 실수가 반복되면 이 파일을 갱신하고 preload를 켠다.

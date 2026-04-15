# Domain: Combat Gameplay

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/domains/README]]

## Scope

전투 로직, Ability 시스템, 적/플레이어 상태, 피해/사망/경험치/성장 루프.
Shard 핵심: AbilityManager, SpawnManager, Monster, Player, Inventory, PlayerLevelManager.

## Typical Areas

- `Assets/01_Scripts/Gameplay/Ability/**`
- `Assets/01_Scripts/Gameplay/Player/**`
- `Assets/01_Scripts/Gameplay/Monster/**`
- `Assets/01_Scripts/Manager/PlayerLevelManager.cs`
- `Assets/01_Scripts/Manager/SpawnManager.cs`

## Routing Metadata

- Preload extra mistake context: no
- Preload category files: none

## Known Recurring Mistakes

- none recorded

## Update Rule

이 도메인에서 재발 가능한 실수가 생기면:
- primary category를 기록하고
- 이 파일의 `Known Recurring Mistakes`를 요약으로 갱신하고
- 필요 시 `Preload extra mistake context`를 `yes`로 바꾸고 category 파일을 명시한다

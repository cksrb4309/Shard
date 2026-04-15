# Harness Mistake Domains

작업 시작 시 모든 실수 파일을 다 읽지 않기 위한 라우팅 문서입니다.

## Navigation

- Root hub: [[docs/HarnessMistakes/README]]
- Category index: [[docs/HarnessMistakes/categories/README]]

## 사용 규칙

1. 먼저 현재 작업의 primary domain을 1개 고릅니다.
2. 해당 domain 파일 하나만 읽습니다.
3. 그 domain 파일에 `Preload extra mistake context: no` 라고 적혀 있으면 category 파일은 더 읽지 않습니다.
4. `Preload extra mistake context: yes` 라고 적혀 있으면 `Preload category files`에 적힌 category 파일만 읽습니다.

## Domain Index

- [[docs/HarnessMistakes/domains/combat-gameplay]]
  - 전투 로직, Ability/적/플레이어 상태, 피해/사망/경험치/성장 루프
- [[docs/HarnessMistakes/domains/ui-bridge]]
  - UI 표시, 입력 브리지, observable sync, controller/binder
- [[docs/HarnessMistakes/domains/scene-prefab]]
  - 씬, 프리팹, 인스펙터 연결, missing reference, 배치/구조 변경
- [[docs/HarnessMistakes/domains/data-assets]]
  - ScriptableObject, config asset, unlock/stat tuning, address/reference consistency
- [[docs/HarnessMistakes/domains/vfx-presentation]]
  - VFX, presentation manager, view object, runtime visual bridge
- [[docs/HarnessMistakes/domains/validation-tooling]]
  - compile/smoke/test, editor menu validation, automation, batchmode limits
- [[docs/HarnessMistakes/domains/docs-harness]]
  - README, AGENTS, CLAUDE, prompt templates, mistake docs 자체

# Scope Boundary Mistakes

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/categories/README]]

## Definition

허용되지 않은 파일이나 자산을 건드렸거나, 필요한 범위를 넘어서 수정했거나, unrelated user changes를 훼손한 경우입니다.

## Typical Signals

- guarded asset을 명시적 허용 없이 수정함
- code-only 작업인데 씬/프리팹/ScriptableObject를 함께 수정함
- 사용자 변경을 되돌리거나 덮어씀

## Prevention Rules

- 기본값은 항상 code-only scope
- guarded asset은 명시적 허용 또는 강한 필요성이 있을 때만
- dirty serialized asset은 task와 직접 관련이 없으면 건드리지 않음

## Incident Log

<!-- 새 항목은 이 줄 아래에 추가한다 -->

# Behavioral Regression Mistakes

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/categories/README]]

## Definition

최소 안전 게이트는 통과했지만, 사용자가 의도한 동작을 놓쳤거나 실제 플레이 동작이 깨진 경우입니다.

## Typical Signals

- compile/smoke는 통과하지만 기능이 원하는 대로 동작하지 않음
- 새 기능이 기존 흐름을 조용히 깨뜨림
- task-specific acceptance check가 빠져 있었음

## Prevention Rules

- 기능별 완료 조건을 더 명확히 기록
- deterministic test가 가능하면 반드시 추가
- 자동 검증이 부족하면 수동 확인 항목을 명시

## Incident Log

<!-- 새 항목은 이 줄 아래에 추가한다 -->

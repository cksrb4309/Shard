# Communication Handoff Mistakes

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/categories/README]]

## Definition

사용자에게 확인을 받아야 할 지점, 수동 실행 요청, 상태 보고, 완료 판정 전달이 부정확해서 생긴 실수입니다.

## Typical Signals

- 사용자가 무엇을 해야 하는지 타이밍이 모호함
- 완료라고 했지만 실제로는 사용자 실행 대기 상태였음
- 질문해야 할 상황에서 질문하지 않음

## Prevention Rules

- 수동 단계가 있으면 정확한 타이밍과 기대 결과를 명시
- 완료, 대기, 미검증 상태를 분리해서 보고
- 사용자가 개입해야 할 단계는 명령형으로 짧고 분명하게 안내

## Incident Log

### YYYY-MM-DD - [제목]
- Trigger:
- Impact:
- Detection:
- Rule Change:
- Validation Change:
- Follow-up Files:

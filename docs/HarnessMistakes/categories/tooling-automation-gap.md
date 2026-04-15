# Tooling Automation Gap Mistakes

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/categories/README]]

## Definition

도구 동작 방식, 배치 검증 한계, 에디터 상태, 자동화 가능 범위를 잘못 이해해서 생긴 실수입니다.

## Typical Signals

- 에디터가 열린 상태에서도 batchmode가 될 거라고 잘못 가정
- GUI 메뉴를 에이전트가 직접 실행할 수 있다고 오해
- 실제 도구 동작과 다른 설명을 함

## Prevention Rules

- 도구가 할 수 있는 것과 못 하는 것을 분리해서 설명
- 에디터 열림/닫힘 경로를 문서에 명시
- 반자동 단계가 있으면 언제 사용자 개입이 필요한지 분명히 적음

## Incident Log

<!-- 새 항목은 이 줄 아래에 추가한다 -->

# Validation Gap Mistakes

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/categories/README]]

## Definition

수행했어야 할 검증을 생략했거나, 불충분한 검증만으로 완료 처리한 경우입니다.

## Typical Signals

- compile만 하고 smoke/test를 생략함
- 에디터가 열려 있어서 검증을 못 돌렸는데도 완료처럼 보고함
- 프로젝트 특화 리스크에 맞는 검증을 추가하지 않음

## Prevention Rules

- 최소 게이트를 명시하고 결과를 보고
- 검증을 못 돌리면 이유와 남은 리스크를 같이 보고
- 재발 가능성이 있으면 smoke/test/menu path를 강화

## Incident Log

<!-- 새 항목은 이 줄 아래에 추가한다 -->

# 서브 에이전트

이 문서는 이 프로젝트에서 내부 서브 에이전트 또는 병렬 위임 작업을 사용하는 방법을 정의한다.

cross-agent handoff와는 다르다:
- `docs/AgentHandoffs/**` — Codex와 Claude Code 사이의 최상위 handoff용
- `docs/SubAgents.md` — 하나의 작업 세션 내 내부 위임 규칙용

## 활성화 조건

아래 두 조건이 모두 충족될 때만 사용한다:
- 현재 플랫폼/세션이 delegation을 지원하고,
- 사용자가 서브 에이전트 또는 병렬 작업을 허용했을 때

## 메인 에이전트 책임

- critical path
- 모호한 설계 결정
- guarded serialized asset
- 최종 코드 통합
- 최종 검증
- 사용자에 대한 최종 보고

## 위임 적합 대상

- read-only 코드베이스 탐색
- 좁은 서브시스템 또는 파일 클러스터 추적
- 빌드/테스트/로그 실패 triage
- 문서, 위키, 요약 초안 작성
- 소유권이 명확히 분리된 bounded 코드 변경

## 위임 부적합 대상

- 파일 편집 영역이 겹치는 작업
- `.unity`, `.prefab`, `12.ScriptableObject/**` 수정
- 명시적 소유권 없는 광범위한 리팩토링

## 소유권 규칙

위임 전에 정확한 파일/폴더 범위, read-only 여부, 기대 출력물을 정의한다.
한 파일 영역에는 동시에 작성자가 하나여야 한다.

## Handoff 분리

`docs/AgentHandoffs/**`를 내부 서브 에이전트 노트 저장소로 사용하지 않는다.
서브 에이전트가 발견한 장기 지식은 `docs/ProjectWiki/**`에 저장한다.

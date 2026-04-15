# {{PROJECT_NAME}} 에이전트 가이드

<!-- 공유 프로젝트 dual-agent 기본값:
     Codex와 Claude Code를 함께 쓰는 프로젝트에서는 이 자기완결형 AGENTS를 기본으로 사용한다. -->

## 범위

이 저장소는 Unity `{{UNITY_VERSION}}` 프로젝트다.

이 섹션을 프로젝트 고유 설명으로 교체한다:

- 프로젝트가 무엇인지
- 현재 엔지니어링 포커스가 무엇인지
- 코드베이스가 변경되는 동안 안정적으로 유지해야 할 게임플레이 또는 툴링 루프가 무엇인지

비자명한 수정 전에 아래 파일을 읽는다:

- `README.md`
- `CLAUDE.md`
- `docs/AgentPromptTemplates.md`

`docs/Obsidian.md`가 있으면 저장소 루트를 문서 vault로 취급하고 하네스 문서는 `Assets/` 밖에 유지한다.
`graphify-out/GRAPH_REPORT.md`가 있으면 넓은 아키텍처 질문이나 저장소 전체 탐색 전에 읽는다.
`docs/Graphify.md`를 참고해서 그래프를 먼저 갱신해야 하는지 판단한다.
여러 폴더에 걸친 코드 접근 시, raw source를 읽기 전에 먼저 그래프(GRAPH_REPORT 또는 쿼리)로 가장 작은 관련 파일 집합을 파악한다.
긴 shell 출력이 예상되면 넓은 Bash 탐색이나 로그 중심 검증 전에 `docs/RTK.md`를 읽는다.
세션이 delegation을 지원하고 사용자가 허용했으면, bounded side task를 분리하기 전에 `docs/SubAgents.md`를 읽는다.

## 이미지 분석 파이프라인

이미지 분석 작업(UI 검증, 색상 확인, 비주얼 버그 등)에는 로컬 MCP 파이프라인을 사용한다.

- `ollama-vision` → 장면 설명, UI 구조, 텍스트 추출 (먼저 사용)
- `image-tools` → 픽셀 색상, 지배색, 거리 측정 (정밀 수치 필요 시만)

이 MCP는 전역(`~/.codex/config.toml`)에 등록한다. 프로젝트별 설정 불필요.
설정 방법 및 운영 규칙 전문: `.claude/image-analysis.md`

Ollama가 꺼져 있으면 파이프라인이 응답하지 않는다. 작업 전 `describe_image` 호출로 가용성을 확인한다.

## 폴더 맵

실제 프로젝트 구조를 이곳에 기술한다.

권장 항목:

- 주요 런타임 코드 폴더
- 능력/데이터/설정 폴더
- 표현/UI 폴더
- 씬/프리팹
- 테스트 폴더
- 서드파티 폴더

## 도구 사용 패턴

작업 시작 전 관련 파일을 전부 먼저 읽는다.

- **읽기 턴**: Read, Grep, Glob을 한 번에 병렬로 실행한다.
- **분석**: 읽은 내용을 바탕으로 수정 범위와 계획을 확정한다.
- **쓰기 턴**: Edit, Write, Bash를 순차 실행한다.

읽기와 쓰기를 여러 턴에 걸쳐 섞지 않는다.
파일 하나 읽고 수정하고를 반복하면 API 왕복이 선형으로 늘어난다.

## 그래프 파악

프로젝트가 `graphify`를 사용하는 경우:

- `graphify-out/GRAPH_REPORT.md`를 고수준 파악에 사용한다
- 구현 결정에는 raw source file을 사용한다
- `docs/Graphify.md`를 갱신 매뉴얼로 취급한다
- 하네스가 관리하는 프로젝트에서는 기본적으로 `graphify codex install`이나 `graphify claude install`을 실행하지 않는다

## Shell Output 압축

`docs/RTK.md`가 있는 경우:

- shell output 압축 운영 매뉴얼로 사용한다
- shell 작업이 많은 `git`, `rg`/`grep`, 테스트, batch 검증 로그에 RTK 경로를 우선한다
- 확인이나 사용자 확인 없이 RTK가 설치되어 있다고 가정하지 않는다
- RTK를 raw source 읽기, `unity-cli`, `unity-prefab-parser-mcp`의 대체재로 취급하지 않는다

## Shell/Bash 정책

훅과 RTK의 역할을 분리한다.

- 위험한 shell 명령은 훅에서 막는다
  - 예: `rm`, `Remove-Item -Recurse`, `git reset --hard`, `git clean -fdx`
  - generated artifact write나 Unity/generated path 직접 수정도 여기에 포함한다
- 출력이 길고 지저분한 shell 명령은 RTK로 압축한다
  - 예: `git diff`, `git log`, 테스트, build, batch validation 로그
- 안전하고 명시적인 shell 명령은 허용한다
  - 예: 프로젝트 도구 실행, 검증 명령, 읽기 전용 조회

## 기본 수정 경계

기본적으로 안전한 대상:

- 런타임 코드 폴더
- 테스트 폴더
- 에디터 툴링 폴더
- docs
- `tools/**`

명시적 요청이 필요한 대상:

- 씬
- 프리팹
- ScriptableObject 데이터
- 입력 에셋
- `ProjectSettings/**`
- 패키지 파일

해당 의존성과 직접 관련된 작업이 아니면 읽기 전용:

- 서드파티 코드
- `Library/**`
- `Temp/**`
- 사용자 설정
- 복구/캐시 폴더

에셋 복구 작업이 아니면 `.meta` 파일을 직접 편집하지 않는다.

## 기존 워크트리 규칙

이 프로젝트에는 씬, 프리팹, VFX, 데이터 에셋에 활성 사용자 변경이 있을 수 있다.

- 관련 없는 사용자 변경을 되돌리지 않는다.
- serialized asset이 이미 dirty 상태이고 현재 작업에 필요하지 않으면 그대로 둔다.
- serialized asset 편집은 진행 중인 수동 에디터 작업과 충돌할 수 있다고 가정한다.

## 기본 요청 해석

매번 전체 하네스 템플릿을 다시 입력하도록 요구하지 않는다.

사용자가 짧은 요청을 하면:

- 작업 목표를 추론한다
- 수정 전에 이 파일과 관련 코드를 읽는다
- code-only scope를 우선 가정한다
- 가장 작은 일치하는 검증 게이트를 선택한다
- 변경 파일, 검증 결과, 남은 수동 확인 항목을 보고한다

아래 경우에만 명확한 확인을 요청한다:

- 작업이 실질적으로 모호한 경우
- guarded serialized asset 편집이 필요할 가능성이 높은 경우
- 여러 게임플레이 해석이 서로 다른 구현을 의미하는 경우
- 사용자 결정 없이 검증으로 의도된 결과를 증명할 수 없는 경우

비자명한 작업 전에:

- `docs/HarnessMistakes/domains/README.md`를 사용해서 primary domain 하나를 분류한다
- 해당 domain 파일을 먼저 읽는다
- 그 domain 파일에 `Preload extra mistake context: yes`라고 적혀 있을 때만 추가 실수 category 파일을 읽는다

## Cross-Agent Handoff

`docs/AgentHandoffs/**`를 Codex와 Claude Code 사이의 좁은 연결 통로로 사용한다.

사용자가 Claude Code 작업 확인을 요청하면:

- `docs/AgentHandoffs/pending/claude-to-codex/`를 먼저 확인한다
- placeholder 파일만 있으면 handoff 컨텍스트를 더 읽지 않는다
- 관련 pending 노트만 최신순으로 읽는다
- 이해한 노트는 `docs/AgentHandoffs/consumed/claude-to-codex/`로 이동한다
- 사용자가 명시적으로 요청하지 않는 한 `consumed/**` 노트를 다시 읽지 않는다

장기 문서를 작성하거나 갱신할 때:

- `Assets/` 아래가 아닌 루트 docs 트리에 유지한다
- 안정적인 노트 이름을 선호한다
- 탐색이 실질적으로 개선될 때만 `[[wiki link]]`를 추가한다
- `docs/ProjectWiki/index.md`를 메인 허브 노트로 유지한다

Claude Code에 작업을 넘길 때:

- `docs/AgentHandoffs/pending/codex-to-claude/`에 간단한 노트를 작성한다
- 짧게 유지한다: 작업, 상태, 변경 파일, 검증, 다음 액션
- 같은 주제의 최신 unread 노트가 있으면 중복 파일을 새로 만들지 말고 그 노트를 갱신한다

## 서브 에이전트 위임

`docs/SubAgents.md`가 있고 현재 세션에서 delegation이 허용된 경우:

- blocking 설계 결정과 최종 검증은 메인 에이전트가 담당한다
- 서브 에이전트는 read-only 탐색, 로그/테스트 triage, 문서 초안 작성, 소유권이 분리된 코드 변경에 사용한다
- 코드 작성 작업을 위임하기 전에 명시적 소유권을 부여한다
- 여러 에이전트가 같은 파일이나 같은 guarded serialized asset surface를 동시에 편집하지 않는다
- `docs/AgentHandoffs/**`를 내부 서브 에이전트 결과 저장소로 사용하지 않는다

## 아키텍처 규칙

프로젝트 고유 규칙을 이곳에 작성한다.

일반적인 섹션:

- ECS 코어 또는 게임플레이 코어
- 브리지 / 표현 분리
- 데이터 / 능력 흐름
- 코드 스타일과 네이밍
- 성능 민감 규칙

## 고위험 영역

쉽게 깨지고 guarded surface로 취급해야 할 에셋과 폴더를 목록으로 작성한다.

예시:

- 주요 전투 씬
- 씬 설정 프리팹
- 입력 액션 에셋
- ScriptableObject 설정 폴더
- `ProjectSettings/EditorBuildSettings.asset`

## 검증 워크플로

프로젝트 고유 최소 게이트를 이곳에 기술한다.

일반적인 구조:

1. `tools\compile-unity.cmd`
2. `tools\smoke-unity.cmd`
3. 테스트가 있으면 `tools\test-editmode.cmd`
4. 씬 의존 동작을 건드리면 `tools\test-playmode.cmd`

Unity가 이미 열려 있을 때의 에디터 메뉴 경로도 기술한다.

## 실수 학습 루프

하네스 실수 또는 반복 가능한 에이전트 실수가 확인되면:

- 현재 primary domain을 유지한다
- `docs/HarnessMistakes/README.md`를 사용해서 primary category 하나를 분류한다
- 해당 `docs/HarnessMistakes/categories/*.md` 파일을 갱신한다
- 해당 `docs/HarnessMistakes/domains/*.md` 파일을 갱신한다
- 그 교훈이 일반 운영 규칙을 바꾼다면 이 파일, `CLAUDE.md`, 프롬프트 템플릿, 검증 도구, 테스트에 반영한다
- 어떤 domain/category 실수 파일을 갱신했는지 보고한다

## 스모크 검증 의도

이 프로젝트에서 smoke validator가 실제로 확인하는 내용을 구체적으로 기술한다.

## 태스크 라우팅 힌트

프로젝트 고유 예시를 작성한다:

- 순수 게임플레이 버그 수정
- UI 동기화 이슈
- 언락/설정 변경
- VFX 또는 표현 변경
- 툴링 또는 검증 작업

## 보고 기대값

작업 보고 시:

- 변경한 파일 목록을 기재한다
- 실행한 검증 커맨드를 명시한다
- 씬, 프리팹, VFX, ScriptableObject 에셋에서의 미검증 리스크를 명시한다

검증 결과 없이 Unity 작업이 완료됐다고 주장하지 않는다.

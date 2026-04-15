# {{PROJECT_NAME}}

<!-- 목표: 100줄 이하 유지. 이 파일은 매 세션 자동 로드된다.
     상황 의존적 상세 내용(시스템 구조, 비주얼 흐름, 데이터 흐름)은
     docs/project-context.md에 작성하고 여기서 포인터만 남긴다. -->

- Engine: Unity `{{UNITY_VERSION}}`
- Genre/Type: {{GENRE_OR_PROJECT_TYPE}}
- Architecture: {{PRIMARY_ARCHITECTURE}}

## 런타임 루프

<!-- 4~6줄 이내. 실제 게임플레이 또는 툴링 흐름을 단계로 요약. -->

1.
2.
3.
4.

## 폴더 맵

<!-- 최상위 폴더만. 세부 경로는 project-context.md에. -->

```
Assets/
├── {{RUNTIME_CODE_FOLDER}}/   — 런타임 코드
├── {{DATA_FOLDER}}/           — ScriptableObject / Config
├── {{PRESENTATION_FOLDER}}/   — VFX, UI, View
├── {{SCENES_FOLDER}}/         — 씬
├── {{PREFABS_FOLDER}}/        — 프리팹
├── 99_Tests/                  — 테스트
└── Editor/                    — 에디터 툴링
```

## 검증

<!-- 에디터 닫힌 상태 기준 우선순위. -->

```
tools\compile-unity.cmd
tools\smoke-unity.cmd
tools\test-editmode.cmd   (테스트 있을 때)
tools\validate-unity.cmd  (전체 검증)
```

에디터가 열려 있으면: `Tools/Harness Validation/Run Smoke Validation`

## 보호 에셋

<!-- 수정 전 명시적 허가가 필요한 경로. 구체적으로 기입. -->

- `Assets/{{MAIN_SCENE_PATH}}`
- `Assets/{{CORE_PREFAB_PATH}}`
- `Assets/{{CONFIG_DATA_PATH}}/**`
- `ProjectSettings/**`

## 참조

더 읽어야 할 파일:

- `AGENTS.md` — 수정 경계, 검증 워크플로, 아키텍처 규칙 (비자명한 작업 전 필독)
- `docs/project-context.md` — 시스템 구조, 비주얼 흐름, 데이터 흐름, 시스템 순서 (관련 작업 시에만 로드)
- `docs/ProjectWiki/index.md` — 누적된 시스템/에셋/ADR 지식 (시스템·에셋 관련 작업 시에만 로드)
- `docs/HarnessMistakes/domains/README.md` — 실수 컨텍스트 (코드 수정 포함 작업 시에만 로드)
- `docs/AgentHandoffs/README.md` — Codex ↔ Claude Code handoff 규칙 (다른 에이전트 작업 내역 확인 요청을 받았을 때만 로드)
- `docs/Obsidian.md` — Obsidian vault 운영 규칙 (문서 작업 시에만 로드)
- `docs/RTK.md` — RTK 전역 설치/확인/우회 규칙 (긴 shell output이 예상될 때만 로드)
- `docs/SubAgents.md` — delegation 허용 세션에서만 읽는 서브 에이전트 운영 규칙
- `docs/Graphify.md` — graphify 설치/갱신 규칙 (구조 질문 전 `graphify-out/GRAPH_REPORT.md`가 있으면 함께 확인)
- `graphify-out/GRAPH_REPORT.md` — 코드 접근 범위를 좁힐 때 먼저 참고
- `docs/AgentPromptTemplates.md` — 요청 템플릿

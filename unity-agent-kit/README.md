# Unity Agent Kit

이 폴더는 어떤 Unity 프로젝트에든 `에이전트 적용 스타터 팩`처럼 복사해 넣을 수 있게 만든 재사용 킷입니다.

목표는 사용자가 긴 설명을 다시 쓰지 않아도, 이 폴더를 읽은 Codex 또는 Claude Code가 해당 프로젝트에 맞는:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/AgentPromptTemplates.md`
- `docs/Obsidian.md`
- `docs/RTK.md`
- `docs/SubAgents.md`
- `docs/HarnessMistakes/**`
- `docs/AgentHandoffs/**`
- `docs/Graphify.md`
- `.claude/**`
- `.codex/**`
- `.mcp.json`
- `.claude/claude-desktop-config.md`
- `tools/*`
- `Assets/Editor/HarnessValidation/*`
- 필요 시 `Assets/99_Tests/*`

를 설치하거나 갱신하도록 만드는 것입니다.

## 권장 사용 흐름

1. Codex 또는 Claude Code에서 Unity 프로젝트를 연다.
2. 새 스레드를 연다.
3. 이 `unity-agent-kit` 폴더를 프로젝트 루트에 복사한다.
4. `Assets/` 아래에는 넣지 않는다. 문서를 `Assets/` 아래에 두면 Unity가 `.meta` 파일을 만들어 낭비가 생긴다.
5. 사용하는 에이전트에 맞는 시작 문서를 첫 메시지로 보낸다.
6. 에이전트가 공용 설치 규격을 읽고 프로젝트에 맞게 하네스를 적용한다.

기본값은 `shared-project dual-agent` 모드입니다.
즉, Codex로 시작해도 Claude Code용 프로젝트 로컬 설정까지 같이 맞추고, Claude Code로 시작해도 Codex가 바로 읽을 수 있는 문서 구조를 같이 맞춥니다.

## 빠른 시작 파일

- [START_HERE_CODEX.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\START_HERE_CODEX.md)
  - Codex 첫 요청문
- [START_HERE_CLAUDE_CODE.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\START_HERE_CLAUDE_CODE.md)
  - Claude Code 첫 요청문
- [AGENT_KIT_INSTALL_SPEC.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\AGENT_KIT_INSTALL_SPEC.md)
  - 공용 설치 규격
- [FOR_CODEX.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\FOR_CODEX.md)
  - Codex 전용 해석 지침
- [FOR_CLAUDE_CODE.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\FOR_CLAUDE_CODE.md)
  - Claude Code 전용 해석 지침
- `templates/docs/RTK.template.md`
  - 기본 shell output 압축 운영 문서
- `templates/docs/SubAgents.template.md`
  - 서브 에이전트 / 병렬 위임 운영 문서
- `templates/docs/Graphify.template.md`
  - 구조 이해용 knowledge graph 운영 문서 (필수)

## 이 폴더 안의 역할

- `START_HERE.md`
  - 전체 사용 개요와 에이전트별 시작 파일 안내
- `AGENT_KIT_INSTALL_SPEC.md`
  - 두 에이전트가 공통으로 따라야 하는 실제 설치 절차
- `FOR_CODEX.md`
  - Codex가 공용 설치 규격을 어떻게 적용할지에 대한 보조 지침
- `FOR_CLAUDE_CODE.md`
  - Claude Code가 공용 설치 규격을 어떻게 적용할지에 대한 보조 지침
- `templates/`
  - 문서, handoff 폴더, 검증 스크립트, 테스트, 에디터 메뉴의 재사용 템플릿
- `scripts/scan-unity-project.ps1`
  - 프로젝트 구조를 빠르게 읽기 위한 보조 스캔 스크립트
- `scripts/merge-claude-desktop-config.ps1`
  - Claude Desktop 전역 설정 파일의 `mcpServers`를 안전하게 merge하는 보조 스크립트
- [THIRD_PARTY_NOTICES.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\THIRD_PARTY_NOTICES.md)
  - 외부 도구 참조와 라이선스/고지 범위 정리

## Obsidian-Friendly Default

이 킷은 Unity 프로젝트 루트를 Obsidian vault로 열 수 있는 문서 구조를 기본값으로 둔다.

- 문서 계층은 `Assets/` 밖의 루트에 둔다.
- 새 프로젝트 초기화 시 `docs/Obsidian.md`를 기본 생성한다.
- `docs/ProjectWiki/index.md`를 허브 note로 사용한다.
- 장기 지식은 `docs/ProjectWiki/**`, 짧은 handoff는 `docs/AgentHandoffs/**`로 분리한다.
- `docs/HarnessMistakes/**`는 root/domain/category 허브 링크로 한 덩어리로 보이게 유지한다.
- wiki-link는 필요한 곳에만 쓰고, note 이름은 자주 바꾸지 않는다.
- Obsidian `Excluded files`에는 최소한 `Library/**`, `Temp/**`, `Logs/**`, `Obj/**`, `UserSettings/**`, `.git/**`를 권장한다.

## Default: RTK

이 킷은 `rtk`를 기본 포함되는 선택적 최적화 계층으로 다룬다.

- 역할:
  - 긴 shell command 출력을 LLM 컨텍스트에 들어가기 전에 압축
  - `git`, 테스트 로그, `rg`/`grep`, 배치 검증 로그처럼 noise가 큰 출력 절감
- 대체하지 않는 것:
  - `AGENTS.md`, `CLAUDE.md`
  - 하네스 검증 게이트
  - `graphify` 기반 구조 이해
  - Unity 씬/프리팹 안전 편집 규칙

권장 운영:

1. 새 프로젝트 초기화 시 `docs/RTK.md`를 기본 생성한다.
2. 실제 `rtk` 바이너리 설치와 전역 훅 연결은 사용자 전역 환경에서 관리한다.
3. 에이전트는 긴 shell output이 예상될 때 `docs/RTK.md`를 기준으로 RTK 사용 경로를 우선 고려한다.
4. RTK 설치 여부를 추정하지 말고, 설치/확인/우회는 항상 `docs/RTK.md` 기준으로 안내한다.

주의:

- 이 킷은 `rtk init`를 프로젝트 내부 문서/훅을 덮어쓰는 방식으로 직접 실행하는 것을 기본값으로 두지 않는다.
- RTK는 `shell output compression` 계층일 뿐, raw source reading이나 구조화 도구 탐색을 대신하지 않는다.

## Default: Sub-Agents

이 킷은 `sub-agent` 또는 내부 병렬 위임을 선택적 최적화 계층으로 다룬다.

- 역할:
  - bounded side task를 병렬로 조사
  - 테스트/로그/문서 정리 같은 비핵심 경로를 분리
  - 메인 에이전트가 critical path를 계속 진행하도록 돕기
- 대체하지 않는 것:
  - 메인 에이전트의 최종 판단
  - cross-agent handoff (`Codex ↔ Claude Code`)
  - validation gate 실행 책임

권장 운영:

1. 새 프로젝트 초기화 시 `docs/SubAgents.md`를 기본 생성한다.
2. 서브 에이전트는 플랫폼과 세션 정책이 허용하고, 사용자가 delegation을 허용했을 때만 사용한다.
3. 메인 에이전트는 urgent blocking work를 직접 처리하고, sub-agent는 read-only 조사나 disjoint write scope 위주로만 쓴다.
4. sub-agent 결과는 메인 에이전트가 통합하고, 최종 검증도 메인 에이전트가 책임진다.

주의:

- 같은 파일/같은 serialized asset 영역을 여러 에이전트가 동시에 수정하게 두지 않는다.
- `docs/AgentHandoffs/**`는 Codex와 Claude Code 사이 handoff용이고, 내부 sub-agent 결과 저장소로 남용하지 않는다.

## Graphify

이 킷은 `graphify`를 하네스의 대체재가 아니라 구조 이해 계층으로 다룬다. 모든 프로젝트에 기본 설치한다.

- 추천 용도:
  - 큰 Unity 프로젝트의 코드/문서 관계 파악
  - ECS 흐름, 시스템 연결, 설계 메모의 빠른 오리엔테이션
  - 원본 파일을 반복해서 읽지 않고 구조 질문에 답하기
- 비추천 용도:
  - `.unity`, `.prefab`, `ScriptableObject`, `ProjectSettings` 안전성 보장
  - 하네스 문서/훅/검증 게이트 대체

권장 운영:

1. `graphify`는 프로젝트 루트에서 그래프를 빌드한다.
2. 결과는 `graphify-out/`에 유지한다.
3. 에이전트는 `graphify-out/GRAPH_REPORT.md`가 있으면 넓은 구조 질문 전에 먼저 읽는다.
4. 그래프 갱신 규칙은 `docs/Graphify.md`에 적는다.
5. 이 프로젝트는 자동 훅이 아니라 `tools/graphify-refresh.*` 기반의 명시적 갱신을 기본값으로 둔다. 자세한 절차는 `docs/GRAPHIFY_WORKFLOW.md`.

주의:

- 하네스가 이미 설치된 프로젝트에서는 `graphify codex install`, `graphify claude install` 같은 always-on 설치를 기본값으로 쓰지 않는다.
- 이 명령들은 `AGENTS.md`, `CLAUDE.md`, 훅 레이어를 건드릴 수 있어 현재 하네스와 충돌할 수 있다.
- 대신 `graphify`는 명시적 빌드/쿼리 도구로 유지하고, 운영 규칙은 `docs/Graphify.md`에 기록한다.

## 중요한 점

- 이 킷은 프로젝트를 직접 자동 완성하는 마법 상자가 아닙니다.
- 대신 에이전트가 프로젝트를 스캔하고, 안전한 범위부터, 검증 가능한 하네스를 빠르게 설치하도록 표준 절차를 제공합니다.
- 프로젝트마다 씬, 프리팹, 데이터 자산, 테스트 가능한 시스템이 다르므로 최종 설치는 항상 프로젝트 맞춤형이어야 합니다.
- 실수가 확인되면, 그 실수를 범주화하고 해당 `docs/HarnessMistakes/categories/*.md` 파일을 갱신하는 학습 루프까지 포함합니다.
- 작업 시작 시에는 `docs/HarnessMistakes/domains/*.md` 중 해당 도메인 파일을 먼저 읽고, 그 파일이 추가 실수 컨텍스트 로딩 여부를 결정합니다.
- Codex와 Claude Code를 같이 쓰는 프로젝트라면 `docs/AgentHandoffs/pending/**`와 `consumed/**`를 통해 서로의 작업 내역을 짧게 넘기고, 읽은 note는 이동시켜 재독을 막습니다.
- sub-agent를 쓰는 경우에도 메인 에이전트가 ownership과 최종 검증 책임을 유지합니다.
- 공유 프로젝트 기본값에서는 `.claude/settings.json`, `.claude/hooks/**`, `.mcp.json`, `.claude/unity-cli.md`도 함께 설치합니다.
- Codex companion도 함께 준비합니다: 프로젝트 로컬 `.codex/hooks.json`, `.codex/hooks/guard-assets-check.ps1`, 전역 병합용 `.codex/config.template.toml`.
- Codex 실제 활성 설정은 여전히 `~/.codex/config.toml`이므로, `.codex/config.template.toml`은 프로젝트에 복사만 하고 자동 적용하지 않습니다.
- 필요하면 `scripts/merge-codex-config.ps1`로 `~/.codex/config.toml`에 필요한 블록만 병합합니다.
- Claude Desktop을 같이 쓰면 전역 설정 파일의 `mcpServers`도 companion 대상으로 취급하고, 기존 설정을 보존한 채 merge합니다.
- Unity Editor 조작 계층은 `unity-cli`만 표준으로 사용합니다. `Unity-MCP` 계열 에디터 조작 구현체는 이 킷의 공식 대상에 포함하지 않습니다.
- RTK는 기본 문서화 대상이지만 필수 런타임 의존성은 아닙니다. 미설치여도 하네스는 동작하고, 설치되어 있으면 shell-heavy 작업에서 토큰 절감 계층으로만 작동합니다.

## 설치 후에도 남는 역할

에이전트 스타터 킷을 한 번 적용한 뒤에는 이 폴더를 남겨둬도 되고 지워도 됩니다.

- 남겨두는 경우
  - 이후 다른 Codex 또는 Claude Code 스레드가 다시 읽고 갱신 작업을 하기 쉽습니다.
- 지우는 경우
  - 이미 설치된 `AGENTS.md`, `CLAUDE.md`, 검증 도구만으로 운영할 수 있습니다.

# For Codex

## 진입 절차

1. `unity-agent-kit/AGENT_KIT_INSTALL_SPEC.md`를 읽는다 — 원칙, 설치 절차, 체크리스트 확인
2. `unity-agent-kit/steps/01-scan.md`부터 순서대로 읽고, 한 단계씩 완료한다
3. 각 단계 파일이 필요한 템플릿을 지정한다. 단계 시작 전에 모든 `templates/**`를 미리 읽지 않는다

업데이트 요청이라면: `unity-agent-kit/steps/09-update.md`를 읽는다.

## Codex 전용 운영 규칙

- **읽기-분석-쓰기 순서**: Read/Grep/Glob을 한 번에 병렬로 실행 → 분석 → Edit/Write/Bash.
  읽기와 쓰기를 여러 턴에 걸쳐 섞지 않는다.
- **dual-agent 기본**: Codex가 시작해도 Claude Code가 바로 작업 가능한 구조로 설치.
- **`AGENTS.md`는 Codex 호환 자기완결형**: `AGENTS.claude.template.md`는 사용자가 명시할 때만 사용.
- `.codex/config.template.toml`은 project-local 활성 설정이 아닌 전역 `~/.codex/config.toml` 병합용 템플릿.
- 플레이스홀더(`{{UNITY_PREFAB_PARSER_PATH}}` 등)는 실제 환경 값으로 채운다. 추론 불가하면 한 번만 확인.
- 기존 프로젝트가 `docs/CodexPromptTemplates.md`나 `Assets/Editor/CodexValidation/*`을 쓰면 이름 강제 변경 금지.
- 짧은 요청도 내부적으로 목표/수정 범위/완료 조건/검증으로 확장한다.

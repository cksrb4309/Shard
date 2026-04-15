# Agent Kit Install Spec

Unity 프로젝트에 에이전트 운영 구조를 설치하는 공통 규격.

## 원칙

- `Assets/` 아래에 문서 폴더를 두지 않는다 (meta 파일 오염 방지)
- code-only scope 우선. guarded asset(`.unity`, `.prefab`, `ProjectSettings/**`)은 꼭 필요할 때만 수정
- 기존 파일이 있으면 덮어쓰지 않고, 어긋난 부분만 갱신
- 기본 모드: dual-agent (Codex + Claude Code 공존)

## 설치 절차

단계별 파일을 순서대로 읽고, 각 단계를 완료한 뒤 다음으로 넘어간다.

1. `unity-agent-kit/steps/01-scan.md` — 프로젝트 스캔
2. `unity-agent-kit/steps/02-core-docs.md` — 핵심 에이전트 문서
3. `unity-agent-kit/steps/03-harness-docs.md` — 하네스 운영 문서
4. `unity-agent-kit/steps/04-mistakes.md` — 실수 학습 구조
5. `unity-agent-kit/steps/05-handoffs-wiki.md` — Handoff & Wiki
6. `unity-agent-kit/steps/06-tools.md` — 검증 도구
7. `unity-agent-kit/steps/07-companion.md` — 에이전트별 설정 파일
8. `unity-agent-kit/steps/08-validate.md` — 검증 및 보고

업데이트 요청이라면: `unity-agent-kit/steps/09-update.md`

## 최소 산출물 체크리스트

설치 완료 시 아래 항목이 모두 존재해야 한다.

- [ ] `harness-version`
- [ ] `AGENTS.md`
- [ ] `CLAUDE.md` (100줄 이하)
- [ ] `docs/project-context.md`
- [ ] `docs/AgentPromptTemplates.md`
- [ ] `docs/Obsidian.md`
- [ ] `docs/RTK.md`
- [ ] `docs/SubAgents.md`
- [ ] `docs/Graphify.md`
- [ ] `docs/HarnessMistakes/**` (domains + categories)
- [ ] `docs/AgentHandoffs/**` (pending + consumed)
- [ ] `docs/ProjectWiki/**`
- [ ] `tools/unity-batch.ps1` + cmd 파일들
- [ ] `Assets/Editor/HarnessValidation/BatchValidationRunner.cs`
- [ ] `Assets/99_Tests/` (smoke test 1개 이상)
- [ ] `.claude/settings.json`
- [ ] `.claude/hooks/guard-assets-check.ps1`
- [ ] `.mcp.json`
- [ ] `.codex/README.md`, `.codex/hooks.json`, `.codex/hooks/guard-assets-check.ps1`, `.codex/config.template.toml`
- [ ] `.claude/unity-cli.md`

## 피해야 할 것

- 기존 사용자 변경을 되돌리는 것
- 프로젝트 구조 확인 없이 템플릿을 그대로 덮어쓰는 것
- guarded asset을 이유 없이 수정하는 것
- `docs/ProjectWiki/log.md`를 handoff inbox처럼 남용하는 것
- handoff note를 읽은 뒤에도 `pending/**`에 그대로 두는 것

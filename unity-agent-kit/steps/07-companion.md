# Step 07 — 에이전트별 설정 파일

## 이 단계의 목표

`.claude/**`, `.codex/**`, `.mcp.json`을 설치한다.
Claude Code와 Codex 둘 다 바로 작업 가능한 상태를 만든다.

---

## .claude/settings.json

기준 템플릿: `unity-agent-kit/templates/.claude/settings.template.json`

- `permissions.allow`: `tools/*` 검증 스크립트 실행 허용
- `permissions.deny`: `.unity`, `.prefab`, `ProjectSettings/**` 차단
- `{{GUARDED_DATA_PATH}}`를 Step 01에서 확정한 ScriptableObject/Config 경로로 교체

## .claude/hooks/guard-assets-check.ps1

기준 템플릿: `unity-agent-kit/templates/.claude/hooks/guard-assets-check.ps1`

- PreToolUse 훅. guarded asset 편집 시도 시 차단 + 이유 전달
- settings.json deny와 이중 방어선 구성
- `$guardedPatterns`에 프로젝트별 추가 패턴 삽입

## .mcp.json

기준 템플릿: `unity-agent-kit/templates/.mcp.template.json`

- `{{UNITY_PREFAB_PARSER_PATH}}`를 실제 경로로 교체
- 추론 불가하면 플레이스홀더로 두고 사용자에게 안내

## .claude/unity-cli.md

기준 템플릿: `unity-agent-kit/templates/.claude/unity-cli.md`

- unity-cli 도구 사용 기준 및 파일 직접 수정 경계
- MCP 연결 확인 방법과 미연결 시 안내 절차
- unity-prefab-parser-mcp 연결 설정 (Claude Code, Codex 모두 포함)

## .claude/image-analysis.md (ollama-vision 환경이면)

기준 템플릿: `unity-agent-kit/templates/.claude/image-analysis.md`

파이프라인이 없는 환경이면 생략 가능.

## .claude/claude-desktop-config.md (Claude Desktop 사용 시)

기준 템플릿: `unity-agent-kit/templates/.claude/claude-desktop-config.md`

설정 파일 경로: `C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`

- 전체 덮어쓰기 금지. `mcpServers`만 병합
- 기존 `preferences`와 unrelated MCP 보존
- 가능하면 `scripts/merge-claude-desktop-config.ps1` 사용
- 최소 병합 대상: `unity-cli`, `unity-parser` (사용 시), `image-tools`/`ollama-vision` (사용 시)

---

## .codex/

기준 템플릿: `unity-agent-kit/templates/.codex/**`

| 파일 | 역할 |
|------|------|
| `.codex/README.md` | companion ownership split 설명 |
| `.codex/hooks.json` | project-local guarded asset 차단 훅 |
| `.codex/hooks/guard-assets-check.ps1` | guarded path 차단 스크립트 |
| `.codex/config.template.toml` | 전역 `~/.codex/config.toml` 병합용 템플릿 |

- `.codex/config.template.toml`은 활성 설정이 아님
- 전역 병합은 `scripts/merge-codex-config.ps1` 또는 수동으로만 수행
- 병합 최소 대상: `multi_agent = true`, `unity-cli`, `unity-prefab-parser`, project trust

→ 완료되면 `unity-agent-kit/steps/08-validate.md`를 읽는다.

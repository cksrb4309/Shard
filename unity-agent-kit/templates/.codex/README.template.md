# Codex Companion

이 폴더는 Codex용 프로젝트 로컬 companion 설정을 담는다.

## Ownership

- 프로젝트 로컬에서 직접 적용되는 파일:
  - `.codex/hooks.json`
  - `.codex/hooks/guard-assets-check.ps1`
- 전역 설정 예시 파일:
  - `.codex/config.template.toml`

`config.template.toml`은 **프로젝트 로컬에서 자동 적용되지 않는다.**
필요한 블록만 `~/.codex/config.toml`에 병합한다.

## Recommended Workflow

1. 프로젝트 루트의 `.codex/hooks.json`을 설치한다.
2. `.codex/hooks/guard-assets-check.ps1`를 같이 설치한다.
3. `.codex/config.template.toml`에서 필요한 블록만 골라 `~/.codex/config.toml`에 병합한다.
4. 필요하면 `scripts/merge-codex-config.ps1`로 전역 설정 병합을 보조한다.

## Scope

- `hooks.json`은 프로젝트별 규칙을 담는다.
- `config.template.toml`은 Unity CLI, unity parser, graphify multi-agent 같은 Codex 전역 준비 상태를 맞추기 위한 예시다.

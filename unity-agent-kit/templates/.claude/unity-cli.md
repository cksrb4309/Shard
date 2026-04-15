# Unity CLI 사용 가이드

이 문서는 Claude Code와 Codex가 Unity 프로젝트 작업 시 `unity-cli`를 올바르게 활용하기 위한 운영 규칙이다.

## 왜 unity-cli를 쓰는가

`unity-cli`는 Unity 에디터를 외부에서 제어하기 위한 MCP 래퍼다.
현재 환경에서는 `C:\Users\rlack\.local\bin\unity-cli-mcp.js`를 통해 연결된다.

이 방식은 다음 작업에 적합하다.

- Unity Editor 연결 상태 확인
- Play / Pause / Stop 제어
- 에디터 안에서 C# 코드 실행
- 콘솔 로그 읽기 / 비우기
- 테스트 실행
- Game/Scene 뷰 스크린샷 캡처

직렬화된 `.unity`, `.prefab` YAML을 직접 만지는 대신, 필요한 경우 `unity-exec`으로 에디터 안에서 C#을 실행해 조작하는 쪽이 안전하다.

## 주요 도구

| 도구 | 용도 | 비고 |
|------|------|------|
| `unity-status` | Unity Editor 연결 상태 확인 | 세션 시작 시 우선 호출 |
| `unity-editor-play` | 플레이 모드 진입 | 필요 시 `wait` |
| `unity-editor-pause` | 플레이 일시정지 | |
| `unity-editor-stop` | 플레이 종료 | wrapper 구현 상태도 함께 확인 |
| `unity-exec` | 에디터 안에서 C# 코드 실행 | 안전한 에디터 조작의 핵심 |
| `unity-console-read` | 콘솔 로그 읽기 | error/warning/log 필터 가능 |
| `unity-console-clear` | 콘솔 비우기 | |
| `unity-test-run` | Unity 테스트 실행 | EditMode / PlayMode |
| `unity-screenshot` | Scene/Game 뷰 캡처 | UI 확인 보조 |

## 어떤 작업에 쓰는가

`unity-cli`를 우선 쓰는 작업:

- Unity Editor 상태 확인
- 검증용 테스트 실행
- 콘솔 에러 수집
- 플레이 모드 스모크 확인
- 스크린샷 수집
- 에디터 API를 통해 안전하게 수행 가능한 조작

예:

- 특정 메뉴 실행 대신 C#으로 같은 효과를 내는 검증 보조
- 테스트 전 콘솔 비우기, 테스트 후 콘솔 읽기
- 한정된 에디터 조작을 `unity-exec`으로 수행

## 파일 직접 수정이 맞는 작업

- C# 스크립트 (`Assets/**/*.cs`)
- 어셈블리 정의 (`*.asmdef`, `*.asmref`)
- 하네스 문서 및 설정 (`AGENTS.md`, `CLAUDE.md`, `docs/**`, `tools/**`)
- `.claude/settings.json`, `.claude/hooks/**`

씬/프리팹을 직접 YAML로 고치는 것은 마지막 수단이다.
가능하면 `unity-cli` 또는 별도 검증 루프로 해결한다.

## 세션 시작 시 연결 확인

씬, 프리팹, 테스트, 플레이 확인이 포함된 세션은 시작 시 `unity-status`로 연결 상태를 확인한다.

1. `unity-status`가 정상 응답하면 `unity-cli` 사용 모드로 진행한다.
2. 응답이 없거나 실패하면 아래 "미연결 시" 절차를 따른다.

## 연결 상태에서의 운영 규칙

- Unity 조작은 `unity-cli`를 먼저 시도한다.
- `.claude/settings.json`의 `.unity`, `.prefab` deny 항목은 유지한다.
- 직렬화 자산 수정이 필요하면 raw YAML 편집보다 `unity-exec` 기반 접근을 먼저 검토한다.
- `unity-cli`가 지원하지 않는 작업이면 사용자에게 한계를 보고하고 대안을 제시한다.

## 미연결 시

**unity-cli가 연결되어 있지 않습니다.**

확인 순서:

1. Unity 에디터가 열려 있는지 확인한다.
2. `unity-cli` 환경이 설치되어 있는지 확인한다.
3. 전역 설정에 `unity-cli` 서버가 등록되어 있는지 확인한다.
   - Codex: `C:\Users\rlack\.codex\config.toml`
   - Claude Code CLI: `C:\Users\rlack\.claude.json`
   - Claude Desktop: `C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`
4. `unity-cli-mcp.js`와 wrapper 경로가 실제로 존재하는지 확인한다.

사용자 승인 없이 raw YAML 편집으로 독단 진행하지 않는다.

## Read 최적화 보조 도구 — unity-prefab-parser-mcp

`unity-cli`가 Unity 조작을 담당한다면, `unity-prefab-parser-mcp`는 씬/프리팹 읽기를 최적화하는 선택 도구다.

- `.unity`, `.prefab` 구조를 읽을 때 토큰 낭비를 줄인다.
- 조작이 아니라 읽기 최적화용이다.

### Claude Code project-local 설정 (.mcp.json)

프로젝트 루트의 `.mcp.json`에 아래를 추가한다.
`{{UNITY_PREFAB_PARSER_PATH}}`는 실제 설치 경로로 교체한다.

```json
{
  "mcpServers": {
    "unity-parser": {
      "command": "node",
      "args": ["{{UNITY_PREFAB_PARSER_PATH}}/dist/index.js"]
    }
  }
}
```

## 전역 설정 예시

### Codex (`~/.codex/config.toml`)

```toml
[mcp_servers.unity_cli]
command = "node"
args = ["C:\\Users\\rlack\\.local\\bin\\unity-cli-mcp.js"]

[mcp_servers.unity_prefab_parser]
command = "node"
args = ["{{UNITY_PREFAB_PARSER_PATH}}/dist/index.js"]
```

### Claude Code CLI (`~/.claude.json`)

```json
"unity-cli": {
  "command": "node",
  "args": ["C:\\Users\\rlack\\.local\\bin\\unity-cli-mcp.js"]
}
```

### Claude Desktop (`claude_desktop_config.json`)

아래 전역 설정 파일의 `mcpServers`에 병합한다.

`C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`

```json
"unity-cli": {
  "command": "node",
  "args": ["C:\\Users\\rlack\\.local\\bin\\unity-cli-mcp.js"]
}
```

규칙:

- 기존 `preferences`와 unrelated MCP 서버는 유지한다.
- 전체 파일을 덮어쓰지 말고 `mcpServers.unity-cli`만 병합한다.
- 필요하면 `scripts/merge-claude-desktop-config.ps1 -IncludeUnityCli`를 사용한다.

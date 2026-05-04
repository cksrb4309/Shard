# Unity CLI 사용 가이드

이 문서는 Claude Code가 Unity 프로젝트 작업 시 `unity-cli`를 올바르게 활용하기 위한 운영 규칙이다.

## 왜 unity-cli를 쓰는가

`unity-cli`는 Unity 에디터를 외부에서 제어하기 위한 MCP 래퍼다.
현재 환경에서는 `C:\Users\rlack\.local\bin\unity-cli-mcp.js`를 통해 연결된다.

이 방식은 다음 작업에 적합하다:
- Unity Editor 연결 상태 확인
- Play / Pause / Stop 제어
- 에디터 안에서 C# 코드 실행
- 콘솔 로그 읽기 / 비우기
- 테스트 실행
- Game/Scene 뷰 스크린샷 캡처

## 주요 도구

| 도구 | 용도 |
|------|------|
| `unity-status` | 연결 상태 확인 (세션 시작 시 우선 호출) |
| `unity-editor-play` | 플레이 모드 진입 |
| `unity-editor-pause` | 플레이 일시정지 |
| `unity-editor-stop` | 플레이 종료 |
| `unity-exec` | 에디터 안에서 C# 코드 실행 |
| `unity-console-read` | 콘솔 로그 읽기 |
| `unity-console-clear` | 콘솔 비우기 |
| `unity-test-run` | EditMode / PlayMode 테스트 실행 |
| `unity-screenshot` | Scene/Game 뷰 캡처 |

## 파일 직접 수정이 맞는 작업

- C# 스크립트 (`Assets/01_Scripts/**/*.cs`)
- 어셈블리 정의 (`*.asmdef`, `*.asmref`)
- 하네스 문서 및 설정 (`AGENTS.md`, `CLAUDE.md`, `docs/**`, `tools/**`)
- `.claude/settings.json`, `.claude/hooks/**`

씬/프리팹을 직접 YAML로 고치는 것은 마지막 수단이다.

## 세션 시작 시 연결 확인

씬, 프리팹, 테스트, 플레이 확인이 포함된 세션은 시작 시 `unity-status`로 연결 상태를 확인한다.

1. 정상 응답 → `unity-cli` 사용 모드로 진행
2. 응답 없음 → 아래 "미연결 시" 절차를 따른다

## 미연결 시

**unity-cli가 연결되어 있지 않습니다.**

확인 순서:
1. Unity 에디터가 열려 있는지 확인한다.
2. `unity-cli` 환경이 설치되어 있는지 확인한다.
3. 전역 설정에 `unity-cli` 서버가 등록되어 있는지 확인한다.
   - Claude Code CLI: `C:\Users\rlack\.claude.json`
4. `unity-cli-mcp.js` 경로가 실제로 존재하는지 확인한다.

사용자 승인 없이 raw YAML 편집으로 독단 진행하지 않는다.

## unity-prefab-parser-mcp

`.unity`, `.prefab` 구조를 읽을 때 토큰 낭비를 줄이는 선택 도구.
조작이 아니라 읽기 최적화용이다.

프로젝트 루트 `.mcp.json`에서 설정한다. 경로는 실제 설치 위치로 교체 필요.

## Claude Code CLI 전역 설정 (`~/.claude.json`)

```json
"unity-cli": {
  "command": "node",
  "args": ["C:\\Users\\rlack\\.local\\bin\\unity-cli-mcp.js"]
}
```

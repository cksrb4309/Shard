# Claude Desktop Config Guide

이 문서는 Claude Desktop을 함께 쓰는 환경에서 전역 설정 파일을 어떻게 병합해야 하는지 설명한다.

## 대상 파일

Windows 기본 경로:

`C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`

이 파일은 **프로젝트 파일이 아니라 전역 파일**이다.
프로젝트마다 새로 만들지 않고, 필요한 MCP 서버만 안전하게 병합한다.

## Merge Rules

- 전체 파일을 덮어쓰지 않는다.
- `mcpServers`만 병합한다.
- 기존 `preferences`는 그대로 둔다.
- 기존 `unity-cli`, `executor` 같은 unrelated MCP 서버도 그대로 둔다.
- 이미 같은 이름의 서버가 있으면 경로가 같은지 먼저 확인한다.
- 경로가 다르면 독단적으로 지우지 말고 차이를 보고한다.

## Recommended Entries

공유 Unity 하네스 기준으로 우선 고려할 서버:

- `unity-cli`
  - Unity Editor 제어, C# 실행, 콘솔 읽기, 테스트 실행, 스크린샷
- `unity-parser`
  - 씬/프리팹 읽기 최적화
- `ollama-vision`
  - 이미지 설명, UI 구조 파악
- `image-tools`
  - 픽셀 색상, OCR, 거리 측정

## Safe Update Method

가능하면 루트의 `unity-agent-kit/scripts/merge-claude-desktop-config.ps1`를 사용한다.

이 스크립트는:

- 대상 파일이 없으면 새로 만든다
- `mcpServers`만 추가/갱신한다
- 기존 `preferences`와 다른 키는 유지한다

예:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity-agent-kit\scripts\merge-claude-desktop-config.ps1 `
  -UnityCliPath "C:\Users\rlack\.local\bin\unity-cli-mcp.js" `
  -UnityPrefabParserPath "C:\Users\rlack\.local\share\unity-prefab-parser-mcp" `
  -IncludeUnityCli `
  -IncludeUnityParser `
  -IncludeOllamaVision `
  -IncludeImageTools
```

## Manual Review Checklist

- `mcpServers.unity-cli`가 있는가
- `mcpServers.unity-parser`가 있는가
- 이미지 파이프라인을 쓴다면 `ollama-vision`, `image-tools`가 있는가
- 기존 `preferences`가 그대로 남아 있는가
- 기존 unrelated MCP 서버가 사라지지 않았는가

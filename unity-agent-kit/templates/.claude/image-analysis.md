# 로컬 이미지 분석 파이프라인 가이드

이 문서는 Claude Code가 Unity 프로젝트 작업 시 로컬 이미지 분석 MCP를 올바르게 활용하기 위한 운영 규칙이다.

## 파이프라인 구성

이 파이프라인은 Claude 계열 전역 설정에 등록된 두 MCP 서버로 구성된다.
프로젝트별 `.mcp.json`에 별도 추가할 필요는 없다.

| 서버명 | 종류 | 주요 도구 | 용도 |
|--------|------|-----------|------|
| `ollama-vision` | stdio / Python | `describe_image` | 장면 설명, UI 구조 파악, 화면 텍스트 추출 |
| `image-tools` | stdio / exe | `image_sample_color`, `image_dominant_colors`, `image_measure_distance`, `image_ocr_region` | 정밀 수치 측정 |

백엔드: Ollama (`llava-phi3:latest`) — 로컬 실행, 인터넷 불필요.

## 사용 우선순위

1. **ollama-vision 먼저** — 이미지 전체를 설명하거나 UI 구조를 파악할 때
2. **image-tools는 정밀 수치 필요 시만** — 특정 픽셀 색상, 지배색, 거리, OCR 영역 추출

추정값(ollama-vision 분석 결과)과 측정값(image-tools 수치 결과)을 구분해서 출력한다.

## Unity 작업에서의 활용 시나리오

| 작업 유형 | 권장 도구 | 예시 |
|-----------|-----------|------|
| UI 스크린샷으로 레이아웃 확인 | ollama-vision | "HP 바가 좌측 상단에 있는지 확인" |
| 스프라이트/텍스처 색상 검증 | image-tools → `image_dominant_colors` | "캐릭터 스프라이트 주요 색상이 스펙과 맞는지 확인" |
| 화면 UI 텍스트 읽기 | ollama-vision 또는 `image_ocr_region` | "게임 오버 화면의 점수 텍스트 추출" |
| 비주얼 버그 디버깅 | ollama-vision → 상세 측정은 image-tools | "스크린샷에서 이상한 아티팩트가 있는지 확인" |
| UI 목업 디자인 참조 파악 | ollama-vision | "디자인 시안 이미지에서 버튼 배치 파악" |
| 특정 픽셀 좌표 색상 확인 | `image_sample_color(x, y)` | "버튼 배경색이 정확히 #FF0000인지 확인" |

## 세션 시작 시 가용성 확인

이미지 분석 작업이 포함된 세션은 두 MCP의 가용성을 확인한다.

**Ollama 동작 확인이 선행되어야 한다.**
ollama-vision은 Ollama 서비스가 실행 중일 때만 동작한다.

1. `describe_image`를 테스트 이미지로 호출해서 응답 여부를 확인한다.
2. 응답이 오면 → 파이프라인 사용 가능.
3. 오류가 나면 → 아래 "파이프라인 미연결 시" 절차를 따른다.

## 파이프라인 미연결 시

---

**로컬 이미지 분석 파이프라인이 응답하지 않습니다.**

Ollama 서비스가 실행 중이지 않을 수 있습니다.

해결 방법:

1. **Ollama 앱을 실행한다.**
   - `C:\Users\rlack\AppData\Local\Programs\Ollama\ollama.exe` 실행
   - 또는 시작 메뉴에서 Ollama를 검색해서 실행
2. **Ollama가 실행 중인지 확인한다.**
   - 터미널에서: `ollama list` → 모델 목록이 나오면 정상
   - `llava-phi3:latest` 모델이 목록에 있어야 한다
3. **MCP 서버 자체 문제라면:**
   - `ollama-vision` 서버 경로: `C:\Users\rlack\.local\share\ollama-vision-mcp\.venv\Scripts\python.exe -m src.server`
   - `image-tools` 서버 경로: `C:\Users\rlack\.local\bin\image-tools-mcp.exe`
   - 전역 등록 위치:
     - Claude Code CLI: `C:\Users\rlack\.claude.json`
     - Claude Desktop: `C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`

---

Ollama를 바로 시작하기 어려우면 이미지 분석 없이 진행할 수 있는 대안을 사용자에게 제시한다.
파이프라인 없이 독단적으로 이미지 파일을 직접 Read하는 방식으로 대체하지 않는다 (토큰 낭비 위험).

## 전역 등록 설정

두 MCP는 에이전트별 전역 설정 파일에 등록한다. 프로젝트별 설정 파일에는 추가하지 않는다.

### Claude Code CLI (`~/.claude.json`)

`mcpServers` 키 아래에 추가:

```json
"ollama-vision": {
  "command": "C:\\Users\\rlack\\.local\\share\\ollama-vision-mcp\\.venv\\Scripts\\python.exe",
  "args": ["-m", "src.server"]
},
"image-tools": {
  "command": "C:\\Users\\rlack\\.local\\bin\\image-tools-mcp.exe",
  "args": []
},
"unity-parser": {
  "command": "node",
  "args": ["C:\\Users\\rlack\\.local\\share\\unity-prefab-parser-mcp\\dist\\index.js"]
}
```

### Claude Desktop (`claude_desktop_config.json`)

아래 전역 설정 파일의 `mcpServers`에 같은 항목을 병합한다.

`C:\Users\rlack\AppData\Local\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json`

```json
"ollama-vision": {
  "command": "C:\\Users\\rlack\\.local\\share\\ollama-vision-mcp\\.venv\\Scripts\\python.exe",
  "args": ["-m", "src.server"]
},
"image-tools": {
  "command": "C:\\Users\\rlack\\.local\\bin\\image-tools-mcp.exe",
  "args": []
},
"unity-parser": {
  "command": "node",
  "args": ["C:\\Users\\rlack\\.local\\share\\unity-prefab-parser-mcp\\dist\\index.js"]
}
```

기존 `preferences`와 unrelated MCP 서버는 보존한다.
전체 파일을 덮어쓰지 말고 `mcpServers`만 병합한다.

### Codex (`~/.codex/config.toml`)

파일 끝에 추가:

```toml
[mcp_servers.ollama_vision]
command = "C:\\Users\\rlack\\.local\\share\\ollama-vision-mcp\\.venv\\Scripts\\python.exe"
args = ["-m", "src.server"]

[mcp_servers.image_tools]
command = "C:\\Users\\rlack\\.local\\bin\\image-tools-mcp.exe"
args = []
```

## 출력 형식 규칙

- ollama-vision 결과는 **"(추정)"** 태그를 붙인다.
- image-tools 측정값은 **"(측정)"** 태그를 붙인다.
- 두 결과가 충돌하면 측정값을 우선한다.

예시:
```
배경색: #4080B0 (측정, image_dominant_colors 74.6%)
UI 레이아웃: HP 바가 좌측 상단에 표시됨 (추정, ollama-vision)
```

# Step 01 — 프로젝트 스캔

## 이 단계의 목표

프로젝트 구조를 파악하고, 기존 하네스 설치 여부와 현재 상태를 확인한다.

## 스캔 대상

- `ProjectSettings/ProjectVersion.txt` — Unity 버전
- `Assets/` 하위 주요 폴더 구조
- `Assets/**/*.unity` — 씬 목록
- `Assets/**/*.prefab` — 주요 프리팹
- `Assets/**/Config*.asset`, `Assets/**/ScriptableObject/**` — Data asset
- `Assets/01_Scripts/**` 또는 동등한 코드 폴더
- `Assets/99_Tests/**` — 기존 테스트
- `Assets/Editor/**` — 기존 에디터 툴링
- `harness-version` — 기존 설치 버전
- `AGENTS.md`, `CLAUDE.md`, `docs/**`, `README.md` — 기존 문서

## 판단 규칙

- 씬이 여러 개면 가장 테스트 성격이 강한 씬 1개를 주 검증 씬으로 고른다.
  후보가 모호하면 사용자에게 한 번만 짧게 확인한다.
- `harness-version`이 있으면 **업데이트 모드**: `steps/09-update.md`를 읽는다.
- 기존 `AGENTS.md`, `CLAUDE.md`가 있으면 덮어쓰기 대신 갱신 대상으로 메모한다.
- ScriptableObject/Config 경로를 확인해서 guarded data path를 확정한다.
  안전하게 추론할 수 없으면 한 번만 짧게 확인한다.
- 기존 프로젝트가 `docs/CodexPromptTemplates.md`나 `Assets/Editor/CodexValidation/*`을 쓰면
  이름 강제 변경 금지. 기존 경로와 병행 갱신.

## 스캔 완료 후 확정 정보

- Unity 버전:
- 주요 코드 폴더 경로:
- 주 검증 씬/프리팹:
- 기존 하네스 파일 목록 (있으면):
- Guarded data path (ScriptableObject/Config):

→ 완료되면 `unity-agent-kit/steps/02-core-docs.md`를 읽는다.

# RTK

`rtk`는 이 프로젝트의 shell output 압축 계층이다.

긴 커맨드 출력에 의존하는 작업에서 LLM 토큰 소비를 줄이기 위해 사용한다.
아래 항목의 대체재로 취급하지 않는다:

- `AGENTS.md`
- `CLAUDE.md`
- 하네스 검증 게이트
- `graphify`
- `unity-cli`
- `unity-prefab-parser-mcp`

## 역할

RTK를 사용하면 효과가 큰 경우:

- `git status`, `git diff`, `git log`
- `rg` / `grep`
- 테스트 러너 출력
- Unity batch 검증 로그
- 빌드 및 lint 출력

RTK 효과가 낮은 경우:

- raw source file 읽기
- Claude Code `Read`, `Grep`, `Glob`
- Unity scene/prefab parser 워크플로

## 설치 정책

RTK 자체는 **필수 프로젝트 의존성이 아니다.**
킷은 RTK가 이미 설치되어 있다고 가정하지 않는다.

## Claude Code 전역 설치

```powershell
rtk init -g
rtk init --show
```

## 설치 확인

```powershell
rtk --version
rtk gain
rtk init --show
```

## Unity 프로젝트에서 사용

```powershell
rtk git status
rtk git diff
rtk git log -n 10
rtk grep "SpawnManager" .
rtk log Logs\validation\compile.log
```

## 에이전트 동작 규칙

1. RTK가 설치되어 있거나 사용 가능한지 확인한다.
2. 긴 shell 출력에는 RTK를 우선 사용한다.
3. 비자명한 수정 전에는 raw source file을 직접 읽는다.
4. RTK가 `Read/Grep/Glob`이나 Unity parser 도구를 보조한다고 가정하지 않는다.

## 문제 해결

RTK가 설치되어 있지 않은 경우:
- RTK 없이 작업을 계속 진행한다.
- raw shell 출력이 평소보다 클 수 있음을 안내한다.

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

RTK가 가장 유용한 상황은 에이전트가 shell 커맨드를 많이 사용할 때다.

RTK 효과가 낮은 경우:

- raw source file 읽기
- 구조화된 검색 도구
- Claude Code `Read`, `Grep`, `Glob`
- Unity scene/prefab parser 워크플로

## 설치 정책

`docs/RTK.md`는 기본 프로젝트 문서이지만, RTK 자체는 **필수 프로젝트 의존성이 아니다.**

킷은 RTK가 이미 설치되어 있다고 가정하지 않는다.
킷은 RTK에게 프로젝트 로컬 `AGENTS.md`, `CLAUDE.md`, `.claude/settings.json`의 소유권을 넘기지 않는다.

RTK는 Unity 프로젝트 안이 아니라 사용자 전역 환경에서 설치하고 갱신한다.

## Codex 전역 설치

공식 빠른 설치:

```powershell
rtk init -g --codex
rtk init --show
```

이 명령이 하는 일:

- Codex용 RTK 전역 인스트럭션 설치
- `~/.codex/RTK.md` 같은 전역 파일 생성 또는 갱신
- `~/.codex/AGENTS.md`에 RTK 전역 인스트럭션 참조 추가될 수 있음
- Unity 프로젝트 하네스는 RTK와 분리된 상태로 유지

전역 변경 사항은 일상 작업에 반영하기 전에 내용을 확인한다.

## Claude Code 전역 설치

공식 빠른 설치:

```powershell
rtk init -g
rtk init --show
```

이 명령이 하는 일:

- Claude Code용 RTK 전역 훅/인스트럭션 동작 설치
- Unity 프로젝트 하네스는 RTK와 분리된 상태로 유지

기존 전역 훅 설정이 있으면, RTK가 전체 설정을 소유한다고 가정하지 말고 수동으로 병합한다.

## 설치 확인

전역 설치 후 아래 커맨드로 확인:

```powershell
rtk --version
rtk gain
rtk init --show
```

기대 결과:

- RTK를 shell에서 호출할 수 있음
- 토큰 절약 통계를 확인할 수 있음
- 현재 활성화된 AI 도구 연동이 표시됨

## Unity 프로젝트에서 사용

shell 작업이 많은 경우 RTK 경로를 우선한다:

```powershell
rtk git status
rtk git diff
rtk git log -n 10
rtk grep "DamageSystem" .
rtk read Assets/01_Scripts/Combat/DamageSystem.cs
rtk log Logs\Player.log
rtk summary build-output.txt
```

AI 도구가 이미 RTK를 통해 shell 커맨드를 재작성하고 있으면, 일반 커맨드도 이미 압축될 수 있다.
그렇지 않으면 `rtk ...` 커맨드를 명시적으로 호출한다.

`tools\compile-unity.cmd`나 `tools\validate-unity.cmd` 같은 Unity 검증 커맨드는 RTK 훅이 활성 상태일 때 일반 커맨드 경로를 우선한다. 필요한 래퍼를 명확히 알고 있을 때만 `rtk ...`를 명시적으로 사용한다.

## 에이전트 동작 규칙

shell 작업이 많은 경우:

1. RTK가 설치되어 있거나 확실히 사용 가능한지 확인한다.
2. 긴 shell 출력에는 RTK를 우선 사용한다.
3. 비자명한 수정 전에는 raw source file을 직접 읽는다.
4. RTK가 `Read/Grep/Glob`이나 Unity parser 도구를 보조한다고 가정하지 않는다.

구조 파악 작업이 많은 경우:

- `graphify-out/GRAPH_REPORT.md`가 있으면 `graphify`를 먼저 사용한다.

Unity 에셋 작업이 많은 경우:

- `unity-cli` 또는 `unity-prefab-parser-mcp`를 먼저 사용한다.

## 문제 해결

RTK가 설치되어 있지 않은 경우:

- RTK 없이 작업을 계속 진행한다.
- raw shell 출력이 평소보다 클 수 있음을 안내한다.
- 전역 설치는 이 파일을 참고하도록 안내한다.

shell 출력이 여전히 압축되지 않은 경우:

- `rtk init --show`를 실행한다.
- 현재 활성 AI 도구 연동을 확인한다.
- 투명 재작성에 의존하는 대신 `rtk ...` 커맨드를 명시적으로 사용해 본다.

RTK가 다른 전역 훅 설정과 충돌하는 경우:

- Unity 프로젝트 하네스는 그대로 유지한다.
- 전역 훅 동작을 수동으로 병합한다.
- 프로젝트 로컬 guarded-asset 규칙이나 검증 규칙을 약화시키는 방향으로 충돌을 해결하지 않는다.

## 보고 규칙

작업 중 RTK가 관련되었으면 아래를 보고한다:

- RTK 사용 가능 여부
- RTK 경로 사용 여부
- RTK가 도움이 된 shell/로그 작업 단계
- raw shell 출력이 남아있는 부분

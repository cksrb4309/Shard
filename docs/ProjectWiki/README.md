# ProjectWiki

이 디렉터리는 에이전트가 프로젝트를 작업하면서 발견한 지식을 누적하는 위키다.
에이전트가 작성하고 유지한다. 사람은 읽고 질문한다.

`docs/AgentHandoffs/**`와 역할이 다르다.
- `ProjectWiki`는 오래 남길 지식
- `AgentHandoffs`는 다음 에이전트에게 바로 넘길 짧은 작업 인계

## 구조

```
docs/ProjectWiki/
├── README.md          ← 이 파일. 위키 운영 규칙
├── index.md           ← 전체 페이지 카탈로그 (항법용)
├── log.md             ← 작업 이력 (시간순 append-only)
├── systems/           ← 런타임 시스템 페이지
├── assets/            ← 씬/프리팹/ScriptableObject 페이지
└── adr/               ← 아키텍처 결정 기록
```

## 언제 페이지를 만드는가

| 상황 | 만들 페이지 |
|------|------------|
| 시스템을 조사하거나 수정했을 때 | `systems/<SystemName>.md` |
| 씬/프리팹/에셋 의존성을 파악했을 때 | `assets/<AssetName>.md` |
| 아키텍처 결정 이유를 추론/확인했을 때 | `adr/ADR-<NNN>-<slug>.md` |

## 페이지 작성 규칙

- 새 페이지를 만들면 반드시 `index.md`에 한 줄 추가한다.
- 작업이 끝나면 `log.md`에 항목을 append한다.
- 페이지는 짧게 유지한다. 길어지면 분리한다.
- `log.md`를 handoff inbox로 쓰지 않는다.
- 원본 소스(코드, 씬, 프리팹)와 모순되면 원본이 우선이다.

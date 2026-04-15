# {{PROJECT_NAME}} 에이전트 프롬프트 템플릿

## 이 파일의 목적

이 프로젝트에서 프롬프트 단독으로 하네스가 작동하지 않는다.

하네스는 아래 조각들이 합쳐질 때 작동한다:

- 작업 요청
- `AGENTS.md`
- 자동화된 검증
- 작업별 테스트 또는 완료 조건 확인

이 파일은 Codex와 Claude Code 모두 참조할 수 있다.

매번 전체 템플릿을 작성할 필요는 없다.

아래처럼 짧게 요청해도:

```text
이 기능이 가끔 안 돼.
```

에이전트는 이를 아래 항목으로 확장해서 처리한다:

- 작업 목표
- 안전한 수정 범위 추론
- 완료 조건
- 일치하는 검증 경로

더 세밀하게 제어하고 싶을 때만 전체 템플릿을 사용한다.

운영 중 실수가 확정되면 에이전트는:

- 현재 primary domain을 유지한다
- `docs/HarnessMistakes/README.md`를 사용해서 분류한다
- 해당 domain 파일을 갱신한다
- 해당 category 파일을 갱신한다

다른 에이전트의 작업 확인을 요청하면 에이전트는:

- 해당 `docs/AgentHandoffs/pending/**` 폴더만 먼저 확인한다
- consumed handoff 파일은 기본적으로 읽지 않는다
- 이해한 pending 노트는 `docs/AgentHandoffs/consumed/**`로 이동한다

긴 shell 출력이 예상되는 작업이면 에이전트는:

- `docs/RTK.md`를 먼저 확인한다
- 가능하면 RTK 압축 shell 워크플로를 우선 사용한다
- RTK를 raw file 읽기나 구조화된 검색 도구의 대체재로 취급하지 않는다

Shell/Bash를 사용할 때는:

- 위험한 명령은 훅이 막는다는 전제를 둔다
  - 예: `rm`, `Remove-Item -Recurse`, `git reset --hard`, `git clean -fdx`
- 길고 지저분한 출력이 예상되면 RTK를 우선 쓴다
- 안전하고 명시적인 검증 명령은 그대로 허용한다

코드 탐색이 포함된 작업이면 에이전트는:

- `graphify-out/GRAPH_REPORT.md`를 넓은 코드베이스 읽기 전에 먼저 읽는다
- graphify 쿼리로 파일 범위를 좁힌다

Obsidian 친화적 문서를 사용하는 프로젝트라면 에이전트는:

- 문서 노트를 `Assets/` 아래가 아닌 프로젝트 루트 docs 트리에 유지한다
- `docs/ProjectWiki/index.md`를 메인 허브 노트로 사용한다
- 탐색이 실질적으로 개선될 때만 `[[wiki link]]`를 추가한다

현재 세션에서 delegation이 허용된 경우 에이전트는:

- bounded side task를 분리하기 전에 `docs/SubAgents.md`를 확인한다
- 메인 에이전트가 critical path를 담당한다
- 소유권이 명확한 bounded side task만 위임한다
- 소유권이 겹치는 수정이나 guarded serialized asset 편집은 위임하지 않는다

## 기본 템플릿

```text
작업 목표:
- [원하는 기능/버그 수정 한 줄]

배경:
- [현재 증상 또는 목적]
- [관련 시스템/파일 경로]

수정 허용 범위:
- [예: runtime code folders]
- [필요 시 tests]

수정 금지 범위:
- [예: scenes/prefabs/data assets/project settings]

완료 조건:
- [조건 1]
- [조건 2]
- [조건 3]

순서:
- 관련 파일 전부 읽고 분석한 다음 수정해.

검증:
- AGENTS.md 기준으로 검증해줘.
- Unity 에디터가 닫혀 있으면 batch 검증을 실행해줘.
- Unity 에디터가 열려 있으면 내가 실행할 메뉴와 기대 결과를 같이 알려줘.
- 이번 변경에 맞는 테스트가 필요하면 최소 범위로 추가해줘.

보고 방식:
- 변경 파일
- 검증 결과
- 남은 리스크나 수동 확인 항목
```

## 템플릿 1: 코드 전용 작업

런타임 코드와 테스트 안에서만 작업해야 할 때 사용한다.

## 템플릿 2: 코드 + 데이터 작업

ScriptableObject 또는 config asset 편집이 포함될 때 사용한다.

## 템플릿 3: 씬 또는 프리팹 작업

guarded serialized asset을 의도적으로 허용할 때만 사용한다.

## 템플릿 4: 조사 우선

분석만 원하고 아직 코드 변경은 원하지 않을 때 사용한다.

작업이 불명확하고 실행 전에 에이전트가 컨텍스트를 먼저 파악하길 원할 때 사용한다.

```text
작업 목표:
- [알고 싶은 것 또는 풀고 싶은 문제 한 줄]

역할:
- 아직 코드를 수정하지 마.
- 작업을 시작하기 전에, 최선의 결과를 내기 위해 필요한 컨텍스트를 파악할 수 있게
  질문을 3~4개 해줘. 한 번에 하나씩 물어봐. 내 답변을 받은 뒤 다음 질문으로 넘어가.
- 질문이 끝나면 파악한 내용을 요약하고 작업 계획을 제안해줘.
- 계획을 내가 승인하면 그 때 실행에 들어가.
```

이 방식을 **역방향 프롬프팅**이라고 부른다.
일반적인 "정방향 프롬프팅"(내가 정보를 넣어주는 방식) 대신,
에이전트가 목표를 파악한 뒤 스스로 필요한 컨텍스트를 수집한다.

짧은 요청에도 이 방식을 쓰면 잘못된 가정으로 시작하는 것을 막을 수 있다.

```text
예시:

전투 시스템이 이상하게 동작해. 고치기 전에 질문 3~4개 해줘.
한 번에 하나씩 물어봐.
```

## 템플릿 5: 하네스 업데이트

하네스 킷을 최신 버전으로 업데이트할 때 사용한다.

```text
하네스 킷을 업데이트해줘.
킷 경로: [unity-agent-kit 폴더 경로]
```

에이전트는 `FOR_CLAUDE_CODE.md`의 업데이트 절차를 따른다.
변경 계획을 먼저 제시하고 승인받은 뒤 실행한다.

## 템플릿 6: Cross-Agent Handoff 확인

다른 에이전트가 남긴 unread 작업 내역만 확인하고 싶을 때 사용한다.

```text
Codex 작업 내역 확인해줘.

`docs/AgentHandoffs/pending/codex-to-claude/`만 먼저 확인하고,
unread handoff note가 있으면 관련된 것만 읽어줘.
이해한 note는 `docs/AgentHandoffs/consumed/codex-to-claude/`로 이동해줘.
없으면 handoff note가 없다고만 알려주고 다른 handoff 파일은 읽지 마.
```

```text
Claude Code 작업 내역 확인해줘.

`docs/AgentHandoffs/pending/claude-to-codex/`만 먼저 확인하고,
unread handoff note가 있으면 관련된 것만 읽어줘.
이해한 note는 `docs/AgentHandoffs/consumed/claude-to-codex/`로 이동해줘.
없으면 handoff note가 없다고만 알려주고 다른 handoff 파일은 읽지 마.
```

## 한 줄 단축 요청

```text
[기능/버그]를 수정해줘. 관련 파일 전부 읽고 분석한 다음 수정해. AGENTS.md 규칙을 따르고, 수정은 [허용 범위] 안에서만 해줘. 완료 조건은 [조건]이고, 검증은 가능한 자동으로 수행해줘. Unity 에디터가 열려 있으면 내가 실행할 메뉴와 기대 결과를 같이 알려줘.
```

## 템플릿 7: Delegation 허용

에이전트가 bounded side task를 병렬로 분리하길 원할 때 사용한다.

```text
이 작업은 delegation을 허용할게.

단, 메인 에이전트가 critical path와 최종 검증을 직접 맡고,
sub-agent는 read-only 조사, 테스트/로그 확인, 문서 정리, 명확히 분리된 코드 범위에만 써줘.
같은 파일을 여러 에이전트가 동시에 수정하지 말고, guarded asset은 sub-agent에게 맡기지 마.
```

# Graphify

`graphify`는 구조 이해를 빠르게 하기 위한 knowledge graph 계층이다.  
하네스 규칙, 검증 게이트, guarded asset 규칙을 대체하지 않는다.

## 역할

다음 같은 상황에서 유용하다.

- 시스템 구조를 빠르게 파악해야 할 때
- 여러 코드/문서/노트 사이 연결을 보고 싶을 때
- 반복적인 구조 질문을 적은 토큰으로 처리하고 싶을 때

다음 용도로는 쓰지 않는다.

- Unity 변경의 안전성 보장
- 기능 완료 여부 증명
- validation gate 대체

## 설치

에이전트 환경에 맞게 설치한다.

### Codex

```powershell
pip install graphifyy
graphify install --platform codex
```

Codex 병렬 추출을 쓰려면 `~/.codex/config.toml`에 아래가 필요하다.

```toml
[features]
multi_agent = true
```

### Claude Code (Windows)

```powershell
pip install graphifyy
graphify install
```

## 하네스 호환 규칙

하네스가 이미 있는 Unity 프로젝트에서는 아래 always-on 설치를 기본값으로 쓰지 않는다.

```powershell
graphify codex install
graphify claude install
```

이유:

- `AGENTS.md`를 수정할 수 있다
- `CLAUDE.md`를 수정할 수 있다
- 기존 하네스 규칙과 충돌할 수 있다

필요하다면 자동 설치 대신 수동 검토 후 병합한다.

## 초기 빌드

Unity 프로젝트 루트에서 실행한다.

```powershell
tools\graphify-refresh.cmd .
```

Obsidian vault도 같이 만들려면:

```powershell
tools\graphify-refresh.cmd . --obsidian
```

## 명시적 갱신 워크플로

이 프로젝트는 git hook이나 background watcher를 사용하지 않는다.  
갱신은 수동 명령으로만 수행한다.

```powershell
tools\graphify-refresh.cmd .
```

Obsidian 출력 포함:

```powershell
tools\graphify-refresh.cmd . --obsidian
```

`--obsidian`을 사용하면 `graphify-out/obsidian/` 아래에 Obsidian용 산출물이 함께 생성된다.

## 에이전트 동작 규칙

`graphify-out/GRAPH_REPORT.md`가 있으면:

- 넓은 구조 질문 전에 먼저 읽는다
- wide grep/glob로 전체를 훑기 전에 먼저 읽는다
- 고수준 시스템 파악에 우선 사용한다

구체적인 수정 전에는 필요한 raw source file을 직접 읽는다.

## 추천 운영 방식

- 큰 코드/문서 구조가 바뀐 뒤 `tools\graphify-refresh.cmd .` 실행
- Obsidian vault도 같이 쓰는 프로젝트라면 문서 구조가 크게 바뀐 뒤 `--obsidian`으로 재갱신
- 구조 질문이 반복되면 먼저 `GRAPH_REPORT.md`를 읽고 필요 시 graph query를 사용

## 쿼리 예시

```powershell
graphify query "show the combat flow" --graph graphify-out/graph.json
graphify query "what connects SpawnSystem to DamageSystem?" --graph graphify-out/graph.json
graphify path "ChainLightningSystem" "DamageTextEvent"
graphify explain "CombatSetting"
```

## 기본 산출물

`graphify-out/` 아래에는 보통 아래가 생성된다.

- `GRAPH_REPORT.md`
- `graph.json`
- `graph.html`
- `manifest.json`

`--obsidian`을 사용하면 추가로:

- `obsidian/`

## .gitignore 권장

```gitignore
Library/
Temp/
Logs/
Obj/
Build/
Builds/
UserSettings/
graphify-out/
.graphify_*.json
__pycache__/
Assets/**/graphify-out/
```

필요하면 `.graphifyignore`도 추가한다.

## 보고 규칙

작업 중 `graphify`를 사용했으면 아래를 보고한다.

- `GRAPH_REPORT.md` 읽음 여부
- 그래프 갱신 여부
- 실행한 명령
- 그래프가 오래되었을 가능성

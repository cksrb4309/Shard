# Graphify

`graphify`는 구조 이해를 빠르게 하기 위한 knowledge graph 계층이다.
하네스 규칙, 검증 게이트, guarded asset 규칙을 대체하지 않는다.

## 역할

- 시스템 구조를 빠르게 파악해야 할 때
- 여러 코드/문서/노트 사이 연결을 보고 싶을 때
- 반복적인 구조 질문을 적은 토큰으로 처리하고 싶을 때

validation gate 대체, 기능 완료 증명 용도로는 쓰지 않는다.

## 설치

### Claude Code (Windows)

```powershell
pip install graphifyy
graphify install
```

## 하네스 호환 규칙

아래 명령은 사용 금지 (AGENTS.md / CLAUDE.md 덮어쓸 수 있음):

```powershell
graphify codex install
graphify claude install
```

## 초기 빌드

```powershell
tools\graphify-refresh.cmd .
```

Obsidian vault도 같이 만들려면:

```powershell
tools\graphify-refresh.cmd . --obsidian
```

## 명시적 갱신 워크플로

git hook이나 background watcher를 사용하지 않는다. 갱신은 수동 명령으로만 수행한다.

```powershell
tools\graphify-refresh.cmd .
```

## 에이전트 동작 규칙

`graphify-out/GRAPH_REPORT.md`가 있으면:
- 넓은 구조 질문 전에 먼저 읽는다
- wide grep/glob로 전체를 훑기 전에 먼저 읽는다

구체적인 수정 전에는 필요한 raw source file을 직접 읽는다.

## 쿼리 예시

```powershell
graphify query "show the combat flow" --graph graphify-out/graph.json
graphify query "what connects SpawnManager to AbilityManager?" --graph graphify-out/graph.json
graphify path "SpawnManager" "PoolingManager"
graphify explain "AbilityManager"
```

## 기본 산출물

`graphify-out/` 아래: `GRAPH_REPORT.md`, `graph.json`, `graph.html`, `manifest.json`

## .gitignore 권장

```gitignore
graphify-out/
.graphify_*.json
__pycache__/
```

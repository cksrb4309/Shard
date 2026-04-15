# Graphify Workflow (Explicit)

이 프로젝트는 **자동 훅 없이**, 필요할 때만 명시적으로 graphify 산출물을 갱신한다.

## 운영 원칙

- Git hook은 사용하지 않는다.
- 갱신은 수동 명령으로만 수행한다.
- `graphify-out/` 산출물은 버전관리하지 않는다.
- 먼저 임시 디렉터리에서 빌드하고, 성공했을 때만 최종 `graphify-out/`으로 복사한다.

## 갱신 명령

기본 갱신:

```bat
tools\graphify-refresh.cmd .
```

```powershell
python tools\graphify-refresh.py .
```

Obsidian vault도 같이 생성:

```bat
tools\graphify-refresh.cmd . --obsidian
```

```powershell
python tools\graphify-refresh.py . --obsidian
```

`--obsidian`을 주면 graphify 산출물과 함께 `graphify-out/obsidian/`도 생성된다.

## 질의 방법

그래프가 갱신된 뒤에는 `graphify-out/graph.json`을 대상으로 질의한다.

```powershell
graphify query "show the auth flow" --graph graphify-out/graph.json
graphify path "DigestAuth" "Response" --graph graphify-out/graph.json
```

## 추천 운영 루프

- 구조 질문이 들어오기 전 1회 수동 갱신
- 큰 리팩터링이나 폴더 이동 이후 1회 수동 갱신
- 그래프가 오래되었다고 판단되면 다시 갱신
- Obsidian vault도 같이 쓰는 프로젝트라면 문서 구조가 크게 바뀐 뒤 `--obsidian`으로 재갱신

## 코드 접근 규칙

- 여러 파일을 넓게 훑기 전에 먼저 `graphify-out/GRAPH_REPORT.md`를 읽고 탐색 범위를 좁힌다.
- 쿼리가 필요하면 `graphify query`나 `graphify path`로 연결을 본 뒤 해당 파일만 원문으로 읽는다.
- graphify를 읽었다고 해서 raw source reading이나 검증 게이트를 생략하지 않는다.

## Obsidian 사용

- `--obsidian`을 사용하면 `graphify-out/obsidian/` 아래에 Obsidian vault용 산출물이 생긴다.
- 이 출력은 프로젝트 운영 문서가 아니라 generated artifact로 취급한다.
- 따라서 `graphify-out/obsidian/`도 Git에 커밋하지 않는다.
- 별도 vault 경로가 꼭 필요하면 이 래퍼 대신 graphify CLI를 직접 사용해 `--obsidian-dir`를 지정한다.

## 커밋하지 말아야 할 산출물

- `graphify-out/`
- `graphify-out/obsidian/`
- `.graphify_*.json`
- `__pycache__/`
- 잘못 생긴 `Assets/**/graphify-out/`

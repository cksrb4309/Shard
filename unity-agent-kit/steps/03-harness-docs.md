# Step 03 — 하네스 운영 문서

## 이 단계의 목표

에이전트 운영에 필요한 `docs/` 문서들을 설치한다.

---

## docs/AgentPromptTemplates.md

기준 템플릿: `unity-agent-kit/templates/docs/AgentPromptTemplates.template.md`

- 짧은 요청도 목표/수정 범위/완료 조건/검증으로 내부 확장한다는 기준 포함
- code-only, code+data, scene/prefab, investigation-first 예문 포함
- Codex와 Claude Code 둘 다 참조 가능하게 에이전트 중립으로 유지
- 기존 프로젝트가 `docs/CodexPromptTemplates.md`를 쓰면 그 파일도 병행 갱신

---

## docs/Obsidian.md

기준 템플릿: `unity-agent-kit/templates/docs/Obsidian.template.md`

- 프로젝트 루트를 Obsidian vault로 열 수 있다는 전제 문서화
- 하네스 문서와 위키는 `Assets/` 밖 루트 계층에 유지
- Excluded files 최소 목록: `Library/**`, `Temp/**`, `Logs/**`, `Obj/**`, `UserSettings/**`, `.git/**`
- `docs/HarnessMistakes/**`는 제외하지 않고 허브 링크로 클러스터 유지

---

## docs/RTK.md

기준 템플릿: `unity-agent-kit/templates/docs/RTK.template.md`

- shell output 압축 계층. 바이너리 설치/훅 연결은 전역 가이드로만 다룸
- 효과 큰 명령 예시: `git status/diff/log`, `rg/grep`, `tools\*.cmd`, 테스트 러너 로그
- 효과 낮은 경로 명시: raw source reading, Read/Grep/Glob, unity-cli, unity-prefab-parser-mcp

---

## docs/SubAgents.md

기준 템플릿: `unity-agent-kit/templates/docs/SubAgents.template.md`

- delegation 허용 세션에서만 사용
- sub-agent 대상: read-only 조사, 로그 triage, 문서 정리, disjoint write scope
- 메인 에이전트가 blocking design decision, final validation, final report 담당 유지
- `docs/AgentHandoffs/**`는 내부 sub-agent 결과 저장소로 쓰지 않음

---

## docs/Graphify.md

기준 템플릿: `unity-agent-kit/templates/docs/Graphify.template.md`

모든 프로젝트에 필수 설치. 구조 이해 보조 계층.

### graphify 바이너리 설치

에이전트 환경에 맞게 실행한다.

**Codex:**
```powershell
pip install graphifyy
graphify install --platform codex
```

Codex 병렬 추출을 쓰려면 `~/.codex/config.toml`에 아래도 추가:
```toml
[features]
multi_agent = true
```

**Claude Code (Windows):**
```powershell
pip install graphifyy
graphify install
```

### 운영 규칙

- `graphify codex install` / `graphify claude install`은 사용 금지 (하네스 문서/훅 덮어씀)
- `graphify-out/GRAPH_REPORT.md`가 있으면 넓은 구조 질문 전에 반드시 먼저 읽는다
- 갱신은 `tools/graphify-refresh.cmd` 기반 명시적 갱신만 사용 (자동 훅 연결 금지)

> 초기 빌드(`tools\graphify-refresh.cmd .`)는 tools/ 설치 완료 후 step 08에서 실행한다.

→ 완료되면 `unity-agent-kit/steps/04-mistakes.md`를 읽는다.

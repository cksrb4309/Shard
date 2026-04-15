# Step 05 — Handoff & Wiki 구조

## 이 단계의 목표

`docs/AgentHandoffs/**`와 `docs/ProjectWiki/**`를 설치한다.

---

## docs/AgentHandoffs/

기준 템플릿: `unity-agent-kit/templates/docs/AgentHandoffs/**`

```
docs/AgentHandoffs/
  README.md
  _handoff-template.md
  pending/
    codex-to-claude/.gitkeep
    claude-to-codex/.gitkeep
  consumed/
    codex-to-claude/.gitkeep
    claude-to-codex/.gitkeep
```

**Handoff 운영 규칙 (설치 후 에이전트 동작):**
- 자동 로드 안 함. 사용자가 "작업 내역 확인해줘"라고 할 때만 확인
- 또는 직전 작업을 다른 에이전트가 넘긴 상황이 명확할 때
- `pending/<other-to-self>/` 파일명만 먼저 확인. `.gitkeep`만 있으면 더 읽지 않음
- 읽은 note는 같은 턴에 `consumed/<other-to-self>/`로 이동
- consumed는 사용자가 다시 보라고 하지 않는 한 재독 금지
- Note는 짧게: 작업 목표/현재 상태/변경 파일/수행 검증/남은 리스크/다음 액션
- 파일명: `YYYY-MM-DD-HHMM-topic.md`

---

## docs/ProjectWiki/

기준 템플릿: `unity-agent-kit/templates/docs/ProjectWiki/**`

```
docs/ProjectWiki/
  README.md
  index.md          ← 전체 페이지 카탈로그
  log.md            ← 작업 이력 (append-only)
  systems/_template.md
  assets/_template.md
  adr/_template.md
```

**Wiki 운영 규칙 (설치 후 에이전트 동작):**
- 시스템 조사/수정 시 → `systems/<SystemName>.md`
- 씬/프리팹/에셋 의존성 파악 시 → `assets/<AssetName>.md`
- 아키텍처 결정 확인 시 → `adr/ADR-<NNN>-<slug>.md`
- 새 페이지 → 반드시 `index.md`에 한 줄 추가
- 작업 완료 → `log.md`에 항목 append
- `log.md`를 handoff inbox로 쓰지 않음
- 코드/씬/프리팹 원본과 모순되면 원본이 우선

→ 완료되면 `unity-agent-kit/steps/06-tools.md`를 읽는다.

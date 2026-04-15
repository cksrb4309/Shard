# Step 04 — 실수 학습 구조

## 이 단계의 목표

`docs/HarnessMistakes/**` 구조를 설치한다.

---

## 설치 대상

기준 템플릿: `unity-agent-kit/templates/docs/HarnessMistakes/**`

```
docs/HarnessMistakes/
  README.md
  domains/
    README.md               ← primary domain 선택기
    combat-gameplay.md
    ui-bridge.md
    scene-prefab.md
    data-assets.md
    vfx-presentation.md
    validation-tooling.md
    docs-harness.md
  categories/
    README.md
    scope-boundary.md
    context-drift.md
    validation-gap.md
    behavioral-regression.md
    tooling-automation-gap.md
    communication-handoff.md
```

---

## 실수 루프 운영 규칙 (설치 후 에이전트 동작)

**작업 시작 시:**
1. `docs/HarnessMistakes/domains/README.md` 읽기 → primary domain 선택
2. 해당 `domains/<domain>.md` 하나만 읽기
3. 그 파일에 `Preload extra mistake context: yes`일 때만 명시된 category 파일도 읽기

**실수 발생 시:**
1. primary category 분류
2. 해당 `categories/<category>.md` 갱신
   - 최소 포함: what happened / why it happened / what rule changed / what validation changed
3. 해당 `domains/<domain>.md`의 `Known recurring mistakes`와 preload 여부 갱신
4. 규칙으로 승격 필요하면 `AGENTS.md`, `CLAUDE.md`, `docs/AgentPromptTemplates.md` 반영

**성공 패턴 승격:**
잘 된 작업 방식이 반복 가능하면 → `AGENTS.md`의 architecture rules 또는 default request interpretation에 반영

**실수 루프 트리거 조건:**
- 사용자가 "이건 실수였다", "재발 방지 기록 남겨라" 등 명시
- 검증 누락, 범위 위반, 잘못된 가정, 잘못된 완료 보고가 확인됨
- 최소 안전 게이트는 통과했으나 의도된 동작을 놓친 구조적 원인이 드러남

→ 완료되면 `unity-agent-kit/steps/05-handoffs-wiki.md`를 읽는다.

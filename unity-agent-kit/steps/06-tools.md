# Step 06 — 검증 도구

## 이 단계의 목표

`tools/`, `Assets/Editor/HarnessValidation/`, `Assets/99_Tests/`를 설치한다.

---

## tools/

Unity 검증 스크립트는 `unity-agent-kit/templates/tools/**`에서, Graphify 갱신 스크립트는 `unity-agent-kit/tools/`에서 복사한다.

설치 대상:
- `tools/unity-batch.ps1` ← `unity-agent-kit/templates/tools/unity-batch.ps1`
- `tools/compile-unity.cmd` ← `unity-agent-kit/templates/tools/compile-unity.cmd`
- `tools/smoke-unity.cmd` ← `unity-agent-kit/templates/tools/smoke-unity.cmd`
- `tools/test-editmode.cmd` ← `unity-agent-kit/templates/tools/test-editmode.cmd`
- `tools/test-playmode.cmd` ← `unity-agent-kit/templates/tools/test-playmode.cmd`
- `tools/validate-unity.cmd` ← `unity-agent-kit/templates/tools/validate-unity.cmd`
- `tools/graphify-refresh.cmd` ← `unity-agent-kit/tools/graphify-refresh.cmd`
- `tools/graphify-refresh.py` ← `unity-agent-kit/tools/graphify-refresh.py`

Unity 검증 스크립트는 복사 후 프로젝트 실정(Unity 설치 경로, 프로젝트 경로)에 맞게 조정.
Graphify 스크립트는 수정 없이 그대로 복사.

---

## Assets/Editor/HarnessValidation/

기준 템플릿: `unity-agent-kit/templates/Assets/Editor/HarnessValidation/BatchValidationRunner.template.cs`

최소 기능:
- smoke validation
- strict smoke validation
- editor menu items
- validation log folder open

에디터 메뉴 이름 (에이전트 중립):
- `Tools/Harness Validation/Run Smoke Validation`
- `Tools/Harness Validation/Run Strict Smoke Validation`
- `Tools/Harness Validation/Run EditMode Smoke Tests`
- `Tools/Harness Validation/Run Full Validation`

기존 프로젝트가 `Tools/Codex Validation/*`을 쓰면 이름 유지.

---

## Assets/99_Tests/

기준 템플릿: `unity-agent-kit/templates/Assets/99_Tests/EditMode/`

템플릿을 복사한 뒤 아래를 조정한다:
- 테스트 클래스명을 프로젝트 실제 시스템 이름으로 변경
- `.asmdef`의 Assembly Reference를 프로젝트 실제 어셈블리로 교체
- 핵심 시스템 중 deterministic하고 editor-only 재현 가능한 것에 smoke test 1개 이상 작성

예: damage apply, inventory stat application, ability unlock flow, progression request consumption

직접 참조가 어려우면 reflection 기반 테스트 허용.

→ 완료되면 `unity-agent-kit/steps/07-companion.md`를 읽는다.

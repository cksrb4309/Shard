# Step 08 — 검증 및 보고

## 이 단계의 목표

가능한 검증을 실행하고 설치 결과를 보고한다.

---

## 체크리스트 확인

`unity-agent-kit/AGENT_KIT_INSTALL_SPEC.md`의 최소 산출물 체크리스트를 다시 확인한다.
누락 항목이 있으면 해당 단계 파일로 돌아가서 완료한다.

---

## Graphify 초기 빌드

```powershell
tools\graphify-refresh.cmd .
```

`graphify-out/GRAPH_REPORT.md`, `graph.json`, `graph.html`, `manifest.json`이 생성되면 정상.
실패하면 `pip install graphifyy`와 `graphify install`이 step 03에서 완료됐는지 확인한다.

---

## 검증 실행

에디터가 **닫혀 있으면** 직접 실행:

1. `tools\compile-unity.cmd`
2. `tools\smoke-unity.cmd`
3. `tools\test-editmode.cmd` (해당되면)
4. `tools\validate-unity.cmd` (해당되면)

에디터가 **열려 있으면** batchmode 실행 불가. 대신:
- 사용자가 실행할 메뉴 경로를 알려준다
- 기대 결과를 구체적으로 적는다
- `Tools/Harness Validation/Run Smoke Validation`부터 시작

---

## 보고 형식

```
하네스 설치 완료
설치 버전: [kit-commit 7자]

설치/갱신 파일: [목록]
수행한 검증: [결과]
미실행 검증: [이유]
남은 리스크: [항목]
수동 확인 필요: [항목]
```

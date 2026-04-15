## 하네스와 검증

이 저장소는 프롬프트 단독이 아닌 실질적인 하네스를 사용한다.

관련 문서:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/AgentPromptTemplates.md`
- `docs/HarnessMistakes/README.md`
- `docs/Graphify.md`

검증 커맨드:

```powershell
tools\compile-unity.cmd
tools\smoke-unity.cmd
tools\test-editmode.cmd
tools\validate-unity.cmd
```

Unity 에디터가 이미 열려 있으면 아래 에디터 메뉴 항목을 사용한다:

- `Tools/Harness Validation/Run Smoke Validation`
- `Tools/Harness Validation/Run Strict Smoke Validation`
- `Tools/Harness Validation/Run EditMode Smoke Tests`
- `Tools/Harness Validation/Run Full Validation`

이 섹션을 아래 프로젝트 고유 내용으로 교체한다:

- 주 검증 씬
- 보호 대상 프리팹 또는 데이터 에셋
- 현재 smoke test 커버리지
- 실패 확정 후 실수 범주화 기록 방법
- `graphify` 구조 이해 사용 여부 및 갱신 방법

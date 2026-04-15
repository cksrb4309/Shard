# For Claude Code

## 진입 절차

1. `unity-agent-kit/AGENT_KIT_INSTALL_SPEC.md`를 읽는다 — 원칙, 설치 절차, 체크리스트 확인
2. `unity-agent-kit/steps/01-scan.md`부터 순서대로 읽고, 한 단계씩 완료한다
3. 각 단계 파일이 필요한 템플릿을 지정한다. 단계 시작 전에 모든 `templates/**`를 미리 읽지 않는다

업데이트 요청이라면: `unity-agent-kit/steps/09-update.md`를 읽는다.

## Claude Code 전용 운영 규칙

- **읽기-분석-쓰기 순서**: Read/Grep/Glob을 한 번에 병렬로 실행 → 분석 → Edit/Write/Bash.
  읽기와 쓰기를 여러 턴에 걸쳐 섞지 않는다.
- **`/compact` 제안 조건**: 주제 전환 3회 이상, 파일 수정 10개 이상, 컨텍스트 압축이 이미 발생했을 때.
  요약 형식: `완료 목록 | 현재 상태(git 커밋 등) | 미완료`.
- **`CLAUDE.md` 100줄 이하 엄수**: 매 세션 자동 로드. 길수록 토큰 낭비 선형 증가.
- **dual-agent 기본**: Claude Code가 시작해도 Codex가 바로 작업 가능한 구조로 설치.
- 짧은 요청도 내부적으로 목표/수정 범위/완료 조건/검증으로 확장한다.
- `AGENTS.md`는 기본값으로 Codex 호환 자기완결형 유지. `AGENTS.claude.template.md`는 사용자가 명시할 때만 사용.

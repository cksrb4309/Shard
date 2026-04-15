# Start Here

이 킷은 Codex와 Claude Code 둘 다 사용할 수 있습니다.

사용하는 에이전트에 맞춰 아래 파일 중 하나를 첫 메시지로 보내면 됩니다.

- [START_HERE_CODEX.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\START_HERE_CODEX.md)
- [START_HERE_CLAUDE_CODE.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\START_HERE_CLAUDE_CODE.md)

공통 전제는 이렇습니다.

- 프로젝트 루트의 `unity-agent-kit`를 기준으로 작업
- `unity-agent-kit`와 하네스 문서는 `Assets/` 아래가 아니라 프로젝트 루트에 둠
- 공용 설치 규격은 [AGENT_KIT_INSTALL_SPEC.md](C:\Users\rlack\Desktop\노션\unity-agent-kit\AGENT_KIT_INSTALL_SPEC.md)
- 에이전트별 보조 지침은 각자의 `FOR_*.md`
- Codex와 Claude Code를 같이 쓸 수 있게 `docs/AgentHandoffs/**` 구조도 함께 설치
- 어느 에이전트로 시작하든 다른 쪽 프로젝트 로컬 설정도 같이 맞추는 shared-project dual-agent 모드가 기본값
- Claude Desktop 설정파일이 있으면 전역 `mcpServers`도 merge 대상으로 취급
- `docs/Obsidian.md`는 기본 산출물로 설치하고, 프로젝트 루트를 Obsidian vault로 열 수 있게 문서 링크 규칙을 맞춤
- `docs/RTK.md`는 기본 산출물로 설치하고, 실제 RTK 설치는 전역 가이드로만 다룸
- `docs/SubAgents.md`는 기본 산출물로 설치하고, 실제 sub-agent 사용은 세션 정책과 delegation 허용 여부를 따름
- `graphify`는 기본 산출물로 설치하고, `docs/Graphify.md`를 통해 구조 이해 계층으로 사용

짧게 말하고 싶으면 이 한 줄도 가능합니다.

```text
루트의 `unity-agent-kit` 기준으로 이 Unity 프로젝트에 에이전트 운영 구조를 설치해줘.
```

다만 가장 안정적인 방식은 에이전트별 시작 문서를 그대로 쓰는 것입니다.

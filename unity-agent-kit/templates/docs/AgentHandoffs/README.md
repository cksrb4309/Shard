# AgentHandoffs

이 폴더는 Codex와 Claude Code가 서로 넘겨야 하는 짧은 작업 인계 메모를 저장한다.

- `docs/ProjectWiki/**`는 장기 지식용
- `docs/AgentHandoffs/**`는 현재 작업 handoff용

두 역할을 섞지 않는다.

## Structure

```text
docs/AgentHandoffs/
├── README.md
├── _handoff-template.md
├── pending/
│   ├── codex-to-claude/
│   └── claude-to-codex/
└── consumed/
    ├── codex-to-claude/
    └── claude-to-codex/
```

## Read Rules

- 모든 작업에서 이 폴더를 자동으로 읽지 않는다.
- 사용자가 `Codex 작업 내역 확인해줘` 또는 `Claude Code 작업 내역 확인해줘`처럼 명시했을 때 먼저 확인한다.
- 또는 다른 에이전트가 끝낸 작업을 바로 이어받는 상황이 명확할 때만 확인한다.
- 먼저 대응되는 `pending/<other-to-self>/` 폴더의 파일명만 확인한다.
- `.gitkeep`만 있으면 handoff note가 없는 것이다. 더 읽지 않는다.
- 실제 note가 있으면 현재 작업과 관련된 note만 최신순으로 읽는다.

## Move Rules

- note를 읽어서 이해했으면 같은 작업 턴 안에서 `consumed/<other-to-self>/`로 이동한다.
- 이미 `consumed/**` 아래에 있는 note는 사용자가 다시 보라고 하지 않는 한 재독하지 않는다.
- note를 읽고도 `pending/**`에 그대로 두지 않는다. unread inbox로 남아야 하기 때문이다.

## Write Rules

- 다른 에이전트가 이어받을 가능성이 있는 비사소한 작업을 끝냈다면 handoff note를 남긴다.
- 경로는 `pending/<self-to-other>/YYYY-MM-DD-HHMM-topic.md`
- 같은 주제에 대해 아직 읽히지 않은 최신 pending note가 있으면 새 note를 남발하지 말고 갱신해도 된다.
- note는 짧게 유지한다. 코드 diff 전문이나 장문 회고는 넣지 않는다.
- 필요한 경우에만 `docs/ProjectWiki/**` 문서를 같이 갱신하고, handoff note에는 그 파일 경로만 적는다.
- Obsidian을 쓴다면 긴 설명 대신 관련 위키 note를 `[[wiki link]]`로 가리켜도 된다.

## Recommended Sections

- From / To / Date
- Task
- Current State
- Files Changed
- Validation
- Files To Read First
- Open Risks Or Manual Steps
- Next Action

자세한 형식은 `_handoff-template.md`를 따른다.

# Domain: Scene Prefab

Related:
- [[docs/HarnessMistakes/README]]
- [[docs/HarnessMistakes/domains/README]]

## Scope

씬, 프리팹, 인스펙터 레퍼런스, MonoBehaviour 연결, serialized hierarchy 변경.

## Typical Areas

- `Assets/**/*.unity`
- `Assets/**/*.prefab`
- scene bootstrap objects
- inspector references

## Routing Metadata

- Preload extra mistake context: no
- Preload category files: none

## Known Recurring Mistakes

- none recorded

## Update Rule

씬/프리팹 오수정, missing reference, guarded asset 실수가 반복되면 이 파일을 갱신하고 preload를 켠다.

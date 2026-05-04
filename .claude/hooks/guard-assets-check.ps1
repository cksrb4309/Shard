# guard-assets-check.ps1
# PreToolUse hook: Unity guarded asset 편집 차단
# unity-agent-kit이 설치한 파일 — 프로젝트별 가드 경로는 아래 $guardedPatterns에 추가한다.

param()

$stdin = [Console]::In.ReadToEnd()
if (-not $stdin) { exit 0 }

try {
    $data = $stdin | ConvertFrom-Json
} catch {
    exit 0
}

$toolName = $data.tool_name
if ($toolName -notin @("Edit", "Write")) { exit 0 }

$filePath = $data.tool_input.file_path
if (-not $filePath) { exit 0 }

# 기본 가드 패턴 + Shard 프로젝트 ScriptableObject 경로
$guardedPatterns = @(
    '\.unity$',
    '\.prefab$',
    '[/\\]ProjectSettings[/\\]',
    '[/\\]12\.ScriptableObject[/\\]'
)

foreach ($pattern in $guardedPatterns) {
    if ($filePath -match $pattern) {
        $msg = @"
[HARNESS] GUARDED ASSET BLOCKED
파일: $filePath

이 경로는 기본값으로 수정 금지(guarded asset)입니다.
  - .unity  씬 파일
  - .prefab 프리팹 파일
  - ProjectSettings/**
  - Assets/12.ScriptableObject/**

수정이 필요하면:
  1. AGENTS.md의 고위험 영역 항목을 확인한다.
  2. 사용자에게 명시적 허가를 요청한다.
  3. 허가 후 .claude/settings.json의 해당 deny 항목을 일시적으로 제거한다.
"@
        Write-Host $msg
        exit 1
    }
}

exit 0

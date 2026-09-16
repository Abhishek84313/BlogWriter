# Console Command Contract: Session Discovery

## Entry Prompt

The initial console prompt accepts a new blog topic, `resume <session-id>`, `list`, or an empty line to exit.

## `list`

**Input**: The literal command `list`, case-insensitive, with optional surrounding whitespace.

**Success output**: At most 20 sessions belonging to the signed-in user, ordered from newest to oldest. Each entry includes:

- Session identifier
- Original question
- Created time
- Last-updated time

**Empty output**: A clear message that no saved sessions are available.

**Failure output**: A clear storage or identity error and a prompt to retry; the application remains running.

## `resume <session-id>`

**Input**: `resume` followed by one 32-character session identifier.

**Success output**: The saved state becomes the active session and continues through the existing workflow.

**Not found**: A clear message that the session is unavailable or belongs to another user; no session is created.

**Conflict after save**: A clear message that the session changed elsewhere and must be reloaded before retrying the follow-up.

## New Topic and Follow-Up

New-topic and follow-up input contracts remain unchanged. Each successful state transition is persisted through the long-term session store using the active signed-in user's identity.
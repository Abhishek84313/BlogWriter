# Console Command Contract: Session List and Recall

## Entry Prompt

The initial console prompt accepts a new blog topic, `resume <number>`, `list`, or an empty line to exit.

## `list`

**Input**: The literal command `list`, case-insensitive, with optional surrounding whitespace.

**Success output**: At most 20 sessions belonging to the signed-in user, ordered from newest to oldest. Each session entry begins with a unique one-based label in the form `[n]`, followed by the existing session summary information:

- Original question
- Last-updated time
- Created time

The underlying session identifier remains internal and is not accepted as user input.

**Empty output**: A clear message that no saved sessions are available; no numbered entries are displayed.

**Failure output**: A clear storage or identity error and a prompt to retry; the application remains running.

## `resume <number>`

**Input**: `resume` followed by a positive one-based number from the current previous-session list, with optional surrounding whitespace.

**Success output**: The corresponding saved state becomes the active session and continues through the existing workflow.

**Invalid selection**: Empty, non-numeric, zero, negative, or out-of-range values produce a clear invalid-selection message. The active session is not replaced and no new session is created.

**Stale or unavailable entry**: The number is resolved against the current list. If the selected session cannot be loaded, the application reports that it is unavailable and preserves the active session.

Raw session identifiers entered after `resume` are rejected with a clear numeric-selection message; no session is created.

## New Topic and Follow-Up

New-topic and follow-up input contracts remain unchanged. Each successful state transition is persisted through the long-term session store using the active signed-in user's identity.

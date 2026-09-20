namespace BlogWriter.Web.Services;

public sealed class BlogWorkspaceService : IDisposable
{
    private readonly IBlogWriterSessionService _sessions;
    private readonly TimeSpan _cancellationTimeout;
    private CancellationTokenSource? _operationCancellation;
    private Task? _activeOperation;
    private long _operationVersion;

    public BlogWorkspaceService(IBlogWriterSessionService sessions)
        : this(sessions, TimeSpan.FromSeconds(10))
    {
    }

    public BlogWorkspaceService(IBlogWriterSessionService sessions, TimeSpan cancellationTimeout)
    {
        _sessions = sessions;
        _cancellationTimeout = cancellationTimeout;
    }

    public BlogWorkspaceState State { get; } = new();

    public event Action? Changed;

    public Task SubmitInitialAsync()
    {
        if (!TryCaptureRange(out WordRange range))
        {
            return Task.CompletedTask;
        }

        string prompt = State.InitialPrompt;
        return RunSessionOperationAsync(
            prompt,
            range,
            cancellationToken => _sessions.StartAsync(prompt, range.Min, range.Max, cancellationToken),
            clearInput: () => State.InitialPrompt = "");
    }

    public Task SubmitRevisionAsync()
    {
        if (State.ActiveSession is null)
        {
            SetValidation("Create or load a session before submitting a revision.");
            return Task.CompletedTask;
        }

        BlogSession activeSession = State.ActiveSession;
        if (!TryCaptureRange(out WordRange range))
        {
            return Task.CompletedTask;
        }

        string revision = State.RevisionPrompt;
        return RunSessionOperationAsync(
            revision,
            range,
            cancellationToken => _sessions.ReviseAsync(
                activeSession,
                revision,
                range.Min,
                range.Max,
                cancellationToken),
            clearInput: () => State.RevisionPrompt = "");
    }

    public void UpdateMinWords(string value)
    {
        State.MinWordsInput = value;
        ValidateVisibleRange();
    }

    public void UpdateMaxWords(string value)
    {
        State.MaxWordsInput = value;
        ValidateVisibleRange();
    }

    public async Task<WorkspaceTransitionResult> NewAsync(bool discardConfirmed)
    {
        if (RequiresDiscardConfirmation(discardConfirmed))
        {
            return WorkspaceTransitionResult.RequiresConfirmation;
        }

        await CancelActiveOperationAsync();
        ClearWorkspace(WorkspaceMode.New);
        return WorkspaceTransitionResult.Completed;
    }

    public async Task<WorkspaceTransitionResult> ListAsync(bool discardConfirmed)
    {
        if (RequiresDiscardConfirmation(discardConfirmed))
        {
            return WorkspaceTransitionResult.RequiresConfirmation;
        }

        await CancelActiveOperationAsync();
        RestoreAcceptedRange();
        State.InitialPrompt = "";
        State.RevisionPrompt = "";
        State.SelectionInput = "";
        State.ValidationMessage = null;
        State.StatusMessage = "Loading saved sessions...";
        NotifyChanged();

        try
        {
            State.DisplayedSessions = (await _sessions.ListAsync()).Take(20).ToList();
            State.Mode = WorkspaceMode.List;
            State.StatusMessage = State.DisplayedSessions.Count == 0
                ? "No saved sessions are available."
                : $"{State.DisplayedSessions.Count} saved sessions loaded.";
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State.DisplayedSessions = [];
            State.Mode = WorkspaceMode.Draft;
            State.ValidationMessage = "Unable to load saved sessions. Try again.";
            State.StatusMessage = null;
        }

        NotifyChanged();
        return WorkspaceTransitionResult.Completed;
    }

    public async Task LoadSelectionAsync()
    {
        if (!SessionListSelection.TryResolve(State.SelectionInput, State.DisplayedSessions, out BlogSessionSummary? summary))
        {
            SetValidation("Enter a number from the current saved-session list.");
            return;
        }

        State.ValidationMessage = null;
        try
        {
            BlogSession? session = await _sessions.LoadAsync(summary!.Id);
            if (session is null)
            {
                SetValidation("That saved session is no longer available. Refresh the list and try again.");
                return;
            }

            Publish(session);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SetValidation("Unable to load the selected session. Try again.");
        }
    }

    public async Task<WorkspaceTransitionResult> QuitAsync(bool discardConfirmed)
    {
        if (RequiresDiscardConfirmation(discardConfirmed))
        {
            return WorkspaceTransitionResult.RequiresConfirmation;
        }

        await CancelActiveOperationAsync();
        ClearWorkspace(WorkspaceMode.Ended);
        State.StatusMessage = "This Blog Writer session has ended.";
        NotifyChanged();
        return WorkspaceTransitionResult.Completed;
    }

    public async Task EndForAuthenticationLossAsync()
    {
        await CancelActiveOperationAsync();
        ClearWorkspace(WorkspaceMode.Ended);
        State.ValidationMessage = "Microsoft Entra sign-in is required.";
        NotifyChanged();
    }

    public void Dispose()
    {
        _operationCancellation?.Cancel();
        _operationCancellation?.Dispose();
    }

    private async Task RunSessionOperationAsync(
        string input,
        WordRange submittedRange,
        Func<CancellationToken, Task<BlogSession>> operation,
        Action clearInput)
    {
        if (State.Mode == WorkspaceMode.Ended)
        {
            SetValidation("This session has ended. Refresh the page to start again.");
            return;
        }

        if (State.IsProcessing)
        {
            SetValidation("A writing operation is already in progress.");
            return;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            SetValidation("Enter a prompt before submitting.");
            return;
        }

        long version = ++_operationVersion;
        var cancellation = new CancellationTokenSource();
        _operationCancellation = cancellation;
        State.IsProcessing = true;
        State.ValidationMessage = null;
        State.StatusMessage = "Writing in progress...";
        NotifyChanged();

        Task<BlogSession> task = operation(cancellation.Token);
        _activeOperation = task;
        try
        {
            BlogSession session = await task;
            if (version != _operationVersion)
            {
                return;
            }

            clearInput();
            Publish(session, submittedRange);
            State.StatusMessage = "Writing complete.";
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            if (version == _operationVersion)
            {
                State.StatusMessage = "Writing cancelled.";
            }
        }
        catch (Exception exception)
        {
            if (version == _operationVersion)
            {
                State.ValidationMessage = exception is SessionConflictException
                    ? "This session changed elsewhere. Refresh the list before retrying."
                    : "The writing operation failed. Your previous draft is unchanged.";
                State.StatusMessage = null;
            }
        }
        finally
        {
            if (version == _operationVersion)
            {
                State.IsProcessing = false;
                _activeOperation = null;
                _operationCancellation = null;
                cancellation.Dispose();
                NotifyChanged();
            }
        }
    }

    private bool RequiresDiscardConfirmation(bool discardConfirmed) =>
        State.HasUnsavedText && !discardConfirmed;

    private async Task CancelActiveOperationAsync()
    {
        Task? activeOperation = _activeOperation;
        CancellationTokenSource? cancellation = _operationCancellation;
        ++_operationVersion;
        _activeOperation = null;
        _operationCancellation = null;
        State.IsProcessing = false;

        if (activeOperation is null || cancellation is null)
        {
            return;
        }

        cancellation.Cancel();
        Task completed = await Task.WhenAny(activeOperation, Task.Delay(_cancellationTimeout));
        if (completed == activeOperation)
        {
            try
            {
                await activeOperation;
            }
            catch
            {
            }

            cancellation.Dispose();
            return;
        }

        _ = activeOperation.ContinueWith(
            task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private void Publish(BlogSession session, WordRange? submittedRange = null)
    {
        WordRange persistedRange = GetSessionRange(session);
        if (submittedRange is null)
        {
            SetAcceptedAndVisibleRange(persistedRange);
        }
        else
        {
            WordRangeValidation visibleRange = WordRange.Parse(State.MinWordsInput, State.MaxWordsInput);
            bool editedDuringProcessing = !visibleRange.IsValid || visibleRange.Range != submittedRange;
            State.AcceptedRange = submittedRange.Value;
            if (!editedDuringProcessing)
            {
                SetVisibleRange(persistedRange);
            }
            else
            {
                ValidateVisibleRange(notify: false);
            }
        }

        State.ActiveSession = session;
        State.Draft = session.State.Draft;
        State.Review = session.State.ReviewNotes;
        State.DisplayedSessions = [];
        State.SelectionInput = "";
        State.Mode = WorkspaceMode.Draft;
        State.ValidationMessage = null;
        NotifyChanged();
    }

    private void ClearWorkspace(WorkspaceMode mode)
    {
        State.Mode = mode;
        State.InitialPrompt = "";
        State.RevisionPrompt = "";
        SetAcceptedAndVisibleRange(WordRange.Default);
        State.Draft = "";
        State.Review = "";
        State.ActiveSession = null;
        State.DisplayedSessions = [];
        State.SelectionInput = "";
        State.IsProcessing = false;
        State.StatusMessage = null;
        State.ValidationMessage = null;
        NotifyChanged();
    }

    private void SetValidation(string message)
    {
        State.ValidationMessage = message;
        NotifyChanged();
    }

    private bool TryCaptureRange(out WordRange range)
    {
        WordRangeValidation validation = ValidateVisibleRange(notify: false);
        if (validation.Range is not WordRange validRange)
        {
            range = default;
            State.ValidationMessage = "Correct Min and Max before submitting.";
            NotifyChanged();
            return false;
        }

        range = validRange;
        State.ValidationMessage = null;
        return true;
    }

    private WordRangeValidation ValidateVisibleRange(bool notify = true)
    {
        WordRangeValidation validation = WordRange.Parse(State.MinWordsInput, State.MaxWordsInput);
        State.MinWordsError = validation.MinError;
        State.MaxWordsError = validation.MaxError;
        if (validation.IsValid && State.ValidationMessage == "Correct Min and Max before submitting.")
        {
            State.ValidationMessage = null;
        }

        if (notify)
        {
            NotifyChanged();
        }

        return validation;
    }

    private static WordRange GetSessionRange(BlogSession session)
    {
        WordRangeValidation validation = WordRange.Parse(
            session.State.MinWords.ToString(),
            session.State.MaxWords.ToString());
        return validation.Range ?? WordRange.Default;
    }

    private void RestoreAcceptedRange() => SetVisibleRange(State.AcceptedRange);

    private void SetAcceptedAndVisibleRange(WordRange range)
    {
        State.AcceptedRange = range;
        SetVisibleRange(range);
    }

    private void SetVisibleRange(WordRange range)
    {
        State.MinWordsInput = range.Min.ToString();
        State.MaxWordsInput = range.Max.ToString();
        State.MinWordsError = null;
        State.MaxWordsError = null;
    }

    private void NotifyChanged() => Changed?.Invoke();
}

ALTER TABLE practice_sessions
    MODIFY COLUMN status ENUM(
        'Practicing',
        'VpCompleted',
        'ReasoningStarted',
        'Submitted',
        'Completed',
        'Abandoned'
    ) DEFAULT 'Practicing';

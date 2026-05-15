ALTER TABLE practice_sessions
    MODIFY COLUMN status ENUM(
        'Practicing',
        'VpCompleted',
        'ReasoningStarted',
        'Completed',
        'Abandoned'
    ) DEFAULT 'Practicing';

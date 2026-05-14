-- -----------------------------------------------------------------------------
-- users
-- -----------------------------------------------------------------------------
CREATE INDEX idx_users_role_status
    ON users (role, status)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- user_refresh_tokens
-- -----------------------------------------------------------------------------
CREATE INDEX idx_refresh_tokens_expires_revoked
    ON user_refresh_tokens (expires_at, is_revoked)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_refresh_tokens_user_revoked
    ON user_refresh_tokens (user_id, is_revoked)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- revoked_access_tokens
-- -----------------------------------------------------------------------------
CREATE INDEX idx_revoked_tokens_expires
    ON revoked_access_tokens (expires_at)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- clinical_case
-- -----------------------------------------------------------------------------
CREATE INDEX idx_clinical_case_created_by
    ON clinical_case (created_by)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_clinical_case_status_created
    ON clinical_case (status, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;

-- type là TEXT → dùng prefix 50
CREATE INDEX idx_clinical_case_type_status
    ON clinical_case (type(50), status)
    ALGORITHM=INPLACE, LOCK=NONE;

-- Full-text search
ALTER TABLE clinical_case
    ADD FULLTEXT INDEX ft_clinical_case_title_desc (title, description);


-- -----------------------------------------------------------------------------
-- virtual_patient
-- -----------------------------------------------------------------------------
CREATE INDEX idx_virtual_patient_case_status
    ON virtual_patient (case_id, status)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_virtual_patient_level_status
    ON virtual_patient (level, status)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- laboratorytest
-- -----------------------------------------------------------------------------
CREATE INDEX idx_lab_clinicalcase
    ON laboratorytest (clinicalcase_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_lab_item
    ON laboratorytest (itemid)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- radiologyreport
-- -----------------------------------------------------------------------------
CREATE INDEX idx_radio_clinicalcase
    ON radiologyreport (clinicalcase_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_radio_modality
    ON radiologyreport (modality)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- notification
-- -----------------------------------------------------------------------------
CREATE INDEX idx_notification_receiver_status_created
    ON notification (receiver, status, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_notification_sender_created
    ON notification (sender, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- assessments
-- -----------------------------------------------------------------------------
CREATE INDEX idx_assessments_topic_difficulty
    ON assessments (topic, difficulty_level)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_assessments_active
    ON assessments (is_active)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- question
-- -----------------------------------------------------------------------------
CREATE INDEX idx_question_assessment
    ON question (assessment_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_question_assessment_cognitive
    ON question (assessment_id, cognitive_level)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- assessment_session
-- -----------------------------------------------------------------------------
CREATE INDEX idx_assessment_session_learner_time
    ON assessment_session (learner_id, start_time DESC)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_assessment_session_learner_assessment
    ON assessment_session (learner_id, assessment_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_assessment_session_assessment_status
    ON assessment_session (assessment_id, status, is_passed)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_as_assessment
    ON assessment_session (assessment_id)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- assessment_answer
-- -----------------------------------------------------------------------------
CREATE INDEX idx_assessment_answer_session
    ON assessment_answer (session_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_assessment_answer_session_flagged
    ON assessment_answer (session_id, is_flagged)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_assessment_answer_question_correct
    ON assessment_answer (question_id, is_correct)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- practice_sessions
-- -----------------------------------------------------------------------------
CREATE INDEX idx_practice_sessions_learner_created
    ON practice_sessions (learner_id, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_practice_sessions_learner_status
    ON practice_sessions (learner_id, status)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_practice_sessions_patient_status
    ON practice_sessions (patient_id, status)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_practice_sessions_created_at
    ON practice_sessions (created_at)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_ps_guidelines
    ON practice_sessions (guidelines_id)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- evaluation
-- -----------------------------------------------------------------------------
CREATE INDEX idx_evaluation_session
    ON evaluation (practice_session_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_evaluation_epa_created
    ON evaluation (epa_id, created_at)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- evaluation_epa_score
-- -----------------------------------------------------------------------------
CREATE INDEX idx_epa_score_eval_epa
    ON evaluation_epa_score (evaluation_id, epa_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_epa_score_covering
    ON evaluation_epa_score (evaluation_id, epa_id, numerical_score, entrustment_level, created_at)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- practice_feedback
-- -----------------------------------------------------------------------------
CREATE INDEX idx_feedback_session
    ON practice_feedback (practice_session_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_feedback_evaluation
    ON practice_feedback (evaluation_id)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- warning
-- -----------------------------------------------------------------------------
CREATE INDEX idx_warning_session
    ON warning (practice_session_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_warning_learner_created
    ON warning (learner_id, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- roadmaps
-- -----------------------------------------------------------------------------
CREATE INDEX idx_roadmaps_learner_created
    ON roadmaps (learnerid, created_at DESC)
    ALGORITHM=INPLACE, LOCK=NONE;


-- -----------------------------------------------------------------------------
-- knowledge_resources
-- -----------------------------------------------------------------------------
ALTER TABLE knowledge_resources
    ADD FULLTEXT INDEX ft_knowledge_title_content (title, content);


-- -----------------------------------------------------------------------------
-- issue
-- -----------------------------------------------------------------------------
CREATE INDEX idx_issue_learner_status
    ON issue (learner_id, status)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_issue_assessment
    ON issue (assessment_id)
    ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_issue_practice
    ON issue (practice_session_id)
    ALGORITHM=INPLACE, LOCK=NONE;
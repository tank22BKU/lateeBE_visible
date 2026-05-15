CREATE TABLE users
(
    userid     VARCHAR(50) PRIMARY KEY,
    name       VARCHAR(100)        NOT NULL,
    email      VARCHAR(100) UNIQUE NOT NULL,
    phone      VARCHAR(20),
    birthday   DATE,
    -- ssn VARCHAR(20),
    password   VARCHAR(255)        NOT NULL,
    gender     VARCHAR(10),
    address    TEXT,
    role       ENUM('Learner', 'Expert', 'Admin') NOT NULL,
    status     ENUM('active', 'inactive') DEFAULT 'active',
    avatar_url VARCHAR(255),
    is_deleted BOOLEAN   DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE admin
(
    aid VARCHAR(50) PRIMARY KEY,
    ssn VARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT fk_admin_users FOREIGN KEY (aid) REFERENCES users (userid)
);

CREATE TABLE learner
(
    lid VARCHAR(50) PRIMARY KEY,
    ssn VARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT fk_learner_users FOREIGN KEY (lid) REFERENCES users (userid)
);

CREATE TABLE expert
(
    eid              VARCHAR(50) PRIMARY KEY,
    ssn              VARCHAR(20) NOT NULL UNIQUE,
    bio_quote        TEXT,
    education_detail TEXT,
    title_position   VARCHAR(255),
    expertise_skill  TEXT,
    social_link      VARCHAR(255),
    CONSTRAINT fk_expert_users FOREIGN KEY (eid) REFERENCES users (userid)
);

CREATE TABLE user_refresh_tokens
(
    token_id       VARCHAR(50) PRIMARY KEY,
    user_id        VARCHAR(50) NOT NULL,
    token_hash     CHAR(64)    NOT NULL UNIQUE,
    expires_at     TIMESTAMP   NOT NULL,
    created_at     TIMESTAMP            DEFAULT CURRENT_TIMESTAMP,
    created_by_ip  VARCHAR(50),
    user_agent     TEXT,
    is_revoked     BOOLEAN     NOT NULL DEFAULT FALSE,
    revoked_at     TIMESTAMP NULL,
    revoked_reason VARCHAR(100),
    CONSTRAINT fk_refresh_token_user FOREIGN KEY (user_id) REFERENCES users (userid) ON DELETE CASCADE
);

CREATE TABLE revoked_access_tokens
(
    jti        VARCHAR(64) PRIMARY KEY,
    user_id    VARCHAR(50),
    expires_at TIMESTAMP NOT NULL,
    revoked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    reason     VARCHAR(100)
);

CREATE TABLE evaluation_clinical_criteria
(
    id          VARCHAR(50) PRIMARY KEY,
    description TEXT,
    version     VARCHAR(20),
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
CREATE TABLE expert_criteria_management
(
    expert_id   VARCHAR(50) NOT NULL,
    criteria_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, criteria_id),
    CONSTRAINT fk_expert_criteria_expert FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_criteria_criteria FOREIGN KEY (criteria_id) REFERENCES evaluation_clinical_criteria (id)
);

CREATE TABLE clinical_case
(
    case_id        VARCHAR(50) PRIMARY KEY,
    title          TEXT NOT NULL,
    description    TEXT,
    type           TEXT,
    status         VARCHAR(50),
    pe             TEXT,
    symptom        TEXT,
    medicalhistory TEXT,
    created_by     VARCHAR(50)  NOT NULL,
    eccid          VARCHAR(50)  NOT NULL,
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_clinical_case_expert FOREIGN KEY (created_by) REFERENCES expert (eid),
    CONSTRAINT fk_clinical_case_evaluation FOREIGN KEY (eccid) REFERENCES evaluation_clinical_criteria (id)
);

CREATE TABLE virtual_patient
(
    patient_id   VARCHAR(50) PRIMARY KEY,
    case_id      VARCHAR(50)  NOT NULL,
    name         VARCHAR(100) NOT NULL,
    age          INT,
    gender       VARCHAR(10),
    pronouns     VARCHAR(50),
    occupation   VARCHAR(255),
    ethnicity    VARCHAR(100),
    persona      TEXT,
    chief_concern VARCHAR(255),
    vital_signs  TEXT,
    instructions TEXT,
    behaviors    TEXT,
    learning_objectives TEXT,
    time_setting INT,
    argument_time INT,
    level        ENUM('Beginner', 'Intermediate', 'Advanced', 'Expert') DEFAULT 'Intermediate',
    case_rule    TEXT,
    status       ENUM('active', 'inactive') DEFAULT 'active',
    avatar_image VARCHAR(255),
    created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_virtual_patient_clinical_case FOREIGN KEY (case_id) REFERENCES clinical_case (case_id)
);

CREATE TABLE expert_virtual_patient_management
(
    expert_id  VARCHAR(50) NOT NULL,
    virtual_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, virtual_id),
    CONSTRAINT fk_expert_virtual_expert FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_virtual_patient FOREIGN KEY (virtual_id) REFERENCES virtual_patient (patient_id)
);

CREATE TABLE expert_clinical_case_management
(
    expert_id VARCHAR(50) NOT NULL,
    case_id   VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, case_id),
    CONSTRAINT fk_expert_case_expert FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_case_clinical FOREIGN KEY (case_id) REFERENCES clinical_case (case_id)
);

CREATE TABLE notification
(
    id          VARCHAR(50) PRIMARY KEY,
    sender      VARCHAR(50) NOT NULL,
    receiver    VARCHAR(50) NOT NULL,
    description TEXT        NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status      ENUM('unread', 'read') DEFAULT 'unread',
    CONSTRAINT fk_notification_sender FOREIGN KEY (sender) REFERENCES users (userid),
    CONSTRAINT fk_notification_receiver FOREIGN KEY (receiver) REFERENCES users (userid)
);

CREATE TABLE system_feedback
(
    id          VARCHAR(50) PRIMARY KEY,
    learner_id  VARCHAR(50) NOT NULL,
    description TEXT        NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_system_feedback_learner FOREIGN KEY (learner_id) REFERENCES learner (lid)
);

CREATE TABLE expert_system_feedback
(
    expert_id          VARCHAR(50) NOT NULL,
    system_feedback_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, system_feedback_id),
    CONSTRAINT fk_expert_system_feedback_expert FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_system_feedback_feedback FOREIGN KEY (system_feedback_id) REFERENCES system_feedback (id)
);

CREATE TABLE labtestitem
(
    itemid   INT PRIMARY KEY,
    label    TEXT,
    fluid    VARCHAR(50),
    category ENUM('Blood Gas', 'Chemistry', 'Hematology'),
    count    DECIMAL(12, 0)
);

CREATE TABLE laboratorytest
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    clinicalcase_id VARCHAR(50) NOT NULL,
    itemid          INT         NOT NULL,
    value           TEXT        NOT NULL,
    rangelower      VARCHAR(50),
    rangeupper      VARCHAR(50),
    CONSTRAINT fk_lab_case FOREIGN KEY (clinicalcase_id) REFERENCES clinical_case (case_id),
    CONSTRAINT fk_lab_item FOREIGN KEY (itemid) REFERENCES labtestitem (itemid)
);

CREATE TABLE expert_laboratory
(
    expert_id  VARCHAR(50) NOT NULL,
    labtest_id INT         NOT NULL,
    PRIMARY KEY (expert_id, labtest_id),
    CONSTRAINT fk_expert_item FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_lab FOREIGN KEY (labtest_id) REFERENCES laboratorytest (id)
);

CREATE TABLE radiologyreport
(
    id              INT AUTO_INCREMENT PRIMARY KEY,
    clinicalcase_id VARCHAR(20) NOT NULL,
    noteid          VARCHAR(20),
    modality        ENUM('CT','Ultrasound','Radiograph','Drainage','MRI','MRCP','ERCP'),
    region          VARCHAR(50),
    examname        TEXT,
    text            TEXT,
    CONSTRAINT fk_radio_case FOREIGN KEY (clinicalcase_id) REFERENCES clinical_case (case_id)
);

CREATE TABLE expert_radiology
(
    expert_id           VARCHAR(50) NOT NULL,
    radiology_report_id INT NOT NULL,
    PRIMARY KEY (expert_id, radiology_report_id),
    CONSTRAINT fk_expert_radio FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_radio_report FOREIGN KEY (radiology_report_id) REFERENCES radiologyreport (id)
);

CREATE TABLE knowledge_resources
(
    id         VARCHAR(50) PRIMARY KEY,
    title      VARCHAR(255) NOT NULL,
    content    TEXT,
    link       TEXT,
    imageUrl   TEXT,
    authorlist TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE expert_knowledge
(
    expert_id             VARCHAR(50) NOT NULL,
    knowledge_resource_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, knowledge_resource_id),
    CONSTRAINT fk_expert_resource FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_resource_knowledge FOREIGN KEY (knowledge_resource_id) REFERENCES knowledge_resources (id)
);

CREATE TABLE guideline
(
    id          VARCHAR(50) PRIMARY KEY,
    description TEXT NOT NULL,
    version     VARCHAR(20),
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE expert_guideline_management
(
    expert_id    VARCHAR(50) NOT NULL,
    guideline_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, guideline_id),
    CONSTRAINT fk_expert_guideline_expert FOREIGN KEY (expert_id) REFERENCES expert (eid),
    CONSTRAINT fk_expert_guideline_guideline FOREIGN KEY (guideline_id) REFERENCES guideline (id)
);

CREATE TABLE practice_sessions
(
    id                  VARCHAR(50) PRIMARY KEY,
    learner_id          VARCHAR(50) NOT NULL,
    patient_id          VARCHAR(50) NOT NULL,
    final_diagnosis     TEXT,
    ai_reasoning_log    JSON,
    vp_conversation_log JSON,
    module_id           VARCHAR(50) DEFAULT 'EPA_STANDARD_V1',
    discussion_type     VARCHAR(50) DEFAULT 'Message Type',
    guidelines_id       VARCHAR(50),
    start_time          TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
    end_time            TIMESTAMP NULL,
    status              ENUM('Practicing', 'VpCompleted', 'ReasoningStarted', 'Completed', 'Abandoned') DEFAULT 'Practicing',
    created_at          TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_practice_learner FOREIGN KEY (learner_id) REFERENCES users (userid),
    CONSTRAINT fk_practice_to_patient FOREIGN KEY (patient_id) REFERENCES virtual_patient (patient_id) ON DELETE CASCADE,
    CONSTRAINT fk_practice_to_guidelines FOREIGN KEY (guidelines_id) REFERENCES guideline (id) ON DELETE SET NULL
);

CREATE TABLE evaluation
(
    id                  VARCHAR(50) PRIMARY KEY,
    epa_id              VARCHAR(20) NOT NULL,
    practice_session_id VARCHAR(50) NOT NULL,
    score               DECIMAL(5, 2),
    duration            INT,
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    feedback_detail     TEXT,
    entrustment_level   INT,
    rubric_version VARCHAR(20),
    CONSTRAINT fk_eval_practice FOREIGN KEY (practice_session_id) REFERENCES practice_sessions (id) ON DELETE CASCADE
);


CREATE TABLE evaluation_epa_score (
    id                VARCHAR(50)   PRIMARY KEY,
    evaluation_id     VARCHAR(50)   NOT NULL,           
    epa_id            VARCHAR(20)   NOT NULL,           -- EPA_1 => EPA_5
    numerical_score   TINYINT       NOT NULL,           -- 0–20
    entrustment_level TINYINT       NOT NULL,           -- 1–5
    feedback_detail   TEXT,                             
    evidence_cited    JSON,                             
    failure_patterns  JSON,                             
    safety_flags      JSON,                             
    created_at        DATETIME      DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_evaluation_id (evaluation_id),
    INDEX idx_epa_id (epa_id),
    INDEX idx_score (numerical_score)
);


CREATE TABLE roadmaps
(
    id         VARCHAR(50) PRIMARY KEY,
    learnerid  VARCHAR(50) NOT NULL,
    content    TEXT,
    version    VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_roadmap_learner FOREIGN KEY (learnerid) REFERENCES users (userid) ON DELETE CASCADE
);

CREATE TABLE summarize_roadmap
(
    roadmap_id    VARCHAR(50) NOT NULL,
    evaluation_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (roadmap_id, evaluation_id),
    CONSTRAINT fk_roadmap_summarize FOREIGN KEY (roadmap_id) REFERENCES roadmaps (id) ON DELETE CASCADE,
    CONSTRAINT fk_roadmap_eval FOREIGN KEY (evaluation_id) REFERENCES evaluation (id) ON DELETE CASCADE
);

CREATE TABLE practice_feedback
(
    id                  VARCHAR(50) PRIMARY KEY,
    overall_attempt     TEXT,
    overall_label       TEXT,
    strength            TEXT,
    improvement         TEXT,
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    evaluation_id       VARCHAR(50) NOT NULL,
    practice_session_id VARCHAR(50) NOT NULL,
    CONSTRAINT fk_feedback_practice FOREIGN KEY (practice_session_id) REFERENCES practice_sessions (id) ON DELETE CASCADE,
    CONSTRAINT fk_feedback_eval FOREIGN KEY (evaluation_id) REFERENCES evaluation (id) ON DELETE CASCADE
);

CREATE TABLE warning
(
    id                  VARCHAR(50) PRIMARY KEY,
    practice_session_id VARCHAR(50) NOT NULL,
    learner_id          VARCHAR(50) NOT NULL,
    label               VARCHAR(100),
    description         TEXT,
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_eval_warning FOREIGN KEY (practice_session_id) REFERENCES practice_sessions (id) ON DELETE CASCADE,
    CONSTRAINT fk_warning_learner FOREIGN KEY (learner_id) REFERENCES users (userid) ON DELETE CASCADE
);


CREATE TABLE assessments
(
    assessment_id            VARCHAR(50) PRIMARY KEY,
    module_id                VARCHAR(50),
    specialty                VARCHAR(100),
    topic                    VARCHAR(100) NOT NULL,
    subtopic                 VARCHAR(100),
    difficulty_level         ENUM('Beginner', 'Intermediate', 'Advanced', 'Expert') DEFAULT 'Intermediate',
    title                    TEXT         NOT NULL,
    descriptions             TEXT,
    goal                     TEXT,
    num_questions            INT           DEFAULT 10,
    time_limit_minutes       INT,
    passing_score_percentage DECIMAL(5, 2) DEFAULT 80.00,
    max_attempts             INT           DEFAULT 1,
    allowed_question_types   JSON,
    is_active                BOOLEAN       DEFAULT TRUE,
    created_at               TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
    updated_at               TIMESTAMP     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);


CREATE TABLE assessment_session
(
    session_id         VARCHAR(50) NOT NULL PRIMARY KEY,
    assessment_id      VARCHAR(50) NOT NULL,
    overall_score       DECIMAL(5, 2) DEFAULT 0.00,
    learner_id          VARCHAR(50) NOT NULL,
    attempt_no          INT           DEFAULT 1,
    duration            INT,
    start_time          TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
    end_time            TIMESTAMP,
    status              ENUM('InProgress', 'Completed', 'Abandoned') DEFAULT 'InProgress',
    is_passed           BOOLEAN,
    CONSTRAINT fk_session_learner FOREIGN KEY (learner_id) REFERENCES users (userid) ON DELETE CASCADE,
    CONSTRAINT fk_session_assessment FOREIGN KEY (assessment_id) REFERENCES assessments (assessment_id) ON DELETE CASCADE
);

CREATE TABLE question
(
    id              VARCHAR(50) NOT NULL PRIMARY KEY,
    assessment_id   VARCHAR(50) NOT NULL,
    question        TEXT        NOT NULL,
    question_option JSON,
    question_type   ENUM('MultipleChoice', 'MultipleResponse', 'TrueFalse', 'FillInBlank', 'ShortAnswer') NOT NULL,
    cognitive_level ENUM('Remember', 'Understand', 'Apply', 'Analyze', 'Evaluate', 'Create'),
    explanation     TEXT,
    points          DECIMAL(5, 2) DEFAULT 1.00,
    created_at      TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
    updated_at      TIMESTAMP     DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_question_assessment FOREIGN KEY (assessment_id) REFERENCES assessments (assessment_id) ON DELETE CASCADE
);

CREATE TABLE assessment_answer
(
    id            VARCHAR(50) PRIMARY KEY,
    session_id    VARCHAR(50) NOT NULL,
    question_id   VARCHAR(50) NOT NULL,
    user_choice   JSON,
    is_correct    BOOLEAN       DEFAULT FALSE,
    points_earned DECIMAL(5, 2) DEFAULT 0.00,
    is_flagged    BOOLEAN       DEFAULT FALSE,
    created_at    TIMESTAMP     DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ans_session FOREIGN KEY (session_id) REFERENCES assessment_session (session_id) ON DELETE CASCADE,
    CONSTRAINT fk_ans_question FOREIGN KEY (question_id) REFERENCES question (id) ON DELETE CASCADE
);

CREATE TABLE issue
(
    id                  VARCHAR(50) PRIMARY KEY,
    assessment_id       VARCHAR(50),
    practice_session_id VARCHAR(50),
    learner_id          VARCHAR(50) NOT NULL,
    ItemType            ENUM('Assessment', 'Practice') NOT NULL,
    is_deleted          BOOLEAN   DEFAULT false,
    editDeadline        INT         NOT NULL,
    description         TEXT        NOT NULL,
    label               VARCHAR(100),
    status              ENUM('Open', 'InReview', 'Resolved', 'Rejected') DEFAULT 'Open',
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_issue_assessment FOREIGN KEY (assessment_id) REFERENCES assessments (assessment_id) ON DELETE CASCADE,
    CONSTRAINT fk_issue_practice FOREIGN KEY (practice_session_id) REFERENCES practice_sessions (id) ON DELETE CASCADE,
    CONSTRAINT fk_issue_learner FOREIGN KEY (learner_id) REFERENCES users (userid) ON DELETE CASCADE
);

CREATE TABLE resolved_issue
(
    issue_id  VARCHAR(50) NOT NULL,
    expert_id VARCHAR(50) NOT NULL,
    feedback  TEXT,
    PRIMARY KEY (issue_id, expert_id),
    CONSTRAINT fk_issue_resolved FOREIGN KEY (issue_id) REFERENCES issue (id) ON DELETE CASCADE,
    CONSTRAINT fk_issue_expert FOREIGN KEY (expert_id) REFERENCES expert (eid ) ON DELETE CASCADE
);


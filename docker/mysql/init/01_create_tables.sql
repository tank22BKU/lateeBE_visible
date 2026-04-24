CREATE TABLE users (
    user_id VARCHAR(50) PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'User',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE user_refresh_tokens (
    token_id VARCHAR(50) PRIMARY KEY,
    user_id VARCHAR(50) NOT NULL,
    token_hash CHAR(64) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by_ip VARCHAR(50),
    user_agent TEXT,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at TIMESTAMP NULL,
    revoked_reason VARCHAR(100),
    CONSTRAINT fk_refresh_token_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

CREATE TABLE revoked_access_tokens (
    jti VARCHAR(64) PRIMARY KEY,
    user_id VARCHAR(50),
    expires_at TIMESTAMP NOT NULL,
    revoked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    reason VARCHAR(100)
);

CREATE TABLE patients (
    patientid VARCHAR(50) PRIMARY KEY,
    clinical_case_id VARCHAR(50), 
    name VARCHAR(100) NOT NULL,
    age INT,
    gender VARCHAR(20),
    pronouns VARCHAR(20),
    ethnicity VARCHAR(50),
    occupation VARCHAR(100),
    setting VARCHAR(50),
    level VARCHAR(20),
    time_setting VARCHAR(50),
    avatar_img TEXT,
    descriptions TEXT,
    chief_concern TEXT,
    vital_signs JSON, 
    instructions JSON,
    case_rules JSON,
    persona JSON,
    status VARCHAR(20) DEFAULT 'active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE clinicalcases (
    clinicalcaseid VARCHAR(20) PRIMARY KEY,
    patientid VARCHAR(50) NOT NULL,
    title TEXT,
    type VARCHAR(50),
    descriptions TEXT,
    symptom TEXT,
    medicalhistory TEXT,
    pe TEXT,
    status VARCHAR(10) DEFAULT 'active',
    createdBy VARCHAR(50),
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_patient FOREIGN KEY (patientid) REFERENCES patients(patientid)
);

CREATE TABLE labtestitem (
    itemid INT PRIMARY KEY,
    label TEXT,
    fluid VARCHAR(20),
    category ENUM('Blood Gas', 'Chemistry', 'Hematology'),
    count DECIMAL(12,0)
);

CREATE TABLE laboratorytest (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clinicalcaseid VARCHAR(20) NOT NULL,
    itemid INT NOT NULL,
    value TEXT NOT NULL,
    rangelower VARCHAR(20),
    rangeupper VARCHAR(20),
    CONSTRAINT fk_lab_case FOREIGN KEY (clinicalcaseid) REFERENCES clinicalcases(clinicalcaseid),
    CONSTRAINT fk_lab_item FOREIGN KEY (itemid) REFERENCES labtestitem(itemid)
);

CREATE TABLE radiologyreport (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clinicalcaseid VARCHAR(20) NOT NULL,
    noteid VARCHAR(20),
    modality ENUM('CT','Ultrasound','Radiograph','Drainage','MRI','MRCP','ERCP'),
    region VARCHAR(50),
    examname TEXT,
    text TEXT,
    CONSTRAINT fk_radio_case FOREIGN KEY (clinicalcaseid) REFERENCES clinicalcases(clinicalcaseid)
);

CREATE TABLE assessments (
    assessment_id VARCHAR(50) PRIMARY KEY,
    creator_id VARCHAR(50) NOT NULL,
    clinical_case_id VARCHAR(20),    
    course_id VARCHAR(50),
    module_id VARCHAR(50),
    specialty VARCHAR(100),          
    topic VARCHAR(100) NOT NULL,
    subtopic VARCHAR(100),
    difficulty_level ENUM('Beginner', 'Intermediate', 'Advanced', 'Expert') DEFAULT 'Intermediate',
    title TEXT NOT NULL,
    descriptions TEXT,
    goal TEXT,                       
    num_questions INT DEFAULT 10,
    time_limit_minutes INT,          
    passing_score_percentage DECIMAL(5,2) DEFAULT 80.00,
    max_attempts INT DEFAULT 1,     
    generation_prompt TEXT,
    allowed_question_types JSON,     
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE assessment_questions (
    question_id VARCHAR(50) PRIMARY KEY,
    assessment_id VARCHAR(50) NOT NULL,
    question_type ENUM('MultipleChoice', 'MultipleResponse', 'TrueFalse', 'FillInBlank', 'ShortAnswer') NOT NULL,
    cognitive_level ENUM('Remember', 'Understand', 'Apply', 'Analyze', 'Evaluate', 'Create'),
    content TEXT NOT NULL,           
    options JSON,                    
    explanation TEXT,                
    points DECIMAL(5,2) DEFAULT 1.00,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_aq_assessment FOREIGN KEY (assessment_id) REFERENCES assessments(assessment_id) ON DELETE CASCADE
);

CREATE TABLE assessment_attempts (
    attempt_id VARCHAR(50) PRIMARY KEY,
    assessment_id VARCHAR(50) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    start_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time TIMESTAMP NULL,
    score DECIMAL(5,2),
    is_passed BOOLEAN,
    status ENUM('InProgress', 'Completed', 'Abandoned') DEFAULT 'InProgress',
    CONSTRAINT fk_attempt_assessment FOREIGN KEY (assessment_id) REFERENCES assessments(assessment_id) ON DELETE CASCADE
);

CREATE TABLE attempt_answers (
    answer_id VARCHAR(50) PRIMARY KEY,
    attempt_id VARCHAR(50) NOT NULL,
    question_id VARCHAR(50) NOT NULL,
    user_choice JSON,             
    is_correct BOOLEAN,
    points_earned DECIMAL(5,2) DEFAULT 0.00,
    is_flagged BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ans_attempt FOREIGN KEY (attempt_id) REFERENCES assessment_attempts(attempt_id) ON DELETE CASCADE,
    CONSTRAINT fk_ans_question FOREIGN KEY (question_id) REFERENCES assessment_questions(question_id)
);

CREATE TABLE assessment_issues (
    issue_id VARCHAR(50) PRIMARY KEY,
    question_id VARCHAR(50) NOT NULL,
    reporter_id VARCHAR(50) NOT NULL,
    label VARCHAR(100),             
    descriptions TEXT NOT NULL,
    feedback TEXT,                  
    status ENUM('Open', 'InReview', 'Resolved', 'Rejected') DEFAULT 'Open',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_issue_question FOREIGN KEY (question_id) REFERENCES assessment_questions(question_id) ON DELETE CASCADE
);

-- Evaluation related tables
CREATE TABLE evaluation_results
(
    result_id           VARCHAR(50) PRIMARY KEY,
    user_id             VARCHAR(50) NOT NULL,
    clinical_case_id    VARCHAR(50) NOT NULL,
    module_id           VARCHAR(50) DEFAULT 'EPA_STANDARD_V1',
    vp_conversation_log JSON,
    ai_reasoning_log    JSON,
    final_diagnosis     TEXT,
    overall_score       DECIMAL(5, 2),
    created_at          TIMESTAMP   DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE evaluation_warnings
(
    warning_id  VARCHAR(50) PRIMARY KEY,
    result_id   VARCHAR(50) NOT NULL,
    label       VARCHAR(100),
    description TEXT,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_eval_warning FOREIGN KEY (result_id) REFERENCES evaluation_results (result_id) ON DELETE CASCADE
);

CREATE TABLE epa_scores
(
    score_id          VARCHAR(50) PRIMARY KEY,
    result_id         VARCHAR(50) NOT NULL,
    epa_id            VARCHAR(20) NOT NULL,
    entrustment_level INT,
    numerical_score   DECIMAL(5, 2),
    feedback_detail   TEXT,
    CONSTRAINT fk_eval_epa FOREIGN KEY (result_id) REFERENCES evaluation_results (result_id) ON DELETE CASCADE
);
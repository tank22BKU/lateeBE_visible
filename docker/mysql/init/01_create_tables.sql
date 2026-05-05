CREATE TABLE users (
    userid VARCHAR(50) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(20),
    birthday DATE,
    -- ssn VARCHAR(20),
    password VARCHAR(255) NOT NULL,
    gender VARCHAR(10),
    address TEXT,
    role ENUM('Learner', 'Expert', 'Admin') NOT NULL,
    status ENUM('active', 'inactive') DEFAULT 'active',
    avatar_url VARCHAR(255),
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE admin (
    aid VARCHAR(50) PRIMARY KEY,
    ssn VARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT fk_admin_users FOREIGN KEY (aid) REFERENCES users(userid)
);

CREATE TABLE learner (
    lid VARCHAR(50) PRIMARY KEY,
    ssn VARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT fk_learner_users FOREIGN KEY (lid) REFERENCES users(userid)
);

CREATE TABLE expert (
    eid VARCHAR(50) PRIMARY KEY,
    ssn VARCHAR(20) NOT NULL UNIQUE,
    bio_quote TEXT,
    education_detail TEXT,
    title_position VARCHAR(255),
    expertise_skill TEXT,
    social_link VARCHAR(255),
    CONSTRAINT fk_expert_users FOREIGN KEY (eid) REFERENCES users(userid)
);

CREATE TABLE guideline (
    id VARCHAR(50) PRIMARY KEY, 
    description TEXT NOT NULL,
    version VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE expert_guideline_management (
    expert_id VARCHAR(50) NOT NULL,
    guideline_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, guideline_id),
    CONSTRAINT fk_expert_guideline_expert FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_guideline_guideline FOREIGN KEY (guideline_id) REFERENCES guideline(id)
);

CREATE TABLE virtual_patient (
    patient_id VARCHAR(50) PRIMARY KEY,
    case_id VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    age INT,
    gender VARCHAR(10),
    pronouns VARCHAR(50),
    occupation VARCHAR(255),
    ethnicity VARCHAR(100),
    persona TEXT,
    vital_signs TEXT,
    instructions TEXT,
    behaviors TEXT,
    time_setting INT,
    level VARCHAR(50),
    avatar_image VARCHAR(255),
    case_rule TEXT,
    status ENUM('active', 'inactive') DEFAULT 'active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_virtual_patient_clinical_case FOREIGN KEY (case_id) REFERENCES clinical_case(case_id)
);

CREATE TABLE expert_virtual_patient_management (
    expert_id VARCHAR(50) NOT NULL,
    virtual_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, virtual_id),
    CONSTRAINT fk_expert_virtual_expert FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_virtual_patient FOREIGN KEY (virtual_id) REFERENCES virtual_patient(patient_id)
);

CREATE TABLE evaluation_clinical_criteria (
    id VARCHAR(50) PRIMARY KEY,
    description TEXT,
    version VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
CREATE TABLE expert_criteria_management (
    expert_id VARCHAR(50) NOT NULL,
    criteria_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, criteria_id),
    CONSTRAINT fk_expert_criteria_expert FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_criteria_criteria FOREIGN KEY (criteria_id) REFERENCES evaluation_clinical_criteria(id)
);

CREATE TABLE clinical_case (
    case_id VARCHAR(50) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    type VARCHAR(50),
    status VARCHAR(50),
    pe         TEXT,
    symptom TEXT,
    medicalhistory TEXT,
    created_by VARCHAR(50) NOT NULL,
    eccid VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_clinical_case_expert FOREIGN KEY (created_by) REFERENCES expert(eid),
    CONSTRAINT fk_clinical_case_evaluation FOREIGN KEY (eccid) REFERENCES evaluation_clinical_criteria(id)
);

CREATE TABLE expert_clinical_case_management (
    expert_id VARCHAR(50) NOT NULL,
    case_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, case_id),
    CONSTRAINT fk_expert_case_expert FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_case_clinical FOREIGN KEY (case_id) REFERENCES clinical_case(case_id)
);

CREATE TABLE notification (
    id VARCHAR(50) PRIMARY KEY,
    sender VARCHAR(50) NOT NULL,
    receiver VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status ENUM('unread', 'read') DEFAULT 'unread',
    CONSTRAINT fk_notification_sender FOREIGN KEY (sender) REFERENCES users(userid),
    CONSTRAINT fk_notification_receiver FOREIGN KEY (receiver) REFERENCES users(userid)
);

CREATE TABLE system_feedback (
    id VARCHAR(50) PRIMARY KEY,
    learner_id VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_system_feedback_learner FOREIGN KEY (learner_id) REFERENCES learner(lid)
);

CREATE TABLE expert_system_feedback (
    expert_id VARCHAR(50) NOT NULL,
    system_feedback_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (expert_id, system_feedback_id),
    CONSTRAINT fk_expert_system_feedback_expert FOREIGN KEY (expert_id) REFERENCES expert(eid),
    CONSTRAINT fk_expert_system_feedback_feedback FOREIGN KEY (system_feedback_id) REFERENCES system_feedback(id)
);


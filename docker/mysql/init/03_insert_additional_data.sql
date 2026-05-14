ALTER TABLE practice_sessions
    ADD COLUMN has_practice_feedback  BOOLEAN DEFAULT FALSE 
        COMMENT 'True khi đã gen practice_feedback ít nhất 1 lần',
    ADD COLUMN feedback_generated_at  TIMESTAMP NULL 
        COMMENT 'Thời điểm gen practice_feedback lần đầu',
    ADD COLUMN final_score            INT NULL 
        COMMENT 'Điểm tổng sau modifier (0-110)',
    ADD COLUMN final_entrustment_level INT NULL 
        COMMENT 'Entrustment Level tổng hợp (1-5)';

INSERT INTO guideline (id, description, version)
VALUES
(
    'GL-001',
    'Management guideline for acute ischemic stroke including thrombolysis eligibility, blood pressure control, and post-stroke monitoring.',
    'v1.0'
);

INSERT INTO guideline (id, description, version)
VALUES
(
    'GL-002',
    'Clinical guideline for acute pancreatitis focusing on fluid resuscitation, pain management, nutritional support, and complication monitoring.',
    'v1.2'
);

INSERT INTO practice_sessions (id, learner_id, patient_id, final_diagnosis, ai_reasoning_log, vp_conversation_log, module_id, discussion_type, guidelines_id, start_time, end_time, status, created_at)
VALUES
('PS-005', 'USR-LRN-01', '10070247', 'Appendicitis', '{"steps":["history","exam","diagnosis"]}', '{"dialogue":"..."}', 'EPA_STANDARD_V1', 'Message Type', 'GL-001', '2026-05-01 09:00:00', '2026-05-01 09:30:00', 'Completed', '2026-05-01 09:00:00'),
('PS-006', 'USR-LRN-01', '10073256', 'Cholecystitis', '{"steps":["history","exam","diagnosis"]}', '{"dialogue":"..."}', 'EPA_STANDARD_V1', 'Message Type', 'GL-002', '2026-05-02 10:00:00', '2026-05-02 10:40:00', 'Completed', '2026-05-02 10:00:00');



INSERT INTO evaluation (id, epa_id, practice_session_id, score, duration, created_at, feedback_detail, entrustment_level)
VALUES
('EVAL-001', 'EPA-001', 'PS-005', 85.5, 30, '2026-05-01 09:31:00', 'Good clinical reasoning and communication.', 4),
('EVAL-002', 'EPA-002', 'PS-006', 78.0, 40, '2026-05-02 10:41:00', 'Solid diagnosis, needs improvement in history taking.', 3);


INSERT INTO assessments (assessment_id, module_id, specialty, topic, subtopic, difficulty_level, title, descriptions, goal, num_questions, time_limit_minutes, passing_score_percentage, max_attempts, allowed_question_types, is_active)
VALUES
('ASM-001', 'EPA_STANDARD_V1', 'General Surgery', 'Appendicitis', 'Acute Abdomen', 'Intermediate', 'Assessment on Appendicitis', 'Test knowledge on appendicitis.', 'Evaluate diagnosis and management.', 2, 30, 80.00, 3, '["MultipleChoice","ShortAnswer"]', TRUE),
('ASM-002', 'EPA_STANDARD_V1', 'General Surgery', 'Cholecystitis', 'Gallbladder', 'Intermediate', 'Assessment on Cholecystitis', 'Test knowledge on cholecystitis.', 'Evaluate diagnosis and management.', 2, 30, 80.00, 3, '["MultipleChoice","ShortAnswer"]', TRUE);


INSERT INTO assessment_session (session_id, assessment_id, overall_score, learner_id, attempt_no, duration, start_time, end_time, status, is_passed)
VALUES
('ASMT-SES-001', 'ASM-001', 85.5, 'USR-LRN-01', 1, 30, '2026-05-01 09:00:00', '2026-05-01 09:30:00', 'Completed', TRUE),
('ASMT-SES-002', 'ASM-002', 78.0, 'USR-LRN-01', 1, 40, '2026-05-02 10:00:00', '2026-05-02 10:40:00', 'Completed', TRUE);


INSERT INTO question (id, assessment_id, question, question_option, question_type, cognitive_level, explanation, points)
VALUES
('Q-001', 'ASM-001', 'What is the most common symptom of appendicitis?', '{"options":["Fever","Right lower quadrant pain","Jaundice","Hematuria"]}', 'MultipleChoice', 'Remember', 'Right lower quadrant pain is classic.', 1.00),
('Q-002', 'ASM-001', 'Describe the typical presentation of acute appendicitis.', NULL, 'ShortAnswer', 'Understand', 'Pain migrates to RLQ.', 2.00),
('Q-003', 'ASM-002', 'Which imaging is best for cholecystitis diagnosis?', '{"options":["CT Scan","Ultrasound","X-ray","MRI"]}', 'MultipleChoice', 'Apply', 'Ultrasound is first-line.', 1.00),
('Q-004', 'ASM-002', 'List two complications of cholecystitis.', NULL, 'ShortAnswer', 'Analyze', 'Perforation, abscess.', 2.00);


INSERT INTO assessment_answer (id, session_id, question_id, user_choice, is_correct, points_earned, is_flagged)
VALUES
('ANS-001', 'ASMT-SES-001', 'Q-001', '{"selected":"Right lower quadrant pain"}', TRUE, 1.00, FALSE),
('ANS-002', 'ASMT-SES-001', 'Q-002', '{"answer":"Pain migrates to RLQ."}', TRUE, 2.00, FALSE),
('ANS-003', 'ASMT-SES-002', 'Q-003', '{"selected":"Ultrasound"}', TRUE, 1.00, FALSE),
('ANS-004', 'ASMT-SES-002', 'Q-004', '{"answer":"Perforation, abscess."}', TRUE, 2.00, FALSE);




CREATE INDEX idx_vp_level   ON virtual_patient(level);
CREATE INDEX idx_vp_gender  ON virtual_patient(gender);
CREATE INDEX idx_vp_status  ON virtual_patient(status);
CREATE INDEX idx_vp_created ON virtual_patient(created_at);

CREATE INDEX idx_ps_learner_patient ON practice_sessions(learner_id, patient_id);
CREATE INDEX idx_ps_status          ON practice_sessions(status);

CREATE INDEX idx_eval_session ON evaluation(practice_session_id);

CREATE INDEX idx_pf_session ON practice_feedback(practice_session_id);

CREATE INDEX idx_vp_owner ON virtual_patient(owner_expert_id);
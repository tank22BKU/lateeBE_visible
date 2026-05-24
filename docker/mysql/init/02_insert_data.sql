-- User
INSERT INTO users (userid, name, email, phone, birthday, password, gender, address, role, avatar_url)
VALUES ('USR-EXP-001', 'Tachibana Hana', 'hana.tachibana@latee.com', '(568) 333-111-222', '1990-05-15', 'expert1',
        'Female', 'Shibuya, Tokyo, Japan', 'Expert', '/images/doctorFEMALE.jpeg'),
       ('USR-EXP-002', 'Andrew Nguyen', 'andrew.nguyen@latee.com', '(568) 367-987-237', '1982-11-20', 'expert2', 'Male',
        'Hudson, Wisconsin (WI), 54016', 'Expert', '/images/d22.jpg');

INSERT INTO users (userid, name, email, phone, birthday, password, gender, address, role, status)
VALUES ('USR-ADM-01', 'Nguyen Quan Tri', 'admin.system@latee.com', '0901234567', '1985-01-01', 'admin1', 'Male',
        'Hanoi, Vietnam', 'Admin', 'active'),
       ('USR-LRN-01', 'Tran Van Hoc', 'hoc.tran@student.com', '0912345678', '2002-09-20', 'learner1', 'Male',
        'Ho Chi Minh City, Vietnam', 'Learner', 'active');

INSERT INTO users
(userid, name, email, phone, birthday, password, gender, address, role, avatar_url)
VALUES ('USR-EXP-003','Emily Carter','emily.carter@latee.com','0908111222','1987-04-18','expert3','Female','Boston, USA','Expert','/images/d23.jpg'),

       ('USR-EXP-004','Le Minh Duc','duc.le@latee.com','0908333444','1985-08-22','expert4','Male','Ho Chi Minh City, Vietnam','Expert','/images/d24.jpg'),

       ('USR-LRN-002','Nguyen Thanh Long','long.nguyen@student.com','0911222333','2001-06-02','learner2','Male','Da Nang, Vietnam','Learner','/images/student1.jpg'),

       ('USR-LRN-003','Pham Bao Anh','baoanh@student.com','0911555666','2003-12-10','learner3','Female','Hanoi, Vietnam','Learner','/images/student2.jpg');

-- Admin
INSERT INTO admin (aid, ssn)
VALUES ('USR-ADM-01', '052204022947');

-- Learner
INSERT INTO learner (lid, ssn)
VALUES ('USR-LRN-01', '052204022949');

INSERT INTO learner (lid, ssn)
VALUES
    ('USR-LRN-002','052204022950'),
    ('USR-LRN-003','052204022951');

-- Expert
INSERT INTO expert (eid, ssn, bio_quote, education_detail, title_position, expertise_skill, social_link)
VALUES  ('USR-EXP-001',
        '052204022948',
        'Dr. Tachibana Hana focuses on hands-on clinical practice guidance for students. She is well known for her patient-centered teaching approach and her refined ability to convey healthcare communication skills effectively.',
        'Masters Degree in Clinical Nursing from Kyoto University. Internationally certified Clinical Simulation Training Specialist.',
        'Clinical Instructor',
        'Patient Interaction, Clinical Supervision, Medical Simulation Training',
        'https://linkedin.com/in/hanatachibana'),
        ('USR-EXP-002',
        '052204022949',
        'Dr. Andrew is a leading expert in analyzing complex clinical cases. With over 15 years of experience, he has developed modern diagnostic consulting models that help medical students shorten their learning curve when approaching real-world diseases.',
        'Doctor of Medicine (MD) in Internal Medicine from Johns Hopkins University. Advanced Medical Education Teaching Certification from Harvard Medical School.',
        'Specialist in Diagnostic Reasoning',
        'Clinical Reasoning, Diagnostic Strategy, Case-based Learning',
        'https://linkedin.com/in/andrewnguyen');

INSERT INTO expert
(eid,
 ssn,
 bio_quote,
 education_detail,
 title_position,
 expertise_skill,
 social_link)
VALUES ('USR-EXP-003',
        '052204022952',
        'Helping learners improve diagnostic confidence through evidence-based medicine.',
        'MD Internal Medicine - Stanford University',
        'Clinical Education Specialist',
        'Internal Medicine, Diagnostic Reasoning, Simulation',
        'https://linkedin.com/in/emilycarter'),

       ('USR-EXP-004',
        '052204022953',
        'Building practical medical thinking using virtual patient scenarios.',
        'PhD Medical Education - HCMUT Medical Faculty',
        'Senior Medical Educator',
        'Clinical Training, Medical Assessment, Case Authoring',
        'https://linkedin.com/in/leminhduc');

-- Evaluation Clinical Criteria
INSERT INTO evaluation_clinical_criteria (id, description, version)
VALUES  ('CRIT-001', 'Patient Communication Excellence', 'V1.0'),
        ('CRIT-002', 'Diagnostic Accuracy and Reasoning', 'V1.0'),
        ('CRIT-003', 'Clinical Simulation Performance', 'V2.1');

-- Expert Criteria Management
INSERT INTO expert_criteria_management (expert_id, criteria_id)
VALUES  ('USR-EXP-001', 'CRIT-001'),
        ('USR-EXP-001', 'CRIT-003'),
        ('USR-EXP-002', 'CRIT-002');

-- Guideline
INSERT INTO guideline (id, description, version, created_at, updated_at) 
VALUES (
    'GL01', 
    'Standard Clinical Guidance for Patient Communication and Reasoning.', 
    'v1.0', 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP
);

-- Expert Guideline Management
INSERT INTO expert_guideline_management (expert_id, guideline_id)
VALUES (
    'USR-EXP-001', 
    'GL01'
);

-- User Refresh Tokens
INSERT INTO user_refresh_tokens (token_id, user_id, token_hash, expires_at, created_by_ip, user_agent)
VALUES ('TK-001', 'USR-EXP-001', 'hash_string_alpha_123', '2024-12-31 23:59:59', '1.1.1.1',
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'),
       ('TK-002', 'USR-EXP-002', 'hash_string_beta_456', '2024-12-31 23:59:59', '8.8.8.8',
        'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)');

INSERT INTO user_refresh_tokens (token_id, user_id, token_hash, expires_at, created_by_ip, user_agent)
VALUES ('TKN-ADM-01', 'USR-ADM-01', 'abc123hash', '2026-06-01 10:00:00', '192.168.1.1', 'Chrome/Windows'),
       ('TKN-LRN-01', 'USR-LRN-01', 'xyz789hash', '2026-06-01 10:00:00', '113.161.0.1', 'Safari/iPhone');

-- Revoked Access Tokens
INSERT INTO revoked_access_tokens (jti, user_id, expires_at, reason)
VALUES ('JWT-OLD-999', 'USR-LRN-01', '2026-05-01 08:00:00', 'User logged out');



INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('27892518',
        'Appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with appendicitis',
        'APPENDICITIS',
        'active',
        'Admission Vitals: Temp: 98 HR: 112 Resp: 16 O2Sat: 97% General: No acute distress; alert and fully oriented Cardiac: Regular rate and rhythm; normal S1 and S2 Pulmonary: Lungs clear to auscultation bilaterally Abdomen: Soft, non-distended, acutely tender to palpation in the right lower quadrant; mild tenderness in the right upper quadrant; (+) rebound; (-) gaurding; (+) Rovsing/(+)Psoas signs Extremities: Warm and well-perfused',
        'I am a a while year old male with medical history for chronic lower back pain who now presents with complaint of right lower quadrant/flank abdominal pain since this morning. According to the patient, he was feeling well until yesterday afternoon, when he developed acute-onset of back pain. He does have a history of chronic lower back pain with multiple surgical interventions, however I feel that the quality of the pain during this episode was different than my usual pain. These symptoms continued through-out the night, and this morning he noted that the discomfort had localized to the right lower quadrant/flank instead. At this time he also began experiencing some mild feeling sick to my stomach, but no vomiting. Due to the unusual and non-resolving nature of my symptoms, he presented to the emergency room for further evaluation. I don''t have any fever or chills, has had normal bowel movements, and normal appetite (indeed, the I''ve been feeling feeling quite hungry). No recent sick contacts. No other associated systemic symptoms. Asthma, HT,neuropathy in bilateral legs and arm for multiple years, GERD, Recent weight loss> a while Non-contributory',
        'The patient is a ___ year old male with medical history for chronic lower back pain who now presents with complaint of right lower quadrant/flank abdominal pain since this morning. According to the patient, he was feeling well until yesterday afternoon, when he developed acute-onset of back pain. He does have a history of chronic lower back pain with multiple surgical interventions, however he states that the quality of the pain during this episode was different than his usual pain. These symptoms continued through-out the night, and this morning he noted that the discomfort had localized to the right lower quadrant/flank instead. At this time he also began experiencing some mild nausea, but no emesis. Due to the unusual and non-resolving nature of his symptoms, he presented to the ED for further evaluation. He denies any fever or chills, has had normal bowel movements, and normal appetite (indeed, the patient reports feeling quite hungry). No recent sick contacts. No other associated systemic symptoms. Past Medical History: Asthma, HT,neuropathy in bilateral legs and arm for multiple years, GERD, Recent weight loss> Social History: ___ Family History: Non-contributory',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('21807759',
        'acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'PHYSICAL EXAMINATION upon admission: ___ Temp:102 HR:80 BP:120j/80 Resp:20 O(2)Sat:92 on 5L low Constitutional: Comfortable HEENT: Extraocular muscles intact Chest: Clear to auscultation Cardiovascular: Normal first and second heart sounds Abdominal: There is some guarding on the right side of the abdomen and some mild right lower quadrant tenderness Extr/Back: No edema or calf tenderness was to Neuro: Speech fluent Psych: Normal mood',
        'a while with history of prostate cancer presents with a 2 day history of abdominal pain which began with a periumbilical burning sensation and radiated to my lower right side of my belly this AM. Patient denies fever, chills but reports some feeling sick to my stomach and dry heaving. He has had decreased appetite but denies diarrhea and hematochezia. He is passing flatus and my last bowel movement was yesterday afternoon. I have that my pain has improved after receiving morphine. medical history: Prostate Cancer a while NC',
        'HPI: ___ with history of prostate cancer presents with a 2 day history of abdominal pain which began with a periumbilical burning sensation and radiated to his RLQ this AM. Patient denies fever, chills but reports some nausea and dry heaving. He has had decreased appetite but denies diarrhea and hematochezia. He is passing flatus and his last bowel movement was yesterday afternoon. He reports that his pain has improved after receiving morphine. Past Medical History: PMH: Prostate Cancer Social History: ___ Family History: NC',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('20345216',
        'Acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'On admission: Vitals: 98.1 95 124/61 22 96% GEN: A&O, NAD HEENT: No scleral icterus, mucus membranes moist CV: RRR, No M/G/R PULM: Clear to auscultation b/l, No W/R/R ABD: Soft, very TTP RLQ, +obturator sign, -Rosving sign. No rebound, some voluntary gaurding Ext: No ___ edema, ___ warm and well perfused',
        'Mr. a while is an otherwise healthy a while man who presents with 10h history of abdominal pain. The pain began periumbilically and migrated to the right lower quadrant. He initially had one episode of diarrhea. Has not had any feeling sick to my stomach or vomiting. Denies fevers or chills. He has not wanted to eat since the pain began. seasonal and food allergies Past Surgical History: pilonidal cyst excision a while Non-contributory',
        'Mr. ___ is an otherwise healthy ___ man who presents with 10h history of abdominal pain. The pain began periumbilically and migrated to the right lower quadrant. He initially had one episode of diarrhea. Has not had any nausea or emesis. Denies fevers or chills. He has not wanted to eat since the pain began. Past Medical History: Past Medical History: seasonal and food allergies Past Surgical History: pilonidal cyst excision Social History: ___ Family History: Non-contributory',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('24078130',
        'Acute Appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'Physical Exam upon admission: Exam: 97.6, 72, 120/64, 16, 99RA no acute distress, obese young male clear to auscultation bilaterally regular rate and rhythm abdomen soft nondistended mildly to moderately tender in suprapubic region down into right groin, obese abdomen, no obvious palpable testicle in right inguinal canal, no testicles present in scrotum rectal no gross blood, hemoccult negative, no masses Physical Exam',
        'a while with one day of lower abdominal pain that started after having a bowel movement at 5pm. Felt a lower abdominal pressure that radiated into the groin which has not gone away since. Mild feeling sick to my stomach, no vomiting. Another bowel movement this evening. Feels pressure with voiding as if he has to push to expel urine, although has urinated multiple times since 5pm. Never had pain like this before, does not radiate to one side or the other. No fevers, no sweats, no chills. No prior surgeries. a while: premature born at 32 weeks, found to have crytorchidism at birth, testes had descended into the scrotum as a toddler per patient and my mother, no issues since PS: none a while Non contributory',
        '___ with one day of lower abdominal pain that started after having a bowel movement at 5pm. Felt a lower abdominal pressure that radiated into the groin which has not gone away since. Mild nausea, no vomiting. Another bowel movement this evening. Feels pressure with voiding as if he has to push to expel urine, although has urinated multiple times since 5pm. Never had pain like this before, does not radiate to one side or the other. No fevers, no sweats, no chills. No prior surgeries. Past Medical History: ___: premature born at 32 weeks, found to have crytorchidism at birth, testes had descended into the scrotum as a toddler per patient and his mother, no issues since PS: none Social History: ___ Family History: Non contributory',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('23919775',
        'Acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'Vitals: 98.8 54 116/44 18 98RA Gen-AAOx3, NAD HEENT-AT, NC, sclera non icteric Heart-RRR, normal S1, S2 Lungs-CTA B/L abdomen-soft, ND, diffuse discomfort to palpation extr-no edema',
        'a while p/w abdominal pain x18 hours. Pain started at 9pm, was sharp, a while, infra-umbilical, and migrated to the lower right side of my belly. He tried to vomit once to try to make himself feel better, however, he did not. I don''t have other episodes of feeling sick to my stomach or vomiting, as well as diarrhea, fevers, or chills. This is the first time he has had pain like this. Last meal approximately 24 hours ago. broken left arm treated with a cast a while NC',
        '___ p/w abdominal pain x18 hours. Pain started at 9pm, was sharp, ___, infra-umbilical, and migrated to the RLQ. He tried to vomit once to try to make himself feel better, however, he did not. He denies other episodes of nausea or vomiting, as well as diarrhea, fevers, or chills. This is the first time he has had pain like this. Last meal approximately 24 hours ago. Past Medical History: broken left arm treated with a cast Social History: ___ Family History: NC',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('27022201',
        'acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'PHYSICAL EXAMINATION upon admission: ___ Temp:99.0 HR:100 BP:126/67 Resp:18 O(2)Sat:100 Normal HEENT: Normocephalic, atraumatic Oropharynx within normal limits Chest: Clear to auscultation Cardiovascular: Regular Rate and Rhythm, Normal first and second heart sounds Abdominal: Palpation right lower quadrant with voluntary guarding, positive Psoas and Rovsing sign GU/Flank: No costovertebral angle tenderness Extr/Back: No cyanosis, clubbing or edema Skin: No rash Neuro: Speech fluent Psych: Normal mood, Normal mentation',
        'a while with 3 days aof worsening abdominal pain. Patient was seen in emergency room for pelvic pain 2 days ago, and was cleared from a GYN standpoint and sent home. Pain progressed in the lower right side of my belly and fevers ensued with a T max of 101.0. Endorsed nbausea, but no vomiting. Last BM yesterday was diarrhea. Normal urination. No STD history. No chest pain, SOB, dyspnea. No headache, vision changes, or mental status changes. none a while NC',
        'HPI: ___ with 3 days aof worsening abdominal pain. Patient was seen in ED for pelvic pain 2 days ago, and was cleared from a GYN standpoint and sent home. Pain progressed in the RLQ and fevers ensued with a T max of 101.0. Endorsed nbausea, but no vomiting. Last BM yesterday was diarrhea. Normal urination. No STD history. No chest pain, SOB, dyspnea. No headache, vision changes, or mental status changes. Past Medical History: none Social History: ___ Family History: NC',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('24238743',
        'acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'upon admission: ___ Vitals: 98.7 57 126/74 17 100% RA GEN: A&O, NAD ABD: Soft, nondistended, tender to palpation in the RLQ at ___, negative Rosving''s sign, negative psoas sign, no rebound or guarding, no palpable masses, no hepatosplenomegaly Physical examination',
        'Mr. a while is a a while year old man I have "crampy aching" abdominal pain which began gradually yesterday afternoon and localized to the periumbilical region initially. He subsequently experienced diarrhea (3 loose stools) and NBNB vomiting (a while). I have that my abdominal pain has now migrated to the lower right side of my belly and is constant in nature. He reported subjective fever and chills. He denied any anorexia as patient had some crackers in the emergency room this morning. I don''t have any recent sick contacts and travel. GERD a while nc',
        'HPI: Mr. ___ is a ___ year old man presenting with "crampy aching" abdominal pain which began gradually yesterday afternoon and localized to the periumbilical region initially. He subsequently experienced diarrhea (3 loose stools) and NBNB vomiting (___). He reports that his abdominal pain has now migrated to the RLQ and is constant in nature. He reported subjective fever and chills. He denied any anorexia as patient had some crackers in the ED this morning. He denies any recent sick contacts and travel. Past Medical History: Past Medical History: GERD Social History: ___ Family History: nc',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('26687335',
        'appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with appendicitis',
        'APPENDICITIS',
        'active',
        'upon admission: ___ Temp: 97.8 HR: 91 BP: 109/77 Resp: 18 O(2)Sat: 100 Normal Constitutional: Comfortable HEENT: Normocephalic, atraumatic Chest: Clear to auscultation Cardiovascular: Regular Rate and Rhythm Abdominal: Soft, TTP in RLQ +BS GU/Flank: No costovertebral angle tenderness Extr/Back: No cyanosis, clubbing or edema Skin: No rash Neuro: Speech fluent Psych: Normal mood Physical examination',
        'This is a a while year-old male, a while only , with history of chronic kidney disease, presents with a 24 hour-history of abdominal pain. With the help of an interpreter, patient states that pain was initially mild and located in my epigastrium and periumbilical area, later increasing in severity and shifting towards my right lower quadrant. He currently rates it as a while in intensity. Concomitantly, he endorses feeling sick to my stomach and multiple episodes of non-bloody, non-bilious vomiting, which have now resolved. Also, chills but no quantified fever, as well as a couple non-bloody, loose bowel movements earlier today. He presents to our Emergency Department for evaluation and management. Hyperaldosteronism a while adrenal adenoma s/p RFA recently CKD HTN DM2 a while Father: HTN, CAD Mother: HTN a while: HTN No family h/o renal disease.',
        'This is a ___ year-old male, ___ only , with history of chronic kidney disease, presents with a 24 hour-history of abdominal pain. With the help of an interpreter, patient states that pain was initially mild and located in his epigastrium and periumbilical area, later increasing in severity and shifting towards his right lower quadrant. He currently rates it as ___ in intensity. Concomitantly, he endorses nausea and multiple episodes of non-bloody, non-bilious emesis, which have now resolved. Also, chills but no quantified fever, as well as a couple non-bloody, loose bowel movements earlier today. He presents to our Emergency Department for evaluation and management. Past Medical History: Hyperaldosteronism ___ adrenal adenoma s/p RFA on ___ CKD HTN DM2 Social History: ___ Family History: Father: HTN, CAD Mother: HTN ___: HTN No family h/o renal disease.',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('21409557',
        'appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with appendicitis',
        'APPENDICITIS',
        'active',
        '___: VS: 98.6 69 117/71 16 99% RA Gen: NAD, AOx3 ___: reg Pulm: no distress Abd: S/ND/TTP RLQ - Rovsing''s ___: no LLE',
        'a while year old female presents with abdominal pain for 14 hours. Patient noted crampymid/upper middle part of my stomach abdominal pain last night which has progressed to lower right side of my belly pain. Associated with feeling sick to my stomach and chills. Denies vomiting, diarrhea, fevers, recent weight changes. Denies prior symptoms. GERD, MS, h/o pyelonephritis a while NC',
        '___ year old female presents with abdominal pain for 14 hours. Patient noted crampymid/epigastric abdominal pain last night which has progressed to RLQ pain. Associated with nausea and chills. Denies emesis, diarrhea, fevers, recent weight changes. Denies prior symptoms. Past Medical History: GERD, MS, h/o pyelonephritis Social History: ___ Family History: NC',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('25514003',
        'Primary Diagnosis: Cholecystitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with primary diagnosis: cholecystitis',
        'PRIMARY DIAGNOSIS: CHOLECYSTITIS',
        'active',
        '97.9 94 ___ Gen: well-appearing male, appears younger than stated age, NAD, no icterus, somnolent HEENT: NC/AT, EOMI, PERRLA bilat., MMM, without cervical LAD on my exam Cor: RRR without m/g/r, no JVD, no bruits Lungs: CTA bilat. ___: +BS, soft, ND, NT, no masses, no hernias Ext: warm feet, no edema',
        'a while yr old male 8 mo sp lap gastric band. Has 2 month history of upper right side of my belly pain radiating to my epigastrum and back after fatty food intake. Pt starts a while min after ingestion and lasts a while hrs. ER work up has included US of upper right side of my belly which shows gallstones, CT scan which shows possible duodenal diverticulum, KUB/CXR which shows band to be in good position. Lab work up not consistent with acute infection or common bile duct stone. Pt has not been compliant with a post band diet eating high fat content food and sweets. I did not inquire about my exercise habits. He has lost 31lbs since entiring the weight loss program and has been able to keep it off. HTN, Obesity, PTSD, Anxiety, Rt shoudler and Lt knee ligament damage for which he takes Naproxen (PRN) a while FH: Non-contributory',
        '___ yr old male 8 mo sp lap gastric band. Has 2 month hx of RUQ pain radiating to his epigastrum and back after fatty food intake. Pt starts ___ min after ingestion and lasts ___ hrs. ER work up has included US of RUQ which shows gallstones, CT scan which shows possible duodenal diverticulum, KUB/CXR which shows band to be in good position. Lab work up not consistent with acute infection or common bile duct stone. Pt has not been compliant with a post band diet eating high fat content food and sweets. I did not inquire about his exercise habits. He has lost 31lbs since entiring the weight loss program and has been able to keep it off. Past Medical History: HTN, Obesity, PTSD, Anxiety, Rt shoudler and Lt knee ligament damage for which he takes Naproxen (PRN) Social History: ___ Family History: FH: Non-contributory',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('27553284',
        'Primary diagnosis: Cholecystitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with primary diagnosis: cholecystitis',
        'PRIMARY DIAGNOSIS: CHOLECYSTITIS',
        'active',
        'Vital Signs: T 98.2 HR 66 BP 97/51 RR 16 O2 Sat 96%RA General: Alert and oriented x 3, more comfortable after receiving pain medications. HEENT: nonicteric, atraumatic/normocephalic Cardiovascular: S1,S2, Regular rate and Rhythm, no murmurs/rubs/gallops Lungs: Clear to auscultation bilaterally, no rhonchi/rales/crackles Abdomen: tenderness to deep palpation along entire subcostal region bilaterally, tenderness to deep palpation on RUQ, ___ sign, marked tenderness to light palpation on R upper back',
        'Patient is a a while year old female who is 8 weeks postpartum who presented for work up of right upper quadrant pain. She was found on HIDA scan to not have filling of the duodenum so she was sent here from a while for ERCP. On a while She had an ERCP which found sludge and they performed a sphincterotomy. She says that since then my pain was much improved until PPD 2 when after eating my pain recurred one hour after eating and then resolved after an hour. She then had return of pain after eating dinner which did not resolve and progressively got worse. She also started having feeling sick to my stomach and vomiting. Nonbloody and originally nonbilious until this morning. She is passing gas and having diarrhea. Denies fever asthma, rheumatoid arthritis, anemia, colon polyps, GERD a while DM, asthma',
        'Patient is a ___ year old female who is 8 weeks postpartum who presented for work up of right upper quadrant pain. She was found on HIDA scan to not have filling of the duodenum so she was sent here from ___ for ERCP. On ___ She had an ERCP which found sludge and they performed a sphincterotomy. She says that since then her pain was much improved until PPD 2 when after eating her pain recurred one hour after eating and then resolved after an hour. She then had return of pain after eating dinner which did not resolve and progressively got worse. She also started having nausea and vomiting. Nonbloody and originally nonbilious until this morning. She is passing gas and having diarrhea. Denies fever Past Medical History: asthma, rheumatoid arthritis, anemia, colon polyps, GERD Social History: ___ Family History: DM, asthma',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('29328838',
        'PRIMARY: 
    Diverticulitis
    
    SECONDARY:
    Hypertension
    Psychiatric Disorders
    Peripheral vascular disease s/p Left common femoral 
    endarterectomy',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with primary: 
    diverticulitis
    
    secondary:
    hypertension
    psychiatric disorders
    peripheral vascular disease s/p left common femoral 
    endarterectomy',
        'PRIMARY: 
    DIVERTICULITIS
    
    SECONDARY:
    HYPERTENSION
    PSYCHIATRIC DISORDERS
    PERIPHERAL VASCULAR DISEASE S/P LEFT COMMON FEMORAL 
    ENDARTERECTOMY',
        'published',
        'ADMISSION PHYSICAL EXAM ============================= Vitals: T 98.1 / BP 136/83 / HR 64 / RR 18 / O2sat 97%RA GEN: A&O, NAD HEENT: No scleral icterus, mucus membranes moist CV: RRR, No M/G/R PULM: non-labored respirations on RA ABD: Soft, nondistended, focal mild TTP LLQ, no rebound or guarding, normoactive bowel sounds, no palpable masses. Left groin incision well healed Extremities: warm and well-perfused Neuro: A&OX3',
        'a while s/p left common femoral endarterectomy a while with Dr. a while with complain of left groin pain at incision site for 3 days, found on OSH CT scan (currently unavailable) to have reported 2 cm collection superficial to CFA. The patient states she has had 3 days of left groin pain that is a while, causing my to go to my PCP a while. my PCP obtained a while CT scan which revealed the fluid collection. She came to a while emergency room after learning the results. The scans are not currently available due to a tech issue. I have taking my Plavix as prescribed (scheduled to stop next day after admission). I don''t have numbness or tingling in either lower extremity, extremities are WWP, and denies CP, SOB, HA, and all other symptoms. HTN migraines, takes fioricet multiple times a day IBS OA ?seizure disorder GERD depression borderline personality d/o narcotic abuse had port-a-cath for "IVF" for "chronic ileus" per patient a while NC',
        '___ s/p left common femoral endarterectomy ___ with Dr. ___ with complain of left groin pain at incision site for 3 days, found on OSH CT scan (currently unavailable) to have reported 2 cm collection superficial to CFA. The patient states she has had 3 days of left groin pain that is ___, causing her to go to her PCP ___. Her PCP obtained ___ CT scan which revealed the fluid collection. She came to ___ ED after learning the results. The scans are not currently available due to a tech issue. She reports taking her Plavix as prescribed (scheduled to stop next day after admission). She denies numbness or tingling in either lower extremity, extremities are WWP, and denies CP, SOB, HA, and all other symptoms. Past Medical History: HTN migraines, takes fioricet multiple times a day IBS OA ?seizure disorder GERD depression borderline personality d/o narcotic abuse had port-a-cath for "IVF" for "chronic ileus" per patient Social History: ___ Family History: NC',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('20535755',
        'Cholecystitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with cholecystitis',
        'CHOLECYSTITIS',
        'archived',
        'ADMISSION EXAM Vitals: 98 127/61 91 18 99% RA General: ___ speaking male in NAD HEENT: NCAT, sclera anicteric, PERRLA. oropharynx clear Neck: JVD halfway up neck Heart: RRR normal s1s2, no murmurs Lungs: CTAB, no wheezes Abdomen: +BS, soft, slightly distended, tender to palpation in center and RUQ. no rebound or guarding. Extremities: warm and well perfused, no ___ edema, DP 2+ bilaterally Neurological: A&Ox3. strength ___ in upper and lower extremities. no asterixis',
        'a while y/o M with medical history of ETOH/ HCV related cirrhosis c/b portal HTN with ascites and HCC s/p RFA and TACE who was transfered from OSH for further evaluation and care. Per OSH records, patient presented recently to emergency room with sudden onset severe upper right side of my belly pain that was "on and off." Reports pain is often postprandial but not always. Pain did not radiate. Reported +n/v. He denied any f/c, hematemesis, melana, BRBPR or any changes in bowel habits. On admission he was found to have albumin of 2.1, total bili of 1. SGOT 36, SGPT 57 with alk phose of 169 and lipase of 299. my WBC count was 6 and HCT 34. He had an upper right side of my belly ultrasound which showed multiple gallstones and increased echogenicity of central liver and portal triads representing possible hepatitis. Also showed gallbladder wall thickening. Negative Murphys sign but large gallstones present. Also had MRI of abdomen which showed large stone in the gb with thickened gb wall and surrounding fluid. CBD is dilated to 10 mm but obstructing lesion could not be seen. Given elevation of bili to 1.5 and alk phos to 193, patient was started on zosyn and transferred to a while for further evaluation and consideration of cholecystectomy with IOC. . On the floor, he feels some abdominal discomfort. Also is complaining of lower back pain. Is asking to eat . Review of Systems: Denies CP, SOB, constipation, diarrhea, urinary symptoms. 1. HCC s/p RFA a while, s/p TACE a while - initially diagnosed in a while - a while: abdominal ultrasound concerning for 2.2-cm echogenic lesion in the right liver lobe concerning for a while - a while: MRI with two arterial enhancing lesions in the liver, one at the junction of the posterior segment II and III and the other at the junction of segments VI and VII, the larger of the two lesions measuring 2.8 cm. - a while: underwent RFA to a 2.3 X 2.8 CM lesion at the junction of segment VI and VII. Second left lobe tumor could not be treated due to an inadequate window for needle access. Post treatment course c/b right hemothorax requiring chest tube placement - a while: underwent TACE 2. EtOH and HCV cirrhosis c/b portal HTN with ascites and variceal bleed 3. HCV, genotype I 4. HTN 5. past Syphilis infection, s/p treatment, negative RPR per notes a while No family h/o liver disease or colon cancer. my mom has arthritis. my dad had diabetes. my sister has diabetes',
        '___ y/o M with PMH of ETOH/ HCV related cirrhosis c/b portal HTN with ascites and HCC s/p RFA and TACE who was transfered from OSH for further evaluation and care. Per OSH records, patient presented on ___ to ED with sudden onset severe RUQ pain that was "on and off." Reports pain is often postprandial but not always. Pain did not radiate. Reported +n/v. He denied any f/c, hematemesis, melana, BRBPR or any changes in bowel habits. On admission he was found to have albumin of 2.1, total bili of 1. SGOT 36, SGPT 57 with alk phose of 169 and lipase of 299. His WBC count was 6 and HCT 34. He had an RUQ ultrasound which showed multiple gallstones and increased echogenicity of central liver and portal triads representing possible hepatitis. Also showed gallbladder wall thickening. Negative Murphys sign but large gallstones present. Also had MRI of abdomen which showed large stone in the gb with thickened gb wall and surrounding fluid. CBD is dilated to 10 mm but obstructing lesion could not be seen. Given elevation of bili to 1.5 and alk phos to 193, patient was started on zosyn and transferred to ___ for further evaluation and consideration of cholecystectomy with IOC. . On the floor, he feels some abdominal discomfort. Also is complaining of lower back pain. Is asking to eat . Review of Systems: Denies CP, SOB, constipation, diarrhea, urinary symptoms. Past Medical History: 1. HCC s/p RFA ___, s/p TACE ___ - initially diagnosed in ___ - ___: abdominal ultrasound concerning for 2.2-cm echogenic lesion in the right liver lobe concerning for ___ - ___: MRI with two arterial enhancing lesions in the liver, one at the junction of the posterior segment II and III and the other at the junction of segments VI and VII, the larger of the two lesions measuring 2.8 cm. - ___: underwent RFA to a 2.3 X 2.8 CM lesion at the junction of segment VI and VII. Second left lobe tumor could not be treated due to an inadequate window for needle access. Post treatment course c/b right hemothorax requiring chest tube placement - ___: underwent TACE 2. EtOH and HCV cirrhosis c/b portal HTN with ascites and variceal bleed 3. HCV, genotype I 4. HTN 5. past Syphilis infection, s/p treatment, negative RPR per notes Social History: ___ Family History: No family h/o liver disease or colon cancer. His mom has arthritis. His dad had diabetes. His sister has diabetes',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('27361644',
        'Appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with appendicitis',
        'APPENDICITIS',
        'draft',
        'Admission Vitals: 98.9, 60, 122/68, 16, 99% RA GEN: A&O, NAD HEENT: No scleral icterus, mucus membranes moist CV: RRR, No M/G/R PULM: Clear to auscultation b/l, No W/R/R ABD: Tenderness with localized rebound at RLQ. Otherwise is soft, nondistended, nontender. + psoas sign Ext: No ___ edema, ___ warm and well perfused',
        'I came here because of 12 hours of acute abdominal pain. Symptoms began suddenly upon waking this AM. Pain was initially at periumbillical area but now radiated to my lower right side of my belly. Reports one episode of vomiting and anorexia. Denies fever, chills, diarrhea, and urinary symptoms. Has not tried analgesics for symptoms. Upon evaluation. No acute distress. VSS. Abdomen soft, non-distended. He has localized tenderness with rebound at lower right side of my belly. Otherwise my abdomen is soft. Pain is reproducible with RLE extension. Also has psoas sign. No rovsing. Work up notable for leukocytosis to a while with left shift. Imaging demonstrating inflamed retrocecal appendix without signs of perforation. none a while Non-contributory',
        'Patient presents with 12 hours of acute abdominal pain. Symptoms began suddenly upon waking this AM. Pain was initially at periumbillical area but now radiated to his RLQ. Reports one episode of emesis and anorexia. Denies fever, chills, diarrhea, and urinary symptoms. Has not tried analgesics for symptoms. Upon evaluation. No acute distress. VSS. Abdomen soft, non-distended. He has localized tenderness with rebound at RLQ. Otherwise his abdomen is soft. Pain is reproducible with RLE extension. Also has psoas sign. No rovsing. Work up notable for leukocytosis to ___ with left shift. Imaging demonstrating inflamed retrocecal appendix without signs of perforation. Past Medical History: none Social History: ___ Family History: Non-contributory',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('24019757',
        'PRIMARY: 
    1. Acute on Chronic Pancreatitis with Common Bile Duct stricture
    2. Acute Kidney Injury
    3. Hypertension
    4. Alcohol Withdrawal
    5. Hyponatremia
    6. Hypokalemia
    7. Hypophosphatemia
    8. Leukocytosis unspecified
    9. Elevated PSA
    10. Hepatitis
    11. Hepatic steatosis
    12. Thrombosed Mesenteric veins
    13. Hemorragic Pancreatitis / Hematobilia
    
    SECONDARY: 
    1. Uncontrolled Type II DM
    2. HLD
    3. Insomnia
    4. Alcoholism',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with primary: 
    1. acute on chronic pancreatitis with common bile duct stricture
    2. acute kidney injury
    3. hypertension
    4. alcohol withdrawal
    5. hyponatremia
    6. hypokalemia
    7. hypophosphatemia
    8. leukocytosis unspecified
    9. elevated psa
    10. hepatitis
    11. hepatic steatosis
    12. thrombosed mesenteric veins
    13. hemorragic pancreatitis / hematobilia
    
    secondary: 
    1. uncontrolled type ii dm
    2. hld
    3. insomnia
    4. alcoholism',
        'PRIMARY: 
    1. ACUTE ON CHRONIC PANCREATITIS WITH COMMON BILE DUCT STRICTURE
    2. ACUTE KIDNEY INJURY
    3. HYPERTENSION
    4. ALCOHOL WITHDRAWAL
    5. HYPONATREMIA
    6. HYPOKALEMIA
    7. HYPOPHOSPHATEMIA
    8. LEUKOCYTOSIS UNSPECIFIED
    9. ELEVATED PSA
    10. HEPATITIS
    11. HEPATIC STEATOSIS
    12. THROMBOSED MESENTERIC VEINS
    13. HEMORRAGIC PANCREATITIS / HEMATOBILIA
    
    SECONDARY: 
    1. UNCONTROLLED TYPE II DM
    2. HLD
    3. INSOMNIA
    4. ALCOHOLISM',
        'active',
        'ADMISSION ======================== Vitals: 97.8 110/60 85 18 94 RA General: Alert, oriented, no acute distress HEENT: Sclera anicteric, dry mucous membranes, oropharynx clear Neck: supple, JVP not elevated, no LAD Lungs: Clear to auscultation bilaterally, no wheezes, rales, rhonchi CV: Regular rate and rhythm, normal S1 + S2, no murmurs, rubs, gallops Abdomen: soft, ___, bowel sounds present, no rebound tenderness or guarding, no organomegaly; no stigmata of liver disease Ext: Warm, well perfused, 2+ pulses, no clubbing, cyanosis or edema Skin: no rashes, scars or legions Neuro: WNL',
        'a while year old male with history of alcoholism, HTN, new diagnosis of diabetes here with 2 weeks of generalized weakness, malaise, and 1 week of vomiting and diarrhea. Reports at recent PCP appointment was told he might be diabetic but has not started any meds. Went to urgent care recently where he was given zofran and cyclobenzabrine for abdominal cramping and told that he had the flu. I don''t have ever having a flu swab. The patient has not had any tamiflu. The patient says that on 1 day prior to admission he was unable to walk to the bathroom without becoming extremely exhausted. The patient called my PCP recently morning and was told to come to the emergency room. Of note the patient''s creatinine recently was 0.8. Patient increased my Lisinopril from 10mg to 20mg on the a while. On arrival to the emergency room the patient was found to have vitals of 0 97.5 88 77/44 16 92% RA. Patient bolused fluids and my blood pressure improved. The patient''s labs were significant for a transaminitis, Cr of 6.2, Na of 129 and hyperbilirubinemia of 2.1. The patient was seen by the GI team. US showed mild dilation in bile duct, but no active signs of cholecystitis. On arrival to the floor patient''s vitals were 97.8 110/60 85 18 94 RA. Patient was anxious but not in acute distress. Review of systems: (+) Per (-) Denies fever, chills, night sweats, recent weight loss or gain. Denies headache, sinus tenderness, rhinorrhea or congestion. Denies cough, shortness of breath. Denies chest pain or tightness, palpitations. Denies feeling sick to my stomach, vomiting, diarrhea, constipation or abdominal pain. No recent change in bowel or bladder habits. No dysuria. Denies arthralgias or myalgias. Chronic HYPERCHOLESTEROLEMIA BPH HYPERTENSION - ESSENTIAL, BENIGN FATTY LIVER ESOPHAGEAL REFLUX Sleep apnea Alcoholism Type 2 diabetes mellitus, uncontrolled a while Father passed away of bladder cancer',
        '___ year old male with history of alcoholism, HTN, new diagnosis of diabetes here with 2 weeks of generalized weakness, malaise, and 1 week of vomiting and diarrhea. Reports at recent PCP appointment was told he might be diabetic but has not started any meds. Went to urgent care on ___ where he was given zofran and cyclobenzabrine for abdominal cramping and told that he had the flu. He denies ever having a flu swab. The patient has not had any tamiflu. The patient says that on 1 day prior to admission he was unable to walk to the bathroom without becoming extremely exhausted. The patient called his PCP on ___ morning and was told to come to the emergency room. Of note the patient''s creatinine on ___ was 0.8. Patient increased his Lisinopril from 10mg to 20mg on the ___. On arrival to the ED the patient was found to have vitals of 0 97.5 88 77/44 16 92% RA. Patient bolused fluids and his blood pressure improved. The patient''s labs were significant for a transaminitis, Cr of 6.2, Na of 129 and hyperbilirubinemia of 2.1. The patient was seen by the GI team. US showed mild dilation in bile duct, but no active signs of cholecystitis. On arrival to the floor patient''s vitals were 97.8 110/60 85 18 94 RA. Patient was anxious but not in acute distress. Review of systems: (+) Per HPI (-) Denies fever, chills, night sweats, recent weight loss or gain. Denies headache, sinus tenderness, rhinorrhea or congestion. Denies cough, shortness of breath. Denies chest pain or tightness, palpitations. Denies nausea, vomiting, diarrhea, constipation or abdominal pain. No recent change in bowel or bladder habits. No dysuria. Denies arthralgias or myalgias. Past Medical History: Chronic HYPERCHOLESTEROLEMIA BPH HYPERTENSION - ESSENTIAL, BENIGN FATTY LIVER ESOPHAGEAL REFLUX Sleep apnea Alcoholism Type 2 diabetes mellitus, uncontrolled Social History: ___ Family History: Father passed away of bladder cancer',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('28694648',
        'Perforated sigmoid diverticulitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with perforated sigmoid diverticulitis',
        'PERFORATED SIGMOID DIVERTICULITIS',
        'active',
        'Vitals: T 99.0 HR 118 BP 131/93 RR 18 So2 100% GEN: A&O, NAD HEENT: No scleral icterus, mucus membranes moist CV: RRR, No M/G/R PULM: Clear to auscultation b/l, No W/R/R ABD: Slightly firm, nondistended, with diffuse tenderness to palpation and voluntary guarding. Has point tenderness on LLQ with mild rebound. DRE: normal tone, no gross or occult blood Ext: No ___ edema, ___ warm and well perfused',
        'a while I have 5 days of lower left side of my belly, subjective fevers and constipation. Patient started with a constant, slowly progressive lower abdominal pain (L>R) 5 days prior. Three days ago started experiencing chills and intense sweating heavily with no objective fevers. He has also been having feeling sick to my stomach, but no vomiting, poor appetite and has been constipated, but passing flatus. Has been loosing some weight for the past 6 months related to stress at work. Headaches a while Mother with HTN. Father died at a while of heart problems.',
        '___ presenting with 5 days of LLQ, subjective fevers and constipation. Patient started with a constant, slowly progressive lower abdominal pain (L>R) 5 days prior. Three days ago started experiencing chills and intense diaphoresis with no objective fevers. He has also been having nausea, but no vomiting, poor appetite and has been constipated, but passing flatus. Has been loosing some weight for the past 6 months related to stress at work. Past Medical History: Past Medical History: Headaches Social History: ___ Family History: Family History: Mother with HTN. Father died at ___ of heart problems.',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('20890008',
        'acute appendicitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute appendicitis',
        'ACUTE APPENDICITIS',
        'active',
        'Temp: 97.6 HR: 46 BP: 106/65 RR: 18 100% Ra Gen: NAD HEENT: non icteric, atraumatic CV: RRR no m,r,g RESP: CTABL Abd: soft, non tender, non distended, incisions c/d/i Ext: wwpx4, palpable distal pulses',
        'a while with no significant medical history I have acute onset of lower right side of my belly pain x 2 days. I''ve been feeling feeling lower quadrant discomfort recently morning but that it worsened significantly at night, waking my up from sleep. She endorses anorexia without feeling sick to my stomach or vomiting. Denies fevers or chills. Has some right-sided flank pain without dysuria or hematuria. Denies previous cold or flu symptoms. She no longer gets my period as she has an IUD in place. Denies a history of bloody stools, diarrhea, sick contacts or recent exposures. Travels within the a while for work and pleasure. medical history: none surgery history: none a while: none a while Fam Hx: no history of Crohn''s or UC. Grandfather with a while types of cancers including possible leukemia'' otherwise no history of malignancy',
        '___ with no significant PMH presenting with acute onset of RLQ pain x 2 days. Patient reports feeling lower quadrant discomfort on ___ morning but that it worsened significantly at night, waking her up from sleep. She endorses anorexia without nausea or vomiting. Denies fevers or chills. Has some right-sided flank pain without dysuria or hematuria. Denies previous URI symptoms. She no longer gets her period as she has an IUD in place. Denies a history of bloody stools, diarrhea, sick contacts or recent exposures. Travels within the ___ for work and pleasure. Past Medical History: PMH: none PSH: none ___: none Social History: ___ Family History: Fam Hx: no history of Crohn''s or UC. Grandfather with ___ types of cancers including possible leukemia'' otherwise no hx of malignancy',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('27850323',
        'Acute pancreatitis of unclear etiology (possible due to 
    gallstones, sludge)
    Hypertension',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute pancreatitis of unclear etiology (possible due to 
    gallstones, sludge)
    hypertension',
        'ACUTE PANCREATITIS OF UNCLEAR ETIOLOGY (POSSIBLE DUE TO 
    GALLSTONES, SLUDGE)
    HYPERTENSION',
        'active',
        'Admission Afeb, ___ 97%RA Cons: NAD, lying in bed Eyes: EOMI, no scleral icterus ENT: MMM Cardiovasc: rrr, no murmur, no edema Resp: CTA B GI: +bs,soft, nd, +epigastric and RUQ ttp MSK: no significant kyphosis Skin: no rashes Neuro: no facial droop Psych: blunted affect',
        'a while male with history of htn, PE here with abd pain. Pt reports that a few days ago he began to have a burning in the upper middle part of my stomach area. Over time, it increased to a "fire" with radiation to the back. He has also been having feeling sick to my stomach and vomiting, has not been eating due to vomiting. He has had a few cold sweats, but no known fevers. He does not think that eating/drinking was making the pain worse. I don''t have diarrhea, history of gallstones. I feel that he usually drinks a "few gallons" of water a day because he likes to be always drinking something. I have taking "anticoagulant" for PE for 90 days. I feel that he hasn''t taken my BP medications this week due to feeling weak and the pain. I feel that he is not currently drinking alcohol, but sometimes does based on the client he is working with. States that he used to drink much more, but is not clear about how much. 10 systems reviewed and are otherwise negative. longstanding HTN --states that he has multiple medications for it, but cannot tel me what they are, thinks that he goes to a while, but not sure --in atrius records I do not see refill of norvasc, meto, lisinpril recently DVT/PE a while thinks that it was from going back and forth from a while and a while depression/anxiety-states no longer on zoloft, not taking gabapentin a while sister with a while htn in family',
        '___ male with hx of htn, PE here with abd pain. Pt reports that ___ days ago he began to have a burning in the epigastric area. Over time, it increased to a "fire" with radiation to the back. He has also been having nausea and vomiting, has not been eating due to vomiting. He has had a few cold sweats, but no known fevers. He does not think that eating/drinking was making the pain worse. He denies diarrhea, hx of gallstones. He states that he usually drinks a "few gallons" of water a day because he likes to be always drinking something. He reports taking "anticoagulant" for PE for 90 days. He states that he hasn''t taken his BP medications this week due to feeling weak and the pain. He states that he is not currently drinking alcohol, but sometimes does based on the client he is working with. States that he used to drink much more, but is not clear about how much. 10 systems reviewed and are otherwise negative. Past Medical History: longstanding HTN --states that he has multiple medications for it, but cannot tel me what they are, thinks that he goes to ___, but not sure --in atrius records I do not see refill of norvasc, meto, lisinpril recently DVT/PE ___ thinks that it was from going back and forth from ___ and ___ depression/anxiety-states no longer on zoloft, not taking gabapentin Social History: ___ Family History: sister with ___ htn in family',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('21166109',
        'PRIMARY DIAGNOSIS: 
    1. Cholecystitis
    2. Cholelithiasis
    
    SECONDARY DIAGNOSIS: 
    1. Coronary Artery Disease',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with primary diagnosis: 
    1. cholecystitis
    2. cholelithiasis
    
    secondary diagnosis: 
    1. coronary artery disease',
        'PRIMARY DIAGNOSIS: 
    1. CHOLECYSTITIS
    2. CHOLELITHIASIS
    
    SECONDARY DIAGNOSIS: 
    1. CORONARY ARTERY DISEASE',
        'active',
        'Vitals: 98.4, 144/84, 97, 22, 96% RA Gen: pleasant male, NAD, conversant HEENT: PERRLA, sclera anicteric, throat is clear, no cervical LAD CV: +s1s2, rrr, no mrg appreciated, PMI not palpable, JVP flat Lungs: ctab Abd: well healed RLQ scar from prior appendectomy, +BS, soft, NT, ND Ext: no c/c/e Neuro: A&O x3, CN ___ intact, otherwise nonfocal',
        'This is a a while yo male with recently admitted for ERCP and stent placement, NSTEMI s/p DES to a while who comes in today with recurrent abdominal pain. I have that the day after my discharge last week, he developed a fever and mild abdominal pain. He had called the covering GI doctors who recommended a course of ciprofloxacin and subsequently my fever and pain had subsided. He had been feeling well during the week, but on the day of admission reported upper middle part of my stomach abdominal pain in the morning. I have that he drank coffee and went for a a whileut my pain gradually increased and travelled to the right side of my chest. He then went to an OSH where he had negative troponins and an US that documents gallstones, GB wall thickening and a sonographic a while sign and was then transferred to a while. In the emergency room, my vitals were 96.9, 121/88, 88, 18, 99%RA. He received 4mg morphine sulfate, 1mg dilaudid, 4mg zofran for feeling sick to my stomach. There he had a repeat ultrasound and was seen by surgery. Currently, I don''t have abdominal pain, feeling sick to my stomach, vomiting. Aside from the fever last week, reports no recurrent fevers. No chest pain, a while swelling, orthopnea, PND, night sweats, sweating heavily, diarrhea, constipation, jaundice, dysuria. * NSTEMI in a while s/p DES to RCA x 2, 60% mid LAD LCX gives high OM1 wiht mild diffuse disease RCA 70% of diffuse distal disease with focal 90% distal RCA - a while MIBI without signs of ischemia * Choledocholithasis - s/p biliary stent in a while a while Father with MI at age a while. Three brothers with CABG and MI''s in a while, a while, and a while.',
        'This is a ___ yo male with recently admitted for ERCP and stent placement, NSTEMI s/p DES to ___ who comes in today with recurrent abdominal pain. He reports that the day after his discharge last week, he developed a fever and mild abdominal pain. He had called the covering GI doctors who recommended a course of ciprofloxacin and subsequently his fever and pain had subsided. He had been feeling well during the week, but on the day of admission reported epigastric abdominal pain in the morning. He reports that he drank coffee and went for a ___ut his pain gradually increased and travelled to the right side of his chest. He then went to an OSH where he had negative troponins and an US that documents gallstones, GB wall thickening and a sonographic ___ sign and was then transferred to ___. In the emergency room, his vitals were 96.9, 121/88, 88, 18, 99%RA. He received 4mg morphine sulfate, 1mg dilaudid, 4mg zofran for nausea. There he had a repeat ultrasound and was seen by surgery. Currently, he denies abdominal pain, nausea, vomiting. Aside from the fever last week, reports no recurrent fevers. No chest pain, ___ edema, orthopnea, PND, night sweats, diaphoresis, diarrhea, constipation, jaundice, dysuria. Past Medical History: * NSTEMI in ___ s/p DES to RCA x 2, 60% mid LAD LCX gives high OM1 wiht mild diffuse disease RCA 70% of diffuse distal disease with focal 90% distal RCA - ___ MIBI without signs of ischemia * Choledocholithasis - s/p biliary stent in ___ Social History: ___ Family History: Father with MI at age ___. Three brothers with CABG and MI''s in ___, ___, and ___.',
        'USR-EXP-001',
        'CRIT-001');
INSERT INTO clinical_case (case_id, title, description, type, status, pe, symptom, medicalhistory, created_by, eccid)
VALUES ('29897948',
        'Acute cholecystitis',
        'A patient came to the hospital for evaluation of abdominal symptoms, and was subsequently diagnosed with acute cholecystitis',
        'ACUTE CHOLECYSTITIS',
        'active',
        'On admission: Vitals: 98.8 73 153/99 15 100% RA GEN: NAD CV: RRR ABD: TTP RUQ, otherwise soft. EXT: no c/c/e',
        'a while w/h/o HTN, hysterectomy, hypothyroidism p/w abdominal pain. She noted the abdominal pain started suddenly yesterday evening worsening over the course of the day. + feeling sick to my stomach and vomiting, bilious x1. Normal BM this AM, no diarrhea/constipation. No dysuria/hematuria. No fevers. medical history: HTN surgery history: supracervical hysterectomy, PDA ligation @3, thyroidectomy a while my mother has hypertension. my maternal grandfather died at the age of a while with an MI. Two of my uncles died at the age of a while and a while respectively of MI. my brother has diabetes',
        '___ w/h/o HTN, hysterectomy, hypothyroidism p/w abdominal pain. She noted the abdominal pain started suddenly yesterday evening worsening over the course of the day. + nausea and vomiting, bilious x1. Normal BM this AM, no diarrhea/constipation. No dysuria/hematuria. No fevers. Past Medical History: PMH: HTN PSH: supracervical hysterectomy, PDA ligation @3, thyroidectomy Social History: ___ Family History: Her mother has hypertension. Her maternal grandfather died at the age of ___ with an MI. Two of her uncles died at the age of ___ and ___ respectively of MI. Her brother has diabetes',
        'USR-EXP-001',
        'CRIT-001');


-- Case 1: 27892518 (Richard Anderson)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10070247', '27892518', 'Richard Anderson', 43, 'MALE', 'he/him', 'Worker', 'Hispanic', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Uses painkillers excessively"]}', 
'Severe abdominal pain',
'{"bp": "114/91", "hr": 79, "spo2": 97, "rr": 21, "temp": "36.7"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Uses painkillers excessively", "Frequent ED visits", "Low pain tolerance"]',
'["Distinguish acute abdominal pain from chronic lower back pain", "Identify peritoneal irritation via Psoas and Rovsing signs", "Assess the impact of painkiller overuse on symptom presentation"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/richard_anderson.png');

-- Case 2: 21807759 (Anthony Garcia)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10073256', '21807759', 'Anthony Garcia', 68, 'MALE', 'he/him', 'Manager', 'Unknown', 
'{"emotional_state": "Neutral", "behavioral_rules": ["High pain tolerance"]}', 
'Chronic abdominal pain',
'{"bp": "129/88", "hr": 85, "spo2": 96, "rr": 20, "temp": "38.9"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Chronic alcohol consumption", "High pain tolerance", "Delayed hospital presentation"]',
'["Recognize appendicitis in elderly patients with high pain thresholds", "Analyze high fever (38.9) and right-sided guarding", "Evaluate appendicitis complications in patients with prior malignancy"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/anthony_garcia.png');

-- Case 3: 20345216 (Richard Jackson)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10080679', '20345216', 'Richard Jackson', 28, 'MALE', 'he/him', 'Engineer', 'Unknown', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Avoids medical care"]}', 
'Nausea and abdominal pain',
'{"bp": "120/92", "hr": 84, "spo2": 98, "rr": 22, "temp": "36.7"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Avoids medical care", "High work-related stress", "Continues eating despite nausea"]',
'["Map the migration of pain from periumbilical to RLQ", "Interpret a positive Obturator sign during physical exam", "Manage health anxiety in young patients who avoid medical care"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/richard_jackson.png');

-- Case 4: 24078130 (Donald Rodriguez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10085111', '24078130', 'Donald Rodriguez', 18, 'MALE', 'he/him', 'Teacher', 'Asian', 
'{"emotional_state": "Anxious", "behavioral_rules": ["High anxiety"]}', 
'Chronic abdominal pain',
'{"bp": "133/94", "hr": 92, "spo2": 97, "rr": 21, "temp": "36.4"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High work-related stress", "Irregular meal patterns", "Delays seeking consultation"]',
'["Evaluate suprapubic pain radiating to the groin in young males", "Correlate history of cryptorchidism with current abdominal symptoms", "Identify appendiceal dilation and fat stranding on imaging results"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/donald_rodriguez.png');

-- Case 5: 23919775 (Richard Lopez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10139369', '23919775', 'Richard Lopez', 22, 'MALE', 'he/him', 'Salesperson', 'Hispanic', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Limited access to healthcare"]}', 
'Chronic abdominal pain',
'{"bp": "134/75", "hr": 94, "spo2": 100, "rr": 17, "temp": "37.1"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Avoids medical care", "Stress-related symptoms", "Alcohol consumption"]',
'["Assess acute infra-umbilical pain migrating to RLQ", "Differentiate surgical abdomen from common flu symptoms", "Practice history taking with patients having limited healthcare access"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/richard_lopez.png');

-- Case 6: 27022201 (Donna Davis)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10154376', '27022201', 'Donna Davis', 19, 'FEMALE', 'she/her', 'Nurse', 'Unknown', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Uses painkillers excessively"]}', 
'Severe abdominal pain',
'{"bp": "146/75", "hr": 81, "spo2": 97, "rr": 17, "temp": "37.2"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Uses painkillers excessively", "Poor medication adherence", "Physically active"]',
'["Identify clinical signs of perforated appendicitis and early peritonitis", "Interpret CT findings of appendicolith and terminal ileum thickening", "Manage a diagnostic workup for pelvic pain in female patients"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/donna_davis.png');

-- Case 7: 24238743 (William Martinez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10156068', '24238743', 'William Martinez', 21, 'MALE', 'he/him', 'Programmer', 'African American', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Sedentary lifestyle"]}', 
'Chronic abdominal pain',
'{"bp": "137/70", "hr": 66, "spo2": 100, "rr": 22, "temp": "37.1"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Avoids medical care", "Sedentary lifestyle", "Poor insight into illness"]',
'["Differentiate crampy periumbilical pain from typical GERD symptoms", "Confirm obstructing appendicolith using pelvic CT imaging", "Assess the relevance of subjective fever and chills in young adults"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/william_martinez.png');

-- Case 8: 26687335 (Michael Smith)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10185476', '26687335', 'Michael Smith', 40, 'MALE', 'he/him', 'Salesperson', 'Unknown', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Heavy smoker"]}', 
'Nausea and abdominal pain',
'{"bp": "113/85", "hr": 80, "spo2": 98, "rr": 20, "temp": "36.6"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High work-related stress", "Self-medication", "Heavy smoker"]',
'["Evaluate abdominal pain in patients with Chronic Kidney Disease (CKD)", "Interpret cecal thickening at the appendiceal orifice on non-contrast CT", "Analyze the risk of appendicitis in patients with prior adrenal RFA"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/michael_smith.png');

-- Case 9: 21409557 (Sandra Jackson)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10205542', '21409557', 'Sandra Jackson', 50, 'FEMALE', 'she/her', 'Manager', 'African American', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Low pain tolerance"]}', 
'Chronic abdominal pain',
'{"bp": "123/74", "hr": 93, "spo2": 98, "rr": 22, "temp": "37.0"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Former smoker", "Delays seeking consultation", "Low pain tolerance"]',
'["Analyze epigastric pain progressing to RLQ in patients with Multiple Sclerosis", "Identify appendiceal dilation (9mm) and fat stranding on contrast CT", "Evaluate the clinical significance of nausea and chills without emesis"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/sandra_jackson.png');

-- Case 10: 25514003 (Matthew Jones)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10071659', '25514003', 'Matthew Jones', 34, 'MALE', 'he/him', 'Manager', 'Caucasian', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Ignores symptoms"]}', 
'Intermittent abdominal pain',
'{"bp": "145/89", "hr": 74, "spo2": 100, "rr": 19, "temp": "36.6"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Avoids medical care", "High anxiety", "Former smoker", "Ignores early pain"]',
'["Diagnose postprandial RUQ pain in patients with a laparoscopic gastric band", "Distinguish gastric band malposition from acute cholecystitis", "Analyze gallbladder wall thickening (5mm) in the setting of fatty liver"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/matthew_jones.png');

-- Case 11: 27553284 (Jessica Gonzalez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10069551', '27553284', 'Jessica Gonzalez', 33, 'FEMALE', 'she/her', 'Shop Owner', 'Unknown', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Health anxiety"]}', 
'Nausea and abdominal pain',
'{"bp": "144/84", "hr": 78, "spo2": 98, "rr": 21, "temp": "36.8"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High work-related stress", "Continues eating despite nausea", "Former smoker"]',
'["Identify recurrent RUQ pain following sphincterotomy and ERCP", "Analyze gallbladder sludge and small shadowing stones on ultrasound", "Evaluate post-partum risk factors for gallbladder disease"]',
30, 15, 'Beginner', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/jessica_gonzalez.png');

-- Case 12: 29328838 (Emily Rodriguez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10058856', '29328838', 'Emily Rodriguez', 73, 'FEMALE', 'she/her', 'Worker', 'African American', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Low pain tolerance"]}', 
'Chronic abdominal pain',
'{"bp": "120/93", "hr": 62, "spo2": 100, "rr": 16, "temp": "36.7"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High work-related stress", "High-fat diet", "Delayed hospital presentation"]',
'["Evaluate LLQ tenderness in elderly patients with a history of endarterectomy", "Interpret sigmoid wall thickening as a sign of chronic diverticular disease", "Assess groin pain and fluid collection following surgical intervention"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/emily_rodriguez.png');

-- Case 13: 20535755 (William Miller)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10056223', '20535755', 'William Miller', 48, 'MALE', 'he/him', 'Student', 'Unknown', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Sedentary lifestyle"]}',
'Chronic abdominal pain', 
'{"bp": "114/84", "hr": 86, "spo2": 100, "rr": 20, "temp": "36.7"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High anxiety", "Sedentary lifestyle", "Low socioeconomic status"]',
'["Identify acute cholecystitis in patients with portal hypertension and ascites", "Analyze CBD dilation (10mm) and thickened GB wall in a cirrhotic liver", "Assess liver function tests (Bilirubin, Alk Phos) to confirm biliary obstruction"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/william_miller.png');

-- Case 14: 27361644 (Daniel Thomas)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10052530', '27361644', 'Daniel Thomas', 22, 'MALE', 'he/him', 'Teacher', 'Asian', 
'{"emotional_state": "Anxious", "behavioral_rules": ["High anxiety"]}', 
'Chronic abdominal pain',
'{"bp": "137/84", "hr": 89, "spo2": 97, "rr": 19, "temp": "37.2"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High anxiety", "Ignores early abdominal pain", "Limited family support"]',
'["Recognize sudden onset of periumbilical pain radiating to RLQ", "Confirm early appendicitis via retrocecal fluid and hyperemic walls on CT", "Evaluate the significance of localized rebound tenderness and Psoas sign"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/daniel_thomas.png');

-- Case 15: 24019757 (Michael Hernandez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10046241', '24019757', 'Michael Hernandez', 53, 'MALE', 'he/him', 'Manager', 'Hispanic', 
'{"emotional_state": "Depressed", "behavioral_rules": ["Irregular meal patterns"]}', 
'Intermittent abdominal pain',
'{"bp": "133/93", "hr": 80, "spo2": 97, "rr": 19, "temp": "36.9"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Depressive mood", "Limited access to healthcare", "Irregular meal patterns"]',
'["Identify necrotizing pancreatitis in a patient with chronic alcoholism", "Analyze severe lab abnormalities (Cr 6.2, Na 129) and transaminitis", "Evaluate thrombosed mesenteric veins and portal vein occlusion on CT"]',
30, 15, 'Expert', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/michael_hernandez.png');

-- Case 16: 28694648 (Thomas Lopez)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10042037', '28694648', 'Thomas Lopez', 55, 'MALE', 'he/him', 'Manager', 'Hispanic', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Physically active"]}', 
'Chronic abdominal pain',
'{"bp": "112/94", "hr": 97, "spo2": 98, "rr": 18, "temp": "37.2"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Physically demanding job", "Stress-related symptoms", "Regular medical checkups"]',
'["Recognize perforated sigmoid diverticulitis via extraluminal gas on CT", "Evaluate point tenderness on LLQ with mild rebound and constipation", "Analyze the impact of work-related stress on gastrointestinal symptoms"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/thomas_lopez.png');

-- Case 17: 20890008 (Susan Thomas)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10040626', '20890008', 'Susan Thomas', 29, 'FEMALE', 'she/her', 'Student', 'Caucasian', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Health anxiety"]}', 
'Intermittent abdominal pain',
'{"bp": "106/65", "hr": 46, "spo2": 97, "rr": 18, "temp": "36.4"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Avoids medical care", "High-fat diet", "Health anxiety"]',
'["Identify acute appendicitis with associated enlarged mesenteric lymph nodes", "Analyze anorexia as a primary symptom in young adult females", "Evaluate CT findings of mucosal hyperenhancement in the appendix"]',
30, 15, 'Beginner', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/susan_thomas.png');

-- Case 18: 27850323 (William Brown)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10040056', '27850323', 'William Brown', 31, 'MALE', 'he/him', 'Retired', 'African American', 
'{"emotional_state": "Depressed", "behavioral_rules": ["Ignores symptoms"]}', 
'Intermittent abdominal pain',
'{"bp": "112/93", "hr": 76, "spo2": 100, "rr": 16, "temp": "37.1"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Depressive mood", "Ignores early abdominal pain", "Low-fiber diet"]',
'["Identify epigastric fire-like pain radiating to the back as acute pancreatitis", "Assess pancreatic necrosis and hemorrhage on MRCP/MRI imaging", "Manage hypertension and medication non-compliance in acute clinical settings"]',
30, 15, 'Advanced', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/william_brown.png');

-- Case 19: 21166109 (Michael Miller)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10034272', '21166109', 'Michael Miller', 69, 'MALE', 'he/him', 'Teacher', 'Hispanic', 
'{"emotional_state": "Neutral", "behavioral_rules": ["Non-compliant with diet"]}', 
'Chronic abdominal pain',
'{"bp": "123/94", "hr": 65, "spo2": 98, "rr": 18, "temp": "36.9"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["High work-related stress", "Non-compliant with dietary restriction", "Social smoker"]',
'["Recognize recurrent RUQ pain in a patient with a prior biliary stent", "Interpret significant gallbladder wall thickening (1.2 cm) and sludge", "Analyze clinical risks of cholecystitis in patients with Coronary Artery Disease"]',
30, 15, 'Advanced', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/michael_miller.png');

-- Case 20: 29897948 (Mary Taylor)
INSERT INTO virtual_patient (patient_id, case_id, name, age, gender, pronouns, occupation, ethnicity, persona, chief_concern, vital_signs, instructions, behaviors, learning_objectives, time_setting, argument_time, level, case_rule, status, avatar_image)
VALUES ('10031940', '29897948', 'Mary Taylor', 58, 'FEMALE', 'she/her', 'Engineer', 'Hispanic', 
'{"emotional_state": "Anxious", "behavioral_rules": ["Excessive painkillers"]}', 
'Severe abdominal pain',
'{"bp": "114/90", "hr": 68, "spo2": 98, "rr": 16, "temp": "36.9"}',
'{"role": "Medical Learner", "task": "Evaluate and diagnose", "procedure": ["Enter room", "Interaction", "Reasoning"]}',
'["Uses painkillers excessively", "High-fat diet", "Health anxiety"]',
'["Diagnose acute gallbladder disease in a patient with a 2.5cm gallstone", "Identify hepatic steatosis and mesenteric nodes on contrast CT", "Manage acute abdominal pain in patients with HTN and hypothyroidism"]',
30, 15, 'Intermediate', '{"rules": ["Chief Concern", "HPI", "Diagnostic Impression"], "totalTime": "45"}', 'active', 'https://example.com/avatars/mary_taylor.png');



INSERT INTO labtestitem (itemid, label, fluid, category, count)
VALUES (51222, 'Hemoglobin', 'Blood', 'Hematology', 1000),
       (51301, 'White Blood Cells', 'Blood', 'Hematology', 1000),
       (50868, 'Anion Gap', 'Blood', 'Chemistry', 1000),
       (50912, 'Creatinine', 'Blood', 'Chemistry', 1000),
       (50813, 'Lactate', 'Blood', 'Blood Gas', 1000),
       (51274, 'PT', 'Blood', 'Hematology', 1000),
       (51491, 'pH', 'Urine', 'Chemistry', 1000),
       (51492, 'Protein', 'Urine', 'Chemistry', 1000),
       (50861, 'Alanine Aminotransferase (ALT)', 'Blood', 'Chemistry', 1000),
       (50863, 'Alkaline Phosphatase', 'Blood', 'Chemistry', 1000),
       (50878, 'Aspartate Aminotransferase (AST)', 'Blood', 'Chemistry', 1000),
       (50885, 'Bilirubin, Total', 'Blood', 'Chemistry', 1000);

INSERT INTO labtestitem (itemid, label, fluid, category, count)
VALUES (51006, 'Urea Nitrogen', 'Blood', 'Chemistry', 1000);
INSERT INTO labtestitem (itemid, label, fluid, category, count)
VALUES (50956, 'Lipase', 'Blood', 'Chemistry', 1000);
INSERT INTO labtestitem (itemid, label, fluid, category, count)
VALUES (50931, 'Glucose', 'Blood', 'Chemistry', 1000);
INSERT INTO labtestitem (itemid, label, fluid, category, count)
VALUES (50910, 'Creatine Kinase (CK)', 'Blood', 'Chemistry', 1000);
INSERT INTO laboratorytest (clinicalcase_id, itemid, value, rangelower, rangeupper)
VALUES ('20890008', 51222, '13.1 g/dL', '12.0', '16.0'),
       ('20890008', 51301, '9.9 K/uL', '4.0', '11.0'),
       ('27892518', 51301, '19.2 K/uL', '4.0', '11.0'),
       ('21807759', 50813, '2.4 mmol/L', '0.5', '2.0'),
       ('20345216', 50912, '0.8 mg/dL', '0.5', '1.2'),
       ('24078130', 51301, '16.1 K/uL', '4.0', '11.0'),
       ('23919775', 50861, '33.0 IU/L', '0.0', '40.0'),
       ('27022201', 51301, '25.3 K/uL', '4.0', '11.0'),
       ('24238743', 51006, '16.0 mg/dL', '6.0', '20.0'),
       ('26687335', 50912, '2.1 mg/dL', '0.5', '1.2'),
       ('21409557', 51301, '12.5 K/uL', '4.0', '11.0'),
       ('25514003', 50861, '21.0 IU/L', '0.0', '40.0'),
       ('27553284', 50910, '392.0 IU/L', '26.0', '140.0'),
       ('29328838', 51301, '21.7 K/uL', '4.0', '10.0'),
       ('20535755', 50863, '168.0 IU/L', '40.0', '130.0'),
       ('27361644', 51301, '15.3 K/uL', '4.0', '10.0'),
       ('24019757', 50912, '6.2 mg/dL', '0.5', '1.2'),
       ('28694648', 51301, '16.0 K/uL', '4.0', '11.0'),
       ('27850323', 50956, '1342.0 IU/L', '0.0', '60.0'),
       ('21166109', 50861, '185.0 IU/L', '0.0', '40.0'),
       ('29897948', 50931, '147.0 mg/dL', '70.0', '100.0');


INSERT INTO radiologyreport (clinicalcase_id, noteid, modality, region, examname, text)
VALUES ('20890008', '10040626-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Appendix measures up to 9 mm, likely representing appendicitis with mucosal hyperenhancement.'),
       ('27892518', '10070247-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Enlarged and fluid-filled appendix measuring up to 2.1 cm with surrounding fat stranding.'),
       ('21807759', '10073256-RR', 'CT', 'Abdomen', 'CT ABDOMEN W/CONTRAST',
        'Hyperemic dilated appendix (16 mm) with a proximal appendicolith and cecal wall thickening.'),
       ('20345216', '10080679-RR', 'Ultrasound', 'Abdomen', 'US ABD LIMIT',
        'Noncompressible tubular structure (1.3 cm) likely the appendix with heterogeneous debris.'),
       ('24078130', '10085111-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Appendix dilated to 9 mm, fluid filled, periappendiceal stranding.'),
       ('23919775', '10139369-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        '11 mm dilated fluid-filled appendix with hyperemic walls, findings compatible with appendicitis.'),
       ('27022201', '10154376-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Dilated appendix measures 12 mm with hyperdense focus at distal aspect (appendicolith).'),
       ('24238743', '10156068-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Appendix dilated to 8 mm, fluid filled with hyperenhancing wall, consistent with appendicitis.'),
       ('26687335', '10185476-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS W/O CONTRAST',
        'Appendix mildly dilated to 9 mm with surrounding fat stranding and cecal thickening.'),
       ('21409557', '10205542-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Appendix dilated up to 9 mm with enhancing wall and adjacent fat stranding.'),
       ('25514003', '10071659-RR', 'CT', 'Abdomen', 'CT ABDOMEN W/CONTRAST',
        'Gallbladder appears normal. Round lesion (18x19 mm) likely a prominent pancreatic lobule.'),
       ('27553284', '10069551-RR', 'Ultrasound', 'Abdomen', 'US LIVER/GALLBLADDER',
        'Gallbladder contracted with several small shadowing stones. No wall edema.'),
       ('29328838', '10058856-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS W/O CONTRAST',
        'Sigmoid wall thickening sequelae from chronic diverticular disease. 19 mm renal cyst.'),
       ('20535755', '10056223-RR', 'Ultrasound', 'Abdomen', 'US LIVER/GALLBLADDER',
        'Hyperechoic area in segment VI (previous RFA site). CBD measures 5 mm, enlarged spleen (14 cm).'),
       ('27361644', '10052530-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Retrocecal appendix is fluid-filled with hyperemic and thickened wall concerning for early appendicitis.'),
       ('24019757', '10046241-RR', 'CT', 'Abdomen', 'CT ABD W&W/O C',
        'Large non-enhancing area in pancreatic head compatible with necrotizing pancreatitis.'),
       ('28694648', '10042037-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Extensive fat stranding from sigmoid mesocolon with extraluminal gas, secondary to perforated diverticulitis.'),
       ('27850323', '10040056-RR', 'MRCP', 'Abdomen', 'MR ABD W&W/OC',
        'Expansion of pancreatic tail compatible with pancreatitis with necrosis and hemorrhage.'),
       ('21166109', '10034272-RR', 'CT', 'Abdomen', 'CT ABDOMEN W/CONTRAST',
        'Gallbladder wall thick (6 mm) with surrounding fat stranding concerning for cholecystitis.'),
       ('29897948', '10031940-RR', 'CT', 'Abdomen', 'CT ABD & PELVIS WITH CONTRAST',
        'Large non-calcified gallstone (2.5x2 cm) in a distended gallbladder. Hepatic steatosis.');


INSERT INTO expert_laboratory (expert_id, labtest_id)
SELECT 'USR-EXP-001', id
FROM laboratorytest
WHERE id <= 10;
INSERT INTO expert_laboratory (expert_id, labtest_id)
SELECT 'USR-EXP-002', id
FROM laboratorytest
WHERE id > 10;

INSERT INTO expert_radiology (expert_id, radiology_report_id)
SELECT 'USR-EXP-001', id
FROM radiologyreport
WHERE id <= 10;
INSERT INTO expert_radiology (expert_id, radiology_report_id)
SELECT 'USR-EXP-002', id
FROM radiologyreport
WHERE id > 10;



INSERT INTO expert_clinical_case_management (expert_id, case_id)
VALUES ('USR-EXP-001', '27892518'),
       ('USR-EXP-001', '21807759'),
       ('USR-EXP-001', '20345216'),
       ('USR-EXP-001', '24078130'),
       ('USR-EXP-001', '23919775'),
       ('USR-EXP-001', '27022201'),
       ('USR-EXP-001', '24238743'),
       ('USR-EXP-001', '26687335'),
       ('USR-EXP-001', '21409557'),
       ('USR-EXP-001', '25514003'),
       ('USR-EXP-002', '27553284'),
       ('USR-EXP-002', '29328838'),
       ('USR-EXP-002', '20535755'),
       ('USR-EXP-002', '27361644'),
       ('USR-EXP-002', '24019757'),
       ('USR-EXP-002', '28694648'),
       ('USR-EXP-002', '20890008'),
       ('USR-EXP-002', '27850323'),
       ('USR-EXP-002', '21166109'),
       ('USR-EXP-002', '29897948');

INSERT INTO expert_virtual_patient_management (expert_id, virtual_id)
VALUES ('USR-EXP-001', '10070247'),
       ('USR-EXP-001', '10073256'),
       ('USR-EXP-001', '10080679'),
       ('USR-EXP-001', '10085111'),
       ('USR-EXP-001', '10139369'),
       ('USR-EXP-001', '10154376'),
       ('USR-EXP-001', '10156068'),
       ('USR-EXP-001', '10185476'),
       ('USR-EXP-001', '10205542'),
       ('USR-EXP-001', '10071659'),
       ('USR-EXP-002', '10069551'),
       ('USR-EXP-002', '10058856'),
       ('USR-EXP-002', '10056223'),
       ('USR-EXP-002', '10052530'),
       ('USR-EXP-002', '10046241'),
       ('USR-EXP-002', '10042037'),
       ('USR-EXP-002', '10040626'),
       ('USR-EXP-002', '10040056'),
       ('USR-EXP-002', '10034272'),
       ('USR-EXP-002', '10031940');

-- ==========================
-- KNOWLEDGE RESOURCES
-- ==========================

INSERT INTO knowledge_resources
(id,
 title,
 content,
 link,
 imageUrl,
 authorlist)
VALUES ('KR-001',
        'Clinical Reasoning Fundamentals',
        '
        <h2>Overview</h2>
        
        <p>
        Clinical reasoning is the structured process clinicians use to collect information,
        form diagnostic hypotheses, evaluate evidence, and determine the most appropriate next step.
        Strong reasoning improves diagnostic accuracy and reduces unnecessary intervention.
        </p>
        
        <h2>Hypothesis Generation</h2>
        
        <p>
        Experienced clinicians rarely begin from zero.
        Initial observations immediately activate mental models and possible explanations.
        These early impressions should remain flexible and continuously updated.
        </p>
        
        <h2>Data Collection</h2>
        
        <p>
        Patient history remains one of the highest value diagnostic tools.
        Questions should narrow possibilities rather than accumulate unrelated information.
        </p>
        
        <h2>Verification</h2>
        
        <p>
        Every working diagnosis requires active confirmation and disconfirmation.
        Clinicians should intentionally search for findings that challenge assumptions.
        </p>
        
        <h2>Cognitive Bias</h2>
        
        <p>
        Anchoring, premature closure, and confirmation bias remain frequent contributors to diagnostic error.
        Structured reflection improves decision quality.
        </p>
        ',
        'https://latee.com/resources/clinical-reasoning',
        '/images/das2.jpeg',
        'Emily Carter'),

       ('KR-002',
        'Patient Communication Essentials',
        '
        <h2>Communication Foundations</h2>
        
        <p>
        Effective patient communication builds trust and directly improves outcomes.
        Patients who understand care plans demonstrate stronger adherence and satisfaction.
        </p>
        
        <h2>Opening the Conversation</h2>
        
        <p>
        A strong opening encourages patients to explain concerns in their own words before directed questioning.
        </p>
        
        <h2>Active Listening</h2>
        
        <p>
        Listening requires verbal and nonverbal attention.
        Interrupting too early often reduces diagnostic quality.
        </p>
        
        <h2>Empathy in Practice</h2>
        
        <p>
        Empathy is demonstrated through acknowledgement, clarification, and respectful language.
        </p>
        
        <h2>Difficult Conversations</h2>
        
        <p>
        High emotion situations benefit from slower pacing, summarization, and shared understanding.
        </p>
        ',
        'https://latee.com/resources/patient-communication',
        '/images/das2.jpg',
        'Tachibana Hana'),

       ('KR-003',
        'Emergency Assessment Checklist',
        '
        <h2>Immediate Stabilization</h2>
        
        <p>
        Emergency assessment begins with identifying life threats before detailed diagnosis.
        </p>
        
        <h2>Primary Survey</h2>
        
        <p>
        Airway, breathing, circulation, disability, and exposure remain the standard sequence.
        </p>
        
        <h2>Rapid Information Gathering</h2>
        
        <p>
        Focused history and targeted examination should happen simultaneously.
        </p>
        
        <h2>Escalation Criteria</h2>
        
        <p>
        Clinicians should recognize deteriorating signs early and activate escalation pathways.
        </p>
        
        <h2>Documentation</h2>
        
        <p>
        Clear documentation improves continuity and reduces handoff failure.
        </p>
        ',
        'https://latee.com/resources/emergency-checklist',
        '/images/das3.jpg',
        'Andrew Nguyen'),

       ('KR-004',
        'Virtual Patient Simulation Handbook',
        '
        <h2>Introduction</h2>
        
        <p>
        Simulation environments provide a safe setting for repeated practice.
        </p>
        
        <h2>Learning Design</h2>
        
        <p>
        Scenarios should align with explicit learning outcomes.
        </p>
        
        <h2>Debriefing</h2>
        
        <p>
        Reflection after simulation contributes more learning value than scenario completion alone.
        </p>
        
        <h2>Feedback Loops</h2>
        
        <p>
        Immediate actionable feedback supports improvement.
        </p>
        
        <h2>Measurement</h2>
        
        <p>
        Performance metrics should emphasize reasoning and process.
        </p>
        ',
        'https://latee.com/resources/virtual-patient',
        '/images/das2.jpeg',
        'Le Minh Duc'),

       ('KR-005',
        'Evidence-Based Clinical Decision Making',
        '
        <h2>Principles</h2>
        
        <p>
        Evidence-based practice combines literature, expertise, and patient context.
        </p>
        
        <h2>Searching Evidence</h2>
        
        <p>
        Efficient searching starts with focused clinical questions.
        </p>
        
        <h2>Appraisal</h2>
        
        <p>
        Quality assessment determines reliability and applicability.
        </p>
        
        <h2>Applying Results</h2>
        
        <p>
        Clinical decisions require adaptation to patient goals.
        </p>
        
        <h2>Continuous Learning</h2>
        
        <p>
        Updating evidence supports long-term quality improvement.
        </p>
        ',
        'https://latee.com/resources/ebm',
        '/images/das2.jpg',
        'Emily Carter'),

       ('KR-006',
        'EPA Assessment Framework',
        '
        <h2>EPA Overview</h2>
        
        <p>
        Entrustable Professional Activities evaluate readiness for real clinical responsibility.
        </p>
        
        <h2>Assessment Levels</h2>
        
        <p>
        Progression reflects increasing independence.
        </p>
        
        <h2>Observation</h2>
        
        <p>
        Direct observation creates stronger assessment quality.
        </p>
        
        <h2>Feedback</h2>
        
        <p>
        Specific and timely feedback accelerates improvement.
        </p>
        
        <h2>Implementation</h2>
        
        <p>
        EPA systems require alignment with curriculum and supervision.
        </p>
        ',
        'https://latee.com/resources/epa',
        '/images/das3.jpg',
        'Medical Education Board');

-- ==========================
-- EXPERT_RESOURCE
-- (expert_knowledge)
-- ==========================

INSERT INTO expert_knowledge
(expert_id,
 knowledge_resource_id)
VALUES ('USR-EXP-001', 'KR-002'),

       ('USR-EXP-002', 'KR-003'),

       ('USR-EXP-003', 'KR-001'),
       ('USR-EXP-003', 'KR-005'),

       ('USR-EXP-004', 'KR-004'),
       ('USR-EXP-004', 'KR-006');
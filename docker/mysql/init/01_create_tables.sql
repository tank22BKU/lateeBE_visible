CREATE TABLE patients (
    patientid VARCHAR(50) PRIMARY KEY,
    clinical_case_id VARCHAR(50) NOT NULL, 
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
    description TEXT,
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
    patientid VARCHAR(10) NOT NULL,
    title TEXT,
    type VARCHAR(50),
    description TEXT,
    symptom TEXT,
    medicalhistory TEXT,
    pe TEXT,
    status VARCHAR(10) DEFAULT 'active',
    createdBy VARCHAR(50),
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_patient
        FOREIGN KEY (patientid)
        REFERENCES patients(patientid)
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
    CONSTRAINT fk_lab_case
        FOREIGN KEY (clinicalcaseid)
        REFERENCES clinicalcases(clinicalcaseid),
    CONSTRAINT fk_lab_item
        FOREIGN KEY (itemid)
        REFERENCES labtestitem(itemid)
);

CREATE TABLE radiologyreport (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clinicalcaseid VARCHAR(20) NOT NULL,
    noteid VARCHAR(20),
    modality ENUM('CT','Ultrasound','Radiograph','Drainage','MRI','MRCP','ERCP'),
    region VARCHAR(50),
    examname TEXT,
    text TEXT,
    CONSTRAINT fk_radio_case
        FOREIGN KEY (clinicalcaseid)
        REFERENCES clinicalcases(clinicalcaseid)
);

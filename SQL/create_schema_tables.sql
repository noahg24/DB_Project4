CREATE SCHEMA `my_patient`;

USE my_patient;

CREATE TABLE InsuranceNetwork (
    i_insname VARCHAR(100) PRIMARY KEY
);


CREATE TABLE Therapist (
    t_theraid VARCHAR(6) PRIMARY KEY,
    t_theraname VARCHAR(100) NOT NULL
);


CREATE TABLE Patient (
    p_patid VARCHAR(8) PRIMARY KEY,
    p_patname VARCHAR(100) NOT NULL,
    p_dob DATE NOT NULL,
    p_insname VARCHAR(100),
    p_inspol VARCHAR(30),

    CONSTRAINT uq_patient_identity
        UNIQUE (p_patname, p_dob, p_inspol)
);


CREATE TABLE ListSkills (
    l_skillid VARCHAR(4) PRIMARY KEY,
    l_skilldesc TEXT NOT NULL
);


CREATE TABLE TherapistSkills (
    ts_skillid VARCHAR(4),
    ts_theraid VARCHAR(6),

    PRIMARY KEY (ts_skillid, ts_theraid),

    CONSTRAINT fk_ts_skill
        FOREIGN KEY (ts_skillid)
        REFERENCES ListSkills(l_skillid),

    CONSTRAINT fk_ts_therapist
        FOREIGN KEY (ts_theraid)
        REFERENCES Therapist(t_theraid)
);


CREATE TABLE SkillTreatcodeMap (
    st_skillid VARCHAR(4),
    st_treatcode VARCHAR(7),

    PRIMARY KEY (st_skillid, st_treatcode),

    CONSTRAINT uq_treatcode UNIQUE (st_treatcode),

    CONSTRAINT fk_st_skill
        FOREIGN KEY (st_skillid)
        REFERENCES ListSkills(l_skillid)
);


CREATE TABLE Treatment (
    tr_treatid INT AUTO_INCREMENT PRIMARY KEY,
    tr_theraid VARCHAR(6) NOT NULL,
    tr_patid VARCHAR(8) NOT NULL,
    tr_startdate DATE NOT NULL,
    tr_enddate DATE,
    tr_treatcode VARCHAR(7) NOT NULL,

    CONSTRAINT fk_tr_therapist
        FOREIGN KEY (tr_theraid)
        REFERENCES Therapist(t_theraid),

    CONSTRAINT fk_tr_patient
        FOREIGN KEY (tr_patid)
        REFERENCES Patient(p_patid),

    CONSTRAINT fk_tr_treatcode
        FOREIGN KEY (tr_treatcode)
        REFERENCES SkillTreatcodeMap(st_treatcode)
);


CREATE TABLE PatientSession (
    ps_sessid VARCHAR(7) PRIMARY KEY,
    ps_sessdate DATE NOT NULL,
    ps_patid VARCHAR(8) NOT NULL,
    ps_sessnotes TEXT,
    ps_theraid VARCHAR(6) NOT NULL,
    ps_treatcode VARCHAR(7) NOT NULL,
    ps_treatid INT NOT NULL,

    CONSTRAINT fk_ps_patient
        FOREIGN KEY (ps_patid)
        REFERENCES Patient(p_patid),

    CONSTRAINT fk_ps_therapist
        FOREIGN KEY (ps_theraid)
        REFERENCES Therapist(t_theraid),

    CONSTRAINT fk_ps_treatcode
        FOREIGN KEY (ps_treatcode)
        REFERENCES SkillTreatcodeMap(st_treatcode),

    CONSTRAINT fk_ps_treatment
        FOREIGN KEY (ps_treatid)
        REFERENCES Treatment(tr_treatid)
);


CREATE TABLE Accounting (
    a_txid VARCHAR(20) PRIMARY KEY,
    a_sessid VARCHAR(7) NOT NULL,
    a_txdate DATE NOT NULL,
    a_baldue NUMERIC(10,2) NOT NULL,
    a_amtcolld NUMERIC(10,2) NOT NULL,
    a_payer VARCHAR(100) NOT NULL,

    CONSTRAINT fk_a_session
        FOREIGN KEY (a_sessid)
        REFERENCES PatientSession(ps_sessid)
);

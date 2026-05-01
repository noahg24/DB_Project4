----------------------------------------------------------------------
--------------------------Payments Made By Who?-----------------------
----------------------------------------------------------------------

-- [1]NUMBER OF INSURANCE PAYERS, OUT-NETWORK
SELECT*
FROM Accounting
WHERE a_payer NOT IN (
		SELECT i_insname
		FROM InsuranceNetwork
		)
	AND a_payer NOT IN (
		SELECT p_patid
		FROM Patient
		);

-- [2]NUMBER OF INSURANCE PAYERS, IN-NETWORK
SELECT*
FROM Accounting
WHERE a_payer IN (
		SELECT i_insname
		FROM InsuranceNetwork
		);

-- [3]NUMBER OF SELF-PAY PAYERS
SELECT*
FROM Accounting
WHERE a_payer IN (
		SELECT p_patid
		FROM Patient
		);

--------------------------------------------------------------------
-----------------------------Treatment------------------------------
--------------------------------------------------------------------

-- [4]NUMBER OF TREATMENTS PER THERAPIST
SELECT tr_patid, COUNT(*) AS treatment_count
FROM Treatment
GROUP BY tr_patid
ORDER BY treatment_count DESC;


-- [5]TOP 5 PATIENTS WITH HIGHEST NUMBER OF TREATMENTS
SELECT tr_patid, COUNT(*) AS treatment_count
FROM Treatment
GROUP BY tr_patid
ORDER BY treatment_count DESC
LIMIT 5;

-- [6]NUMBER OF ON-GOING TREATMENTS
SELECT DISTINCT tr_patid AS treatment_ongoing
FROM Treatment
WHERE tr_enddate IS NULL;

-- [7]AVERAGE NUMBER OF TREATMENTS PER PATIENT
SELECT AVG(treatment_count::numeric) AS avg_treatments_per_patient
FROM (
	SELECT
		tr_patid,
        COUNT(*) AS treatment_count
    FROM Treatment
    GROUP BY tr_patid
	);


-- [8]PEAK MONTHS FOR TREATMENT
SELECT
	TO_CHAR(tr_startdate, 'Mon YYYY') AS month,
	COUNT(*) AS treatments_started
FROM Treatment
GROUP BY
	TO_CHAR(tr_startdate, 'Mon YYYY'),
	DATE_TRUNC('month', tr_startdate)
ORDER BY treatments_started DESC;


-- [9]NUMBER OF INCOMPLETE TREAMENTS
SELECT t1.tr_patid
FROM Treatment t1, Treatment t2
WHERE t1.tr_patid = t2.tr_patid
	AND t1.tr_treatid <> t2.tr_treatid
	AND t1.tr_startdate < t2.tr_enddate
	AND t2.tr_startdate < t1.tr_startdate;

-- [10]PATIENTS WHO NEVER PURSUED TREATMENT
SELECT p_patid
FROM Patient
WHERE p_patid NOT IN (
	SELECT ps_patid
	FROM PatientSession
	)

----------------------------------------------------------------------------
-- Therapist Metrics (introduce user interface, search metrics by theraid --
----------------------------------------------------------------------------

-- [11]NUMBER OF TREATMENTS PER THERAPIST, LIFETIME
SELECT tr_theraid, COUNT(*) AS treatment_count
FROM Treatment
GROUP BY tr_theraid
ORDER BY treatment_count DESC;

-- [12]NUMBER OF OPEN/INCOMPLETE TREATMENT CASES BY THERAPIST
SELECT tr_theraid, COUNT(*) AS treatment_count
FROM Treatment
WHERE tr_enddate IS NULL
GROUP BY tr_theraid
ORDER BY treatment_count DESC;

-- [13]NUMBER OF SESSIONS, LIFETIME
SELECT ps_theraid, COUNT(*) AS session_count
FROM PatientSession
GROUP BY ps_theraid
ORDER BY session_count DESC

-- [14]AVERAGE SESSIONS PER THERAPIST
SELECT AVG(session_count) AS avg_sess_per_thera
FROM(
	SELECT COUNT(*) AS session_count
	FROM PatientSession
	GROUP BY ps_theraid
	);

-- [15]CONFIRM, THERAPIST UNQUALIFIED FOR SESSION (does not count first session)
SELECT
    PatientSession.ps_sessid,
    PatientSession.ps_theraid,
    PatientSession.ps_treatcode,
    LEFT(PatientSession.ps_treatcode, 4) AS required_skillid,
    'NOT_QUALIFIED' AS status
FROM PatientSession,
     Treatment
WHERE PatientSession.ps_treatid = Treatment.tr_treatid
	AND PatientSession.ps_sessdate <> Treatment.tr_startdate
	AND NOT EXISTS (
		SELECT 1
      	FROM TherapistSkills
      	WHERE TherapistSkills.ts_theraid = PatientSession.ps_theraid
        AND TherapistSkills.ts_skillid = LEFT(PatientSession.ps_treatcode, 4)
		);

-- [16]NUMBER OF SKILLS PER THERAPIST
SELECT ts_theraid, COUNT(*) AS num_skills
FROM TherapistSkills
GROUP BY ts_theraid
ORDER BY num_skills DESC;


-- [17]AVERAGE NUMBER OF SKILLS PER THERAPIST
SELECT AVG(num_skills) AS avg_num_skills
FROM (
	SELECT COUNT(*) AS num_skills
	FROM TherapistSkills
	GROUP BY ts_theraid
	ORDER BY num_skills DESC
	);

-- [18]NUMBER OF TREATCODES
SELECT tr_treatcode, COUNT(*) AS usage_count
FROM Treatment
GROUP BY tr_treatcode
ORDER BY usage_count DESC;

-- [19]NUMBER OF THERAPISTS PER TREATMENT
SELECT
	ps_treatid,
	COUNT(DISTINCT ps_theraid) AS num_therapists,
	COUNT(ps_theraid) AS num_sessions 
FROM PatientSession
GROUP BY ps_treatid
ORDER BY num_therapists DESC;



-- [20]AVERAGE NUMBER OF THERAPISTS PER TREATMENT
SELECT AVG(therapist_count)
FROM(
	SELECT ps_treatid, COUNT(DISTINCT ps_theraid) AS therapist_count
	FROM PatientSession
	GROUP BY ps_treatid
	HAVING COUNT(DISTINCT ps_theraid) > 1
	);

-- [21]AVERAGE NUMBER OF SESSIONS PER TREATMENT
SELECT AVG(session_count) AS avg_num_sess
FROM (
   		SELECT ps_treatid, COUNT(*) AS session_count
   		FROM PatientSession
   		GROUP BY ps_treatid
	);


------------------------------------------------------------------------
--Generated Revenue (introduce user interface, search metrics by date)--
------------------------------------------------------------------------

-- [22]REVENUE, LIFETIME
SELECT SUM(a_amtcolld) AS total_collected
FROM Accounting;

-- [23]REVENUE, ANNUAL
SELECT SUM(a_amtcolld) AS total_collected
FROM Accounting
WHERE a_txdate BETWEEN '2023-01-01' AND '2024-01-01';

-- [24]REVENUE, MONTHLY
SELECT SUM(a_amtcolld) AS total_collected
FROM Accounting
WHERE a_txdate BETWEEN '2023-01-01' AND '2023-02-01';

-- [25]REVENUE, DAILY
SELECT SUM(a_amtcolld) AS total_collected
FROM Accounting
WHERE a_txdate = '2025-04-20';

---------------------------------------------------------
---------------------UNPAID BALANCE----------------------
---------------------------------------------------------

-- [26]UNPAID BALANCE BY PATIENT, LIFETIME
WITH balance AS (
    SELECT
		a.a_payer,
        SUM(a.a_baldue) AS total_due,
        SUM(a.a_amtcolld) AS total_collected,
        SUM(a.a_baldue) - SUM(a.a_amtcolld) AS outstanding_balance
    FROM Accounting a
    GROUP BY a.a_payer
	)
SELECT
    p.p_patid,
	p.p_patname,
    p.p_insname,
    p.p_inspol,
    b.outstanding_balance,
    CASE
        WHEN LENGTH(b.a_payer) = 8 THEN 'SELF_PAY'
        ELSE 'INSURANCE'
    END AS payer_type
FROM Patient p, balance b
WHERE p.p_patid = b.a_payer
	AND b.outstanding_balance > 0
ORDER BY b.outstanding_balance DESC;

-- [27]UNPAID BALANCE, LIFETIME
SELECT
	a_sessid,
	SUM(a_baldue) AS bal_due,
	SUM(a_amtcolld) AS amt_colld
FROM Accounting
GROUP BY a_sessid
HAVING SUM(a_baldue) != SUM(a_amtcolld)
ORDER BY a_sessid;

-- [28]UNPAID BALANCE, YEAR-END
SELECT ps_sessid AS session_id,
	SUM(a_baldue) AS balance_due,
	SUM(a_amtcolld) AS amount_collected
FROM PatientSession, Accounting
WHERE ps_sessdate >= '2026-01-01'
	AND ps_sessdate < '2027-01-01'
	AND ps_sessid = a_sessid 
GROUP BY ps_sessid
HAVING SUM(a_baldue) != SUM(a_amtcolld);

--[29]UNPAID BALANCE, MONTH-END
SELECT ps_sessid AS session_id,
	SUM(a_baldue) AS balance_due,
	SUM(a_amtcolld) AS amount_collected
FROM PatientSession, Accounting
WHERE ps_sessdate >= '2026-03-01'
	AND ps_sessdate < '2026-04-01'
	AND ps_sessid = a_sessid 
GROUP BY ps_sessid
HAVING SUM(a_baldue) != SUM(a_amtcolld);

-- [30]UNPAID BALANCE, BY-SESSION
SELECT a_sessid, SUM(a_baldue - a_amtcolld) AS balance
FROM Accounting
GROUP BY a_sessid
HAVING SUM(a_baldue - a_amtcolld) > 0
ORDER BY balance DESC;


-----------------------------------------------------------------
---------------------Delay in session payment--------------------
-----------------------------------------------------------------

-- [31]PAYMENT DELAY, AVERAGE
SELECT AVG(tx_delay)
FROM (
   	SELECT MAX(a_txdate) - ps_sessdate AS tx_delay
    FROM PatientSession, Accounting
   	WHERE ps_sessid = a_sessid
    GROUP BY ps_sessid, ps_sessdate
    HAVING SUM(a_baldue) = SUM(a_amtcolld)
	);

-- [32]PAYMENT DELAY, MAX
SELECT MAX(tx_delay)
FROM (
	SELECT MAX(a_txdate) - ps_sessdate AS tx_delay
	FROM PatientSession, Accounting
    WHERE ps_sessid = a_sessid
    GROUP BY ps_sessid, ps_sessdate
	HAVING SUM(a_baldue) = SUM(a_amtcolld)
	);

-------------------------------------------------------------------
------------------Delay in session balance pay-off-----------------
-------------------------------------------------------------------

-- [33]PAY-OFF DELAY, AVERAGE
SELECT AVG(payoff_delay) AS avg_payoff_delay
FROM (
    SELECT
        ps_sessid,
        MAX(a_txdate) - MIN(a_txdate) AS payoff_delay
    FROM PatientSession, Accounting
    WHERE ps_sessid = a_sessid
    GROUP BY ps_sessid
    HAVING SUM(a_baldue) - SUM(a_amtcolld) = 0
	);

-- [35]PAY-OFF DELAY, MAX
SELECT MAX(tx_delay) AS max_payoff_delay
FROM (
    SELECT
        MAX(a.a_txdate) - ps.ps_sessdate AS tx_delay
    FROM PatientSession ps,
         Accounting a
    WHERE ps.ps_sessid = a.a_sessid
    GROUP BY ps.ps_sessid, ps.ps_sessdate
    HAVING SUM(a.a_baldue) = SUM(a.a_amtcolld)
	);
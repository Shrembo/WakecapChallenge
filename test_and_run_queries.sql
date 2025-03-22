-- CREATE DATABASE WakeCapTest;
-- CREATE USER shrembo WITH PASSWORD 'Pg12345!';
-- GRANT ALL PRIVILEGES ON DATABASE WakeCapTest TO shrembo;
-- GRANT USAGE, CREATE ON SCHEMA public TO shrembo;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public
-- GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO shrembo;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE worker TO shrembo;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE zone TO shrembo;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE worker_zone_assignment TO shrembo;
-- GRANT USAGE, SELECT ON SEQUENCE worker_zone_assignment_id_seq TO shrembo;

-- Insert 20K workers
INSERT INTO worker (name, code)
SELECT 'W' || g, 'W' || g
FROM generate_series(50001, 70000) g;

-- Insert 1K zones
INSERT INTO zone (name, code)
SELECT 'Z' || g, 'Z' || g
FROM generate_series(1001, 2000) g;



-- prepare csv for test
-------------------------------
WITH params AS (
  SELECT
    DATE '2025-04-01' AS start_date,
    5 AS num_days
),

eligible_workers AS (
  SELECT code AS worker_code
  FROM worker
  WHERE id BETWEEN 50001 AND 70000
),

eligible_zones AS (
  SELECT code AS zone_code
  FROM zone
  WHERE id BETWEEN 1001 AND 2000
),

random_assignments AS (
  SELECT
    ew.worker_code,
    (SELECT zone_code FROM eligible_zones ORDER BY RANDOM() LIMIT 1) AS zone_code,
    (p.start_date + i) AS assignment_date
  FROM
    params p,
    generate_series(0, (SELECT num_days - 1 FROM params)) AS i,
    eligible_workers ew
)
SELECT
  worker_code AS "Worker Code",
  zone_code AS "Zone Code",
  TO_CHAR(assignment_date, 'YYYY-MM-DD') AS "Assignment Date"
FROM random_assignments
LIMIT 50000;

-------------------------------



SELECT * FROM worker_zone_assignment
WHERE worker_id > 50002
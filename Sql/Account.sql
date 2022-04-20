-- Script to create the account database.

-- Create the database and connect.
CREATE
USER accounts WITH PASSWORD 'changeme';
CREATE
DATABASE accounts;
GRANT ALL PRIVILEGES ON DATABASE
accounts TO accounts;
\connect accounts accounts;

-- Create tables.
CREATE TABLE accounts
(
    username   VARCHAR(14) NOT NULL PRIMARY KEY,
    salt       VARCHAR(32) NOT NULL,
    password   VARCHAR(64) NOT NULL,
    last_world SMALLINT    NOT NULL DEFAULT 0,
    is_banned  BOOLEAN     NOT NULL DEFAULT FALSE
);

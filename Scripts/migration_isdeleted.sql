-- Migration: Add IsDeleted column to usuariologin table
-- Run this script if the database was initialized before this change was added to init.sql

ALTER TABLE usuariologin
    ADD COLUMN IF NOT EXISTS isdeleted BOOLEAN NOT NULL DEFAULT FALSE;

-- Set existing deleted users (if any manual deletions occurred) to FALSE by default
-- If you had physical deletes, those records are gone — no action needed.

-- Verify
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'usuariologin' AND column_name = 'isdeleted';

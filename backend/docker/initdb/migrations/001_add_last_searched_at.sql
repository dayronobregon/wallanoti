-- Migration: Add LastSearchedAt column to alerts table
-- Date: 2026-03-08
-- Description: Adds LastSearchedAt field to track when each alert was last searched,
--              separate from UpdatedAt which tracks entity modifications.

-- Add the column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'alerts' 
        AND column_name = 'LastSearchedAt'
    ) THEN
        ALTER TABLE alerts ADD COLUMN "LastSearchedAt" TIMESTAMP NULL;
        
        -- Optional: Initialize with UpdatedAt for existing records
        -- This ensures existing alerts start tracking from their last known activity
        UPDATE alerts SET "LastSearchedAt" = "UpdatedAt" WHERE "UpdatedAt" IS NOT NULL;
        
        RAISE NOTICE 'Column LastSearchedAt added successfully to alerts table';
    ELSE
        RAISE NOTICE 'Column LastSearchedAt already exists in alerts table';
    END IF;
END $$;

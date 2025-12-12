-- Run this script on DESKTOP-9RRSD5S to add new columns to existing PaymentTransactions table
-- Or run InitialCreate.sql if tables don't exist yet

USE BSEPayments;
GO

-- Check if table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    -- Add CdsNumber column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'CdsNumber')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD CdsNumber NVARCHAR(50) NOT NULL DEFAULT '';
        
        CREATE INDEX IX_PaymentTransactions_CdsNumber ON PaymentTransactions(CdsNumber);
        
        PRINT 'Added CdsNumber column';
    END
    ELSE
    BEGIN
        PRINT 'CdsNumber column already exists';
    END

    -- Add PostedToCashTrans column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'PostedToCashTrans')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD PostedToCashTrans BIT NOT NULL DEFAULT 0;
        
        PRINT 'Added PostedToCashTrans column';
    END
    ELSE
    BEGIN
        PRINT 'PostedToCashTrans column already exists';
    END
END
ELSE
BEGIN
    PRINT 'PaymentTransactions table does not exist. Please run InitialCreate.sql first.';
END
GO

-- Create VerificationAttempts table
CREATE TABLE VerificationAttempts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DocumentPath NVARCHAR(500) NOT NULL,
    SelfiePath NVARCHAR(500) NOT NULL,
    IsVerified BIT NOT NULL,
    ConfidenceScore FLOAT NOT NULL,
    AttemptDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Create index on AttemptDate for faster queries
CREATE INDEX IX_VerificationAttempts_AttemptDate 
ON VerificationAttempts(AttemptDate DESC);

-- Create index on IsVerified for filtering
CREATE INDEX IX_VerificationAttempts_IsVerified 
ON VerificationAttempts(IsVerified);

-- Run once before deploying the hashed OTP/refresh-token implementation.
-- Existing values were stored in plaintext and cannot be safely converted.
-- This invalidates active authentication artifacts and forces fresh issuance.

BEGIN;

UPDATE "IM_UserOtps"
SET "IsUsed" = TRUE,
    "UsedOn" = COALESCE("UsedOn", CURRENT_TIMESTAMP)
WHERE NOT "IsUsed";

UPDATE "IM_RefreshTokens"
SET "IsRevoked" = TRUE,
    "RevokedOn" = COALESCE("RevokedOn", CURRENT_TIMESTAMP),
    "RevokedByIp" = COALESCE("RevokedByIp", 'security-cutover')
WHERE NOT "IsRevoked" AND NOT "IsUsed";

COMMIT;

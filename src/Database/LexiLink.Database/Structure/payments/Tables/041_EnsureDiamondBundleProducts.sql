INSERT INTO payments."PaymentProducts" (
    "Id",
    "StoreProductId",
    "DiamondAmount",
    "IsAppleAvailable",
    "IsGoogleAvailable",
    "SortOrder",
    "IsActive"
)
VALUES
    ('8eb2f973-7024-4b17-a9e8-99445e4f4510', 'diamond_100', 100, TRUE, TRUE, 10, TRUE),
    ('ca3cbf2e-2048-48f3-97ad-5a2e74f6f1b1', 'diamond_550', 550, TRUE, TRUE, 20, TRUE),
    ('b5acfb10-bdb6-4ec2-8781-4e53ddda8232', 'diamond_1200', 1200, TRUE, TRUE, 30, TRUE),
    ('37e980fc-d337-478e-93f7-31e102d74a16', 'diamond_2500', 2500, TRUE, TRUE, 40, TRUE)
ON CONFLICT ("StoreProductId") DO UPDATE
SET
    "DiamondAmount" = EXCLUDED."DiamondAmount",
    "IsAppleAvailable" = EXCLUDED."IsAppleAvailable",
    "IsGoogleAvailable" = EXCLUDED."IsGoogleAvailable",
    "SortOrder" = EXCLUDED."SortOrder",
    "IsActive" = EXCLUDED."IsActive";

ALTER TABLE "stats"."PlayerPeriodStats"
    ALTER COLUMN "PeriodStartDate" TYPE timestamp without time zone
    USING "PeriodStartDate"::timestamp without time zone;

# Database

## Backup

Real database backup (contains actual university data):
https://drive.google.com/file/d/1dUjal1izYT8AN8ZUElrCk-tcyqfdgih0/view?usp=drive_link

To restore:
```sql
RESTORE DATABASE UniversitySchedule
FROM DISK = 'path\to\UniversitySchedule.bak'
WITH REPLACE;
```

## Test Seed Scripts

Run in order on a fresh database after applying EF migrations:

| File | Description |
|------|-------------|
| `01_seed_base.sql` | Initial test data (groups, subjects, lecturers, GSWL records) |
| `02_seed_fix_more_gswl.sql` | Added more GroupSubjectWithLecturer records |
| `03_seed_fix_lecturer_distribution.sql` | Fixed lecturer overload via round-robin distribution |
| `04_seed_fix_lecturer_cap50.sql` | Capped each lecturer at max 50 events |

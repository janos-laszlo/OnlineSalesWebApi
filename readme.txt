setup:
-run /SQL scripts/init.sql, which creates the database and the user with permissions to access it
-apply the configuration instructions in each project that has a readme.txt
-apply the migrations to the database for each DbContext
-run insert car makes and models.sql

notes:
-use romarg, datahost, mioritichost or hostinger for hosting
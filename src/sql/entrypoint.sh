#start SQL Server, start the script to create the DB and import the data, start the app
/opt/mssql/bin/sqlservr & ./setup-data.sh & ./setup-identity.sh & sleep infinity & wait
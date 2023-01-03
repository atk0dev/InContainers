docker stop postgres-db
docker rm postgres-db

docker run -d --name postgres-db --network zabbix-net -h postgres-db -e POSTGRES_PASSWORD="P@ssw0rd!" -e PGDATA=/var/lib/posgresql/13/data -v "c:\work\tmp\zabbix\data":/var/lib/posgresql/13/data -v "c:\work\tmp\zabbix\backup":/var/lib/posgresql/13/backup postgres:13.9

rem docker exec -it postgres-db bash
rem su - postgres
rem psql
rem create user userzabbix password 'P@ssw0rd!';
rem create database dbzabbix with encoding UNICODE template template0 owner=userzabbix connection limit=-1;
rem \c dbzabbix;
rem grant all on schema public to userzabbix;
rem grant all on all tables in schema public to userzabbix;
rem \q



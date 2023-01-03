docker stop zabbix-web
docker rm zabbix-web

docker run --name zabbix-web --network zabbix-net -e DB_SERVER_HOST="postgres-db" -e POSTGRES_USER="userzabbix" -e POSTGRES_PASSWORD="P@ssw0rd!" -e POSTGRES_DB=dbzabbix -e ZBX_SERVER_HOST="zabbix-srv" -e PHP_TZ="Europe/Kyiv" -p 8080:8080 -d zabbix/zabbix-web-nginx-pgsql:6.0-centos-latest

rem docker exec -it zabbix-web bash

rem go to localhost:8080
rem login with Admin|zabbix
docker stop zabbix-srv
docker rm zabbix-srv

docker run --name zabbix-srv --network zabbix-net -e DB_SERVER_HOST="postgres-db" -e POSTGRES_USER="userzabbix" -e POSTGRES_PASSWORD="P@ssw0rd!" -e POSTGRES_DB=dbzabbix -d zabbix/zabbix-server-pgsql:6.0-centos-latest

rem docker exec -it zabbix-srv bash
rem zabbix_server -R config_cache_reload
rem docker exec -it zabbix-srv bash -c "zabbix_server -R config_cache_reload"


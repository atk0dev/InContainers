create user userzabbix password 'P@ssw0rd!';
create database dbzabbix with encoding UNICODE template template0 owner=userzabbix connection limit=-1;
grant all on schema public to userzabbix;
grant all on all tables in schema public to userzabbix;
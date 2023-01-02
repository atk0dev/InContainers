FROM mcr.microsoft.com/mssql/server:2019-latest

ARG PROJECT_DIR=/tmp/globomantics
RUN mkdir -p $PROJECT_DIR
WORKDIR $PROJECT_DIR
COPY sql/InitializeGlobomanticsData.sql ./
COPY sql/InitializeGlobomanticsIdentity.sql ./
COPY sql/wait-for-it.sh ./
COPY sql/entrypoint.sh ./
COPY sql/setup-data.sh ./
COPY sql/setup-identity.sh ./

CMD ["/bin/bash", "entrypoint.sh"]
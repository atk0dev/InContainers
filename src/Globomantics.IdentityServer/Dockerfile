#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Globomantics.IdentityServer/Globomantics.IdentityServer.csproj", "Globomantics.IdentityServer/"]
RUN dotnet restore "Globomantics.IdentityServer/Globomantics.IdentityServer.csproj"
COPY . .
WORKDIR "/src/Globomantics.IdentityServer"
RUN dotnet build "Globomantics.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Globomantics.IdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "Globomantics.IdentityServer.dll"]
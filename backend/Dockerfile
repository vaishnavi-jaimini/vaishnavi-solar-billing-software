FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VaishnaviSolar.Api/*.csproj VaishnaviSolar.Api/
RUN dotnet restore VaishnaviSolar.Api/VaishnaviSolar.Api.csproj

COPY VaishnaviSolar.Api/ VaishnaviSolar.Api/
RUN dotnet publish VaishnaviSolar.Api/VaishnaviSolar.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "VaishnaviSolar.Api.dll"]

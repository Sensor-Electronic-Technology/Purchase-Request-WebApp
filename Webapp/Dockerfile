FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
RUN apt-get update \
  && apt-get install -y curl 
USER $APP_UID
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ./NuGet.Config .
COPY ["Webapp/Webapp.csproj", "Webapp/"]
RUN dotnet restore "Webapp/Webapp.csproj"
COPY . .
WORKDIR "/src/Webapp"
RUN dotnet build "Webapp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Webapp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Webapp.dll"]

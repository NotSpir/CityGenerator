FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CityGeneration.Api.Rest/CityGeneration.Api.Rest.csproj", "CityGeneration.Api.Rest/"]
RUN dotnet restore "CityGeneration.Api.Rest/CityGeneration.Api.Rest.csproj"
COPY . .
WORKDIR "/src/CityGeneration.Api.Rest"
RUN dotnet build "./CityGeneration.Api.Rest.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CityGeneration.Api.Rest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CityGeneration.Api.Rest.dll"]

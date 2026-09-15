FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CityGeneration.Blazor/CityGeneration.Blazor.csproj", "CityGeneration.Blazor/"]
RUN dotnet restore "CityGeneration.Blazor/CityGeneration.Blazor.csproj"
COPY . .
WORKDIR "/src/CityGeneration.Blazor"
RUN dotnet build "./CityGeneration.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CityGeneration.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CityGeneration.Blazor.dll"]

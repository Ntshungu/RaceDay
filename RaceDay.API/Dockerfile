FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
RUN dotnet restore "RaceDay.API/RaceDay.API.csproj"
COPY . .
WORKDIR "/src/RaceDay.API"
RUN dotnet publish "RaceDay.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]

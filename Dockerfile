FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MyCRM.Client/MyCRM.Client.csproj", "MyCRM.Client/"]
COPY ["MyCRM.Infrastructure/MyCRM.Infrastructure.csproj", "MyCRM.Infrastructure/"]
COPY ["MyCRM.Domain/MyCRM.Domain.csproj", "MyCRM.Domain/"]
COPY ["MyCRM.Application/MyCRM.Application.csproj", "MyCRM.Application/"]
RUN dotnet restore "MyCRM.Client/MyCRM.Client.csproj"

COPY . .
WORKDIR "/src/MyCRM.Client"
RUN dotnet build "MyCRM.Client.csproj" -c Release -o /app/build
RUN dotnet publish "MyCRM.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCRM.Client.dll"]

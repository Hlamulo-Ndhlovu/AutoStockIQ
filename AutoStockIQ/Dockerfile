# Prefer the root Dockerfile when deploying from the GitHub repo root on Render.
# This file is kept for local builds from the AutoStockIQ project folder.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AutoStockIQ.csproj", "./"]
RUN dotnet restore "AutoStockIQ.csproj"

COPY . .
RUN dotnet publish "AutoStockIQ.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "AutoStockIQ.dll"]

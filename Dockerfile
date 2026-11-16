# Multi-stage build for Render (repo root = Docker context)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AutoStockIQ/AutoStockIQ.csproj", "AutoStockIQ/"]
RUN dotnet restore "AutoStockIQ/AutoStockIQ.csproj"

COPY AutoStockIQ/. ./AutoStockIQ/
WORKDIR /src/AutoStockIQ
RUN dotnet publish "AutoStockIQ.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT at runtime; Program.cs binds to it.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "AutoStockIQ.dll"]

# Use the official .NET 8 SDK as the base image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["AutoStockIQ.csproj", "./"]
RUN dotnet restore "AutoStockIQ.csproj"

# Copy the rest of the application code
COPY . .
WORKDIR "/src"

# Build the application
RUN dotnet build "AutoStockIQ.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AutoStockIQ.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET Core runtime as the base image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 80 for the application
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "AutoStockIQ.dll"]
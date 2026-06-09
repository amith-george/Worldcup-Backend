# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["WorldCupApi.csproj", "./"]
RUN dotnet restore "./WorldCupApi.csproj"

# Copy the rest of the application code
COPY . .

# Build and publish the application
RUN dotnet publish "WorldCupApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5024

# Set ASP.NET Core to run on port 5024
ENV ASPNETCORE_URLS=http://+:5024

# Copy the published files from the build stage
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "WorldCupApi.dll"]
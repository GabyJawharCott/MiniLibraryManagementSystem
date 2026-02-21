# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MiniLibraryManagementSystem.csproj", "."]
RUN dotnet restore "MiniLibraryManagementSystem.csproj"
COPY . .
RUN dotnet publish "MiniLibraryManagementSystem.csproj" -c Release -o /app/publish --no-restore

# Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

# PORT is set at runtime by Render/Railway; the app reads it in Program.cs

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniLibraryManagementSystem.dll"]

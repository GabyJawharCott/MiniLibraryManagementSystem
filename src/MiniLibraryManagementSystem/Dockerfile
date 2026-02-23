# Build (explicit platform for consistent behavior on Render/x64)
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MiniLibraryManagementSystem.csproj", "."]
RUN dotnet restore "MiniLibraryManagementSystem.csproj"
COPY . .
RUN dotnet publish "MiniLibraryManagementSystem.csproj" -c Release -o /app/publish --no-restore

# Run
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Reduce risk of segfault (139) in constrained environments; PORT is set by Render at runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MiniLibraryManagementSystem.dll"]

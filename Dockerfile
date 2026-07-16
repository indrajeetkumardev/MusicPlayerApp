# Build Stage (SDK 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Runtime Stage (ASP.NET 8.0)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 🔥 Railway के लिए Port Setup
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# 🔥 🔥 🔥 यहाँ अपनी DLL का नाम लिखें (जैसे YourProjectName.dll) 🔥 🔥 🔥
ENTRYPOINT ["dotnet", "MusicBaseApp.dll"]  
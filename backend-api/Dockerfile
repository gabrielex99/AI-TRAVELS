FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /workspace/backend-api

# Copy project file and restore dependencies
COPY AiTravels.Api/*.csproj ./AiTravels.Api/
RUN dotnet restore ./AiTravels.Api/AiTravels.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /workspace/backend-api/AiTravels.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000 5001

ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "AiTravels.Api.dll"]

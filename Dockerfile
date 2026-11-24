# Multi-stage Dockerfile for HotelSaaS Backend (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["backend/HotelSaaS.Domain/HotelSaaS.Domain.csproj", "backend/HotelSaaS.Domain/"]
COPY ["backend/HotelSaaS.Application/HotelSaaS.Application.csproj", "backend/HotelSaaS.Application/"]
COPY ["backend/HotelSaaS.Infrastructure/HotelSaaS.Infrastructure.csproj", "backend/HotelSaaS.Infrastructure/"]
COPY ["backend/HotelSaaS.API/HotelSaaS.API.csproj", "backend/HotelSaaS.API/"]
RUN dotnet restore "backend/HotelSaaS.API/HotelSaaS.API.csproj"

# Copy full source and build
COPY backend/ backend/
WORKDIR "/src/backend/HotelSaaS.API"
RUN dotnet publish "HotelSaaS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HotelSaaS.API.dll"]


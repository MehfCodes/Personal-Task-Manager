# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy project file
COPY ["PTM.API/PTM.API.csproj", "PTM.API/"]
COPY ["PTM.Application/PTM.Application.csproj", "PTM.Application/"]
COPY ["PTM.Infrastructure/PTM.Infrastructure.csproj", "PTM.Infrastructure/"]
COPY ["PTM.Domain/PTM.Domain.csproj", "PTM.Domain/"]
COPY ["PTM.Contracts/PTM.Contracts.csproj", "PTM.Contracts/"]

# Restore dependencies
RUN dotnet restore "PTM.API/PTM.API.csproj"

# build
COPY . .
WORKDIR "/src/PTM.API"
RUN dotnet build "PTM.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "PTM.API.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# config env
ENV ASPNETCORE_ENVIRONMENT=Production
# port
EXPOSE 80

# run
ENTRYPOINT ["dotnet", "PTM.API.dll"]

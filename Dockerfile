# Use official .NET SDK image to build and publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["POS.Api/POS.Api.csproj", "POS.Api/"]
COPY ["POS.Application/POS.Application.csproj", "POS.Application/"]
COPY ["POS.Domain/POS.Domain.csproj", "POS.Domain/"]
COPY ["POS.Infrastructure/POS.Infrastructure.csproj", "POS.Infrastructure/"]
RUN dotnet restore "POS.Api/POS.Api.csproj"

# Copy the entire source and build
COPY . .
WORKDIR "/src/POS.Api"
RUN dotnet build "POS.Api.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "POS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: Use the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port Render uses
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "POS.Api.dll"]

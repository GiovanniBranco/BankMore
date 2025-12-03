# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj", "BankMore.ContaCorrente.API/"]
RUN dotnet restore "BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj"

COPY src/BankMore.ContaCorrente.API/ BankMore.ContaCorrente.API/
WORKDIR "/src/BankMore.ContaCorrente.API"
RUN dotnet build "BankMore.ContaCorrente.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.ContaCorrente.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.ContaCorrente.API.dll"]

# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj", "src/BankMore.Transferencia.API/"]
RUN dotnet restore "src/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj"

COPY . .
WORKDIR "/src/src/BankMore.Transferencia.API"
RUN dotnet build "BankMore.Transferencia.API.csproj" -c Release -o /app/build

# Estágio 2: Publish
FROM build AS publish
RUN dotnet publish "BankMore.Transferencia.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "BankMore.Transferencia.API.dll"]

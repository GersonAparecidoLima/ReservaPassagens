# Estágio 1: Base de Execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Estágio 2: Compilação e Publicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (.csproj) respeitando a estrutura das pastas
COPY ["src/TicketsBooking.API/TicketsBooking.API.csproj", "src/TicketsBooking.API/"]
COPY ["src/TicketsBooking.Application/TicketsBooking.Application.csproj", "src/TicketsBooking.Application/"]
COPY ["src/TicketsBooking.Core/TicketsBooking.Core.csproj", "src/TicketsBooking.Core/"]
COPY ["src/TicketsBooking.Infrastructure/TicketsBooking.Infrastructure.csproj", "src/TicketsBooking.Infrastructure/"]

# Restaura as dependências do NuGet
RUN dotnet restore "src/TicketsBooking.API/TicketsBooking.API.csproj"

# Copia todo o restante dos arquivos do código fonte
COPY . .
WORKDIR "/src/src/TicketsBooking.API"

# Publica a aplicação otimizada para produção
RUN dotnet publish "TicketsBooking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Imagem Final de Execução
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TicketsBooking.API.dll"]
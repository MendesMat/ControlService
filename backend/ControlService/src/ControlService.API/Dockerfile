# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Configurações comuns da solução (ADR-0026) e os projetos de que a API depende, para o restore ficar em cache
COPY ["global.json", "Directory.Build.props", "Directory.Packages.props", ".editorconfig", "./"]
COPY ["src/ControlService.API/ControlService.API.csproj", "src/ControlService.API/"]
COPY ["src/ControlService.Application/ControlService.Application.csproj", "src/ControlService.Application/"]
COPY ["src/ControlService.Domain/ControlService.Domain.csproj", "src/ControlService.Domain/"]
COPY ["src/ControlService.Infrastructure/ControlService.Infrastructure.csproj", "src/ControlService.Infrastructure/"]
COPY ["src/ControlService.ServiceDefaults/ControlService.ServiceDefaults.csproj", "src/ControlService.ServiceDefaults/"]
RUN dotnet restore "./src/ControlService.API/ControlService.API.csproj"
COPY . .
WORKDIR "/src/src/ControlService.API"
RUN dotnet build "./ControlService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ControlService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControlService.API.dll"]

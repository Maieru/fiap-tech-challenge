FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG CHASSIS_VERSION
WORKDIR /source
COPY NuGet.Config Directory.Build.props ./
COPY artifacts/packages/ artifacts/packages/
COPY artifacts/chassis-version.txt artifacts/chassis-version.txt
COPY src/FIAP.TechChallenge.Fase1.API/ src/FIAP.TechChallenge.Fase1.API/
COPY src/FIAP.TechChallenge.Fase1.Application/ src/FIAP.TechChallenge.Fase1.Application/
COPY src/FIAP.TechChallenge.Fase1.Domain/ src/FIAP.TechChallenge.Fase1.Domain/
COPY src/FIAP.TechChallenge.Fase1.Infrastructure/ src/FIAP.TechChallenge.Fase1.Infrastructure/
RUN dotnet publish src/FIAP.TechChallenge.Fase1.API/FIAP.TechChallenge.Fase1.API.csproj -c $BUILD_CONFIGURATION -o /app/publish -p:UseAppHost=false -p:ChassisVersion="${CHASSIS_VERSION:-$(cat artifacts/chassis-version.txt)}"

FROM base AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FIAP.TechChallenge.Fase1.API.dll"]

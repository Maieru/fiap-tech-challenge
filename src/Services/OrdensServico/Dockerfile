FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG CHASSIS_VERSION
WORKDIR /source
COPY NuGet.Config Directory.Build.props ./
COPY artifacts/packages/ artifacts/packages/
COPY artifacts/chassis-version.txt artifacts/chassis-version.txt
COPY src/Services/OrdensServico/ src/Services/OrdensServico/
RUN dotnet publish src/Services/OrdensServico/FIAP.TechChallenge.OS.API/FIAP.TechChallenge.OS.API.csproj -c Release -o /app -p:ChassisVersion="${CHASSIS_VERSION:-$(cat artifacts/chassis-version.txt)}" -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FIAP.TechChallenge.OS.API.dll"]

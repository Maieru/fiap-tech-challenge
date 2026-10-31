FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Services/OrdensServico/ src/Services/OrdensServico/
RUN dotnet publish src/Services/OrdensServico/FIAP.TechChallenge.OS.API/FIAP.TechChallenge.OS.API.csproj -c Release -o /app -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FIAP.TechChallenge.OS.API.dll"]

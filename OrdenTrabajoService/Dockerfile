FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copia todo el repositorio porque OrdenTrabajoService referencia
# proyectos de UsersService (Domain y Framework) con rutas relativas ../
COPY . .
RUN dotnet restore OrdenTrabajoService/OrdenTrabajoService.csproj
RUN dotnet publish OrdenTrabajoService/OrdenTrabajoService.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "OrdenTrabajoService.dll"]

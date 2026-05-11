FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# WebService no tiene referencias a otros proyectos locales,
# solo se copia su propio directorio.
COPY WebService/ WebService/
RUN dotnet restore WebService/WebService.csproj
RUN dotnet publish WebService/WebService.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "WebService.dll"]

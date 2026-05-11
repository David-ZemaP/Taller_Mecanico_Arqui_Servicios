FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy entire repository and restore only the web app project
COPY . .
RUN dotnet restore App/App.csproj

# publish the App project
WORKDIR /src/App
RUN dotnet publish App.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY --from=build /app/publish ./
COPY ./.env ./.env
ENTRYPOINT ["dotnet", "App.dll"]

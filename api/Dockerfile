FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SocialListening.API.csproj ./
RUN dotnet restore SocialListening.API.csproj

COPY . ./
RUN dotnet publish SocialListening.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet SocialListening.API.dll --urls http://0.0.0.0:${PORT:-8080}"]

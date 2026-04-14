FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ClubTreasury.csproj", "./"]
RUN dotnet restore "ClubTreasury.csproj"

COPY . .
RUN dotnet build "ClubTreasury.csproj" -c "$BUILD_CONFIGURATION" -o /app/build -v minimal

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClubTreasury.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER root
RUN apt-get update && apt-get install -y locales-all

ENV LANG=de_DE.UTF-8
ENV LANGUAGE=de_DE:de
ENV LC_ALL=de_DE.UTF-8

WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/exports /app/logs && \
    chmod -R 777 /app/exports /app/logs

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "ClubTreasury.dll"]

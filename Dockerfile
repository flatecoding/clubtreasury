FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TTCCashRegister.csproj", "./"]
RUN dotnet restore "TTCCashRegister.csproj"

COPY . .
RUN dotnet build "TTCCashRegister.csproj" -c $BUILD_CONFIGURATION -o /app/build -v minimal

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TTCCashRegister.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
USER root
RUN apt-get update && apt-get install -y locales-all

ENV LANG=de_DE.UTF-8
ENV LANGUAGE=de_DE:de
ENV LC_ALL=de_DE.UTF-8

WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/exports/Export /app/logs && \
    chmod -R 777 /app/exports/Export /app/logs

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "TTCCashRegister.dll"]

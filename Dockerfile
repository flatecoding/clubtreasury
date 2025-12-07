FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TTCCashRegister.csproj", "./"]
RUN dotnet restore "TTCCashRegister.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./TTCCashRegister.csproj" -c $BUILD_CONFIGURATION -o /app/build  -v minimal

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TTCCashRegister.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TTCCashRegister.dll"]

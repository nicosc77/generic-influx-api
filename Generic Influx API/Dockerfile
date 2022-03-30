FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["mqtt-influx-bridge/mqtt-influx-bridge.csproj", "mqtt-influx-bridge/"]
RUN dotnet restore "mqtt-influx-bridge/mqtt-influx-bridge.csproj"
COPY . .
WORKDIR "/src/mqtt-influx-bridge"
RUN dotnet build "mqtt-influx-bridge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mqtt-influx-bridge.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mqtt-influx-bridge.dll"]

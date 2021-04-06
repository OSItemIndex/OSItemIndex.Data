#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY *.sln .
COPY OSItemIndex.Aggregator.OSRSBox/*.csproj ./OSItemIndex.Aggregator.OSRSBox/
COPY OSItemIndex.AggregateService/*.csproj ./OSItemIndex.AggregateService/
COPY OSItemIndex.API/*.csproj ./OSItemIndex.API/
RUN dotnet restore

COPY OSItemIndex.Aggregator.OSRSBox/. ./OSItemIndex.Aggregator.OSRSBox/
COPY OSItemIndex.AggregateService/. ./OSItemIndex.AggregateService/
COPY OSItemIndex.API/. ./OSItemIndex.API/

WORKDIR "/src/OSItemIndex.Aggregator.OSRSBox"
RUN dotnet build "OSItemIndex.Aggregator.OSRSBox.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OSItemIndex.Aggregator.OSRSBox.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OSItemIndex.Aggregator.OSRSBox.dll"]

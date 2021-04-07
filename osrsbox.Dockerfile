#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/OSItemIndex.Aggregator.OSRSBox/OSItemIndex.Aggregator.OSRSBox.csproj", "src/OSItemIndex.Aggregator.OSRSBox/"]
COPY ["src/OSItemIndex.AggregateService/OSItemIndex.AggregateService.csproj", "src/OSItemIndex.AggregateService/"]
COPY ["src/OSItemIndex.Models/OSItemIndex.Models.csproj", "src/OSItemIndex.Models/"]
RUN dotnet restore "src/OSItemIndex.Aggregator.OSRSBox/OSItemIndex.Aggregator.OSRSBox.csproj"
COPY . .
WORKDIR "/src/src/OSItemIndex.Aggregator.OSRSBox"
RUN dotnet build "OSItemIndex.Aggregator.OSRSBox.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OSItemIndex.Aggregator.OSRSBox.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OSItemIndex.Aggregator.OSRSBox.dll"]

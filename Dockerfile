#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OSItemIndex.Observer/OSItemIndex.Observer.csproj", "OSItemIndex.Observer/"]
COPY ["OSItemIndex.API/OSItemIndex.API.csproj", "OSItemIndex.API/"]
RUN dotnet restore "OSItemIndex.Observer/OSItemIndex.Observer.csproj"
COPY . .
WORKDIR "/src/OSItemIndex.Observer"
RUN dotnet build "OSItemIndex.Observer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OSItemIndex.Observer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OSItemIndex.Observer.dll"]
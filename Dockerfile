###
FROM node:16.9.1-alpine3.14 as build-node

WORKDIR /TarbikMap.LoadTileReplacement
COPY TarbikMap.LoadTileReplacement/package.json .
COPY TarbikMap.LoadTileReplacement/package-lock.json .
RUN npm install
COPY TarbikMap.LoadTileReplacement/ .
RUN npx webpack

WORKDIR /TarbikMap/ClientApp
COPY TarbikMap/ClientApp/package.json .
COPY TarbikMap/ClientApp/package-lock.json .
RUN npm install

WORKDIR /TarbikMap.LoadTileReplacement
RUN node replace.js

WORKDIR /TarbikMap/ClientApp
COPY TarbikMap/ClientApp/ .
RUN GENERATE_SOURCEMAP=false npm run build

###
FROM mcr.microsoft.com/dotnet/sdk:6.0.100 AS build-dotnet

WORKDIR /app
COPY TarbikMap.Common/TarbikMap.Common.csproj TarbikMap.Common/
COPY TarbikMap/TarbikMap.csproj TarbikMap/
COPY TarbikMap.Tests/TarbikMap.Tests.csproj TarbikMap.Tests/
COPY TarbikMap.Tests.Tasks/TarbikMap.Tests.Tasks.csproj TarbikMap.Tests.Tasks/
COPY TarbikMap.Tests.Browser/TarbikMap.Tests.Browser.csproj TarbikMap.Tests.Browser/
COPY TarbikMap.DataGetting.WikiData/TarbikMap.DataGetting.WikiData.csproj TarbikMap.DataGetting.WikiData/
COPY TarbikMap.DataGetting.WikiDataClasses/TarbikMap.DataGetting.WikiDataClasses.csproj TarbikMap.DataGetting.WikiDataClasses/

WORKDIR /app/TarbikMap.Common
RUN dotnet restore

WORKDIR /app/TarbikMap
RUN dotnet restore

WORKDIR /app/TarbikMap.Tests
RUN dotnet restore

WORKDIR /app/TarbikMap.Tests.Tasks
RUN dotnet restore

WORKDIR /app/TarbikMap.Tests.Browser
RUN dotnet restore

WORKDIR /app/TarbikMap.DataGetting.WikiData
RUN dotnet restore

WORKDIR /app/TarbikMap.DataGetting.WikiDataClasses
RUN dotnet restore

WORKDIR /app
COPY TarbikMap.Common/ TarbikMap.Common/
COPY TarbikMap/ TarbikMap/
COPY TarbikMap.Tests/ TarbikMap.Tests/
COPY TarbikMap.Tests.Tasks/ TarbikMap.Tests.Tasks/
COPY TarbikMap.Tests.Browser/ TarbikMap.Tests.Browser/
COPY TarbikMap.DataGetting.WikiData/ TarbikMap.DataGetting.WikiData/
COPY TarbikMap.DataGetting.WikiDataClasses/ TarbikMap.DataGetting.WikiDataClasses/

WORKDIR /app/TarbikMap
ENV SkipWebpack true
RUN dotnet publish -c Release -o out /p:DebugType=None

WORKDIR /app/TarbikMap.Tests
RUN dotnet build -c Release -o out

WORKDIR /app/TarbikMap.Tests.Tasks
RUN dotnet build -c Release -o out

WORKDIR /app/TarbikMap.Tests.Browser
RUN dotnet build -c Release -o out

WORKDIR /app/TarbikMap.DataGetting.WikiData
RUN dotnet build -c Release -o out

WORKDIR /app/TarbikMap.DataGetting.WikiDataClasses
RUN dotnet build -c Release -o out

###
FROM mcr.microsoft.com/dotnet/aspnet:6.0.0 AS out-app

RUN apt-get update && apt-get install -y libgdiplus

WORKDIR /app
COPY --from=build-dotnet /app/TarbikMap/out/ .
COPY --from=build-node /TarbikMap/ClientApp/build ./ClientApp/build
COPY TarbikMap.Resources /app/TarbikMap.Resources

ENTRYPOINT ["dotnet", "TarbikMap.dll"]

###
FROM alpine:3.13.6 AS out-zipper

RUN apk --no-cache add zip

WORKDIR /app
COPY --from=build-dotnet /app/TarbikMap/out/ .
COPY --from=build-node /TarbikMap/ClientApp/build ./ClientApp/build
COPY TarbikMap.Resources /app/TarbikMap.Resources

ENTRYPOINT ["/bin/sh", "-c" , "zip -r /out/TarbikMap_codeOnly.zip . -x TarbikMap.Resources/\\* && zip -r /out/TarbikMap_resourcesOnly.zip TarbikMap.Resources"]

###
FROM mcr.microsoft.com/dotnet/sdk:6.0.100 AS out-tests

WORKDIR /app
COPY --from=build-dotnet /app/TarbikMap.Tests/out/ .
COPY TarbikMap.Resources /app/TarbikMap.Resources

ENTRYPOINT ["dotnet", "vstest", "TarbikMap.Tests.dll", "--logger:trx;LogFileName=/out/unit_tests.xml"]

###
FROM mcr.microsoft.com/dotnet/sdk:6.0.100 AS out-tests-browser

WORKDIR /app
COPY --from=build-dotnet /app/TarbikMap.Tests.Browser/out/ .

ENTRYPOINT ["dotnet", "vstest", "TarbikMap.Tests.Browser.dll", "--logger:trx;LogFileName=/out/unit_tests.xml"]

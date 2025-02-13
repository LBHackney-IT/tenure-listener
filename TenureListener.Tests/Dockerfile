FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./TenureListener.sln ./
COPY ./TenureListener/TenureListener.csproj ./TenureListener/
COPY ./TenureListener.Tests/TenureListener.Tests.csproj ./TenureListener.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./TenureListener/TenureListener.csproj
RUN dotnet restore ./TenureListener.Tests/TenureListener.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out TenureListener/TenureListener.csproj
RUN dotnet build -c debug -o out TenureListener.Tests/TenureListener.Tests.csproj

CMD dotnet test

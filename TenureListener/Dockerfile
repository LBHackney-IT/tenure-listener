FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./TenureListener/TenureListener.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./TenureListener.csproj


# Copy everything else and build
COPY ./TenureListener ./
RUN dotnet build -c Release -o out ./TenureListener.csproj
RUN dotnet publish -c Release -o out ./TenureListener.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/TenureListener.dll

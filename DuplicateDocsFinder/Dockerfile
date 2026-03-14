FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install OpenCV dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libgomp1 \
    libstdc++6 \
    libc6 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 10000
ENTRYPOINT ["dotnet", "DuplicateDocsFinder.dll"]
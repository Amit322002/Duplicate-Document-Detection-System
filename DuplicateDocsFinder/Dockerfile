FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Use Ubuntu 22.04 to match OpenCvSharp4.official.runtime.ubuntu.22.04-x64 native libs
FROM ubuntu:22.04
WORKDIR /app

# Install .NET 8 runtime
RUN apt-get update && apt-get install -y \
    wget \
    ca-certificates \
    && wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y aspnetcore-runtime-8.0 \
    && rm -rf /var/lib/apt/lists/*

# Install OpenCV native dependencies required by OpenCvSharpExtern.so
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libgomp1 \
    libstdc++6 \
    libc6 \
    libglib2.0-0 \
    libgtk2.0-0 \
    libavcodec-dev \
    libavformat-dev \
    libswscale-dev \
    libdc1394-dev \
    libjpeg-dev \
    libpng-dev \
    libtiff-dev \
    libwebp-dev \
    libopenexr-dev \
    libgstreamer1.0-0 \
    libgstreamer-plugins-base1.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 10000
ENTRYPOINT ["dotnet", "DuplicateDocsFinder.dll"]
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["VENFTApp-Server/VENFTApp-Server.csproj", "VENFTApp-Server/"]
COPY ["VEDriversLite/VEDriversLite.csproj", "VEDriversLite/"]
RUN dotnet restore "VENFTApp-Server/VENFTApp-Server.csproj"
COPY . .
WORKDIR "/src/VENFTApp-Server"
RUN dotnet build "VENFTApp-Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VENFTApp-Server.csproj" -c Release -o /app/publish

#--- docfx-----
FROM mono as docs-env

RUN apt update && apt install -y unzip wget

WORKDIR /tools

RUN wget https://github.com/dotnet/docfx/releases/download/v2.56.7/docfx.zip

RUN unzip docfx.zip -d ./docfx

WORKDIR /build

COPY --from=code-env /app/.build .build

COPY . .

RUN mono /tools/docfx/docfx.exe build docfx_project/docfx.json

#--end of docfx


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VENFTApp-Server.dll"]

#helpful commands
#docker build -t venftappserver -f venftapp-server\Dockerfile .
#docker run -d -p 8080:80 --name DockerizedVENFTAppServer venftappserver

# to change the key, GrcOption.ServerAddress in appsettings.json
#docker run -e "GrpcOption:ServerAddress=newurl"  [imagename]

# to override appsettings in docker container from docker host.
#docker run -d --name=DockerizedVENFTAppServerAppSettings -p 8081:80 -v /d/RSqr/CORUZANT/Temp/appsettings.json:/app/appsettings.json venftappserver
#https://docs.docker.com/samples/dotnetcore/
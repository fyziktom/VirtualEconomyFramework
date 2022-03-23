#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["VENFTApp-Server/VENFTApp-Server.csproj", "VENFTApp-Server/"]
COPY ["VEDriversLite/VEDriversLite.csproj", "VEDriversLite/"]
RUN dotnet restore "VENFTApp-Server/VENFTApp-Server.csproj"
#RUN dotnet restore "VEDriversLite/VEDriversLite.csproj"
COPY . .
WORKDIR "/src/VENFTApp-Server"
RUN dotnet build "VENFTApp-Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VENFTApp-Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VENFTApp-Server.dll"]

#docker build -t venftappserver -f venftapp-server\Dockerfile .
#docker run -d -p 8080:80 --name DockerizedVENFTAppServer venftappserver
# to change the key, GrcOption.ServerAddress in appsettings.json
#docker run -e "GrpcOption:ServerAddress=newurl"  [imagename]
#docker run -d --name=DockerizedVENFTAppServerAppSettings -p 8080:80 -v /d/RSqr/CORUZANT/Temp/appsettings.json:/app/appsettings.json venftappserver
#https://docs.docker.com/samples/dotnetcore/
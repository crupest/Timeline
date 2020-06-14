FROM crupest/timeline-build-env:latest AS build
WORKDIR /timeline-app
COPY . .
RUN dotnet publish Timeline/Timeline.csproj --configuration Release --output ./Timeline/publish/ -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /timeline-app/Timeline/publish .
ENTRYPOINT ["dotnet", "Timeline.dll"]

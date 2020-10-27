FROM node:latest AS front-build
WORKDIR /timeline-app
COPY FrontEnd .
RUN yarn build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS back-build
WORKDIR /timeline-app
COPY BackEnd .
COPY --from=front-build /timeline-app/dist /timeline-app/Timeline/ClientApp
RUN dotnet publish Timeline/Timeline.csproj --configuration Release --output ./Timeline/publish/ -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED true
COPY --from=back-build /timeline-app/Timeline/publish .
ENTRYPOINT ["dotnet", "Timeline.dll"]

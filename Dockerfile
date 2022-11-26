FROM node:latest AS front-build
WORKDIR /timeline-app
COPY FrontEnd .
# Install pnpm
RUN npm install -g pnpm
RUN pnpm install --frozen-lockfile && pnpm run build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS back-build
WORKDIR /timeline-app
COPY BackEnd .
COPY --from=front-build /timeline-app/dist /timeline-app/Timeline/ClientApp
RUN dotnet publish Timeline/Timeline.csproj --configuration Release --output ./Timeline/publish/ -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
WORKDIR /app
COPY --from=back-build /timeline-app/Timeline/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Timeline.dll"]

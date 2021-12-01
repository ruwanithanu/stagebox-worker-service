FROM ghcr.io/angloeastern/dotnet/aspnet:5.0 as base

FROM ghcr.io/angloeastern/dotnet/sdk:5.0 as restore
ARG PAT
COPY . .
RUN find . -type f -not -name "*.csproj" -not -name "*.sln" -not -name "NuGet.Config" -delete -print
RUN find . -type d -empty -delete
RUN dotnet restore
COPY . .

FROM restore as publish
RUN dotnet publish "./src/StageBoxWorker/StageBoxWorker.csproj" --no-restore -c Release -o /app

FROM restore as build
CMD  /wait && dotnet test \
    --no-restore \
    -l:trx \
    -r:/coverage \
    -p:CollectCoverage=true \
    -p:CoverletOutputFormat='"opencover,json"' \
    -p:CoverletOutput=/coverage/tests \
    -p:MergeWith=/coverage/tests.json \
    -m:1
      
FROM base as runtime
WORKDIR /app
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StageBoxWorker.dll"]
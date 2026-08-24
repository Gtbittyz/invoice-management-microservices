FROM mcr.microsoft.com/dotnet/sdk:8.0 as BUILD
WORKDIR  /src
COPY EstoqueService.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Realese -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT [ "dotnet", "EstoqueService.dll" ]
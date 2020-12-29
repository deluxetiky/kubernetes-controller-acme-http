FROM  mcr.microsoft.com/dotnet/sdk:5.0  AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore acme-resolver/acme-resolver.csproj
# Copy everything else and build
COPY . ./
RUN dotnet publish acme-resolver/acme-resolver.csproj -c Release -o /app/out 
#Cert password and gpg password is same!

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "acme-resolver.dll"]

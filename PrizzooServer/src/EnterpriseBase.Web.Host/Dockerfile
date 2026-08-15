FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src
COPY ["src/EnterpriseBase.Web.Host/EnterpriseBase.Web.Host.csproj", "src/EnterpriseBase.Web.Host/"]
COPY ["src/EnterpriseBase.Web.Core/EnterpriseBase.Web.Core.csproj", "src/EnterpriseBase.Web.Core/"]
COPY ["src/EnterpriseBase.Application/EnterpriseBase.Application.csproj", "src/EnterpriseBase.Application/"]
COPY ["src/EnterpriseBase.Core/EnterpriseBase.Core.csproj", "src/EnterpriseBase.Core/"]
COPY ["src/EnterpriseBase.EntityFrameworkCore/EnterpriseBase.EntityFrameworkCore.csproj", "src/EnterpriseBase.EntityFrameworkCore/"]
WORKDIR "/src/src/EnterpriseBase.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/EnterpriseBase.Web.Host", "src/EnterpriseBase.Web.Host"]
COPY ["src/EnterpriseBase.Web.Core", "src/EnterpriseBase.Web.Core"]
COPY ["src/EnterpriseBase.Application", "src/EnterpriseBase.Application"]
COPY ["src/EnterpriseBase.Core", "src/EnterpriseBase.Core"]
COPY ["src/EnterpriseBase.EntityFrameworkCore", "src/EnterpriseBase.EntityFrameworkCore"]
WORKDIR "/src/src/EnterpriseBase.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "EnterpriseBase.Web.Host.dll"]

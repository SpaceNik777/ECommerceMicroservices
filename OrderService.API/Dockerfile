FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["OrderService.API/OrderService.API.csproj", "OrderService.API/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
RUN dotnet restore "OrderService.API/OrderService.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "OrderService.API/OrderService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "OrderService.API/OrderService.API.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OrderService.API.dll"] 
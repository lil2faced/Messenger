FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов
COPY ["Messenger.API/Messenger.API.csproj", "Messenger.API/"]
COPY ["Messenger.Application/Messenger.Application.csproj", "Messenger.Application/"]
COPY ["Messenger.Domain/Messenger.Domain.csproj", "Messenger.Domain/"]
COPY ["Messenger.Infrastructure/Messenger.Infrastructure.csproj", "Messenger.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "Messenger.API/Messenger.API.csproj"

# Копируем остальной код
COPY . .

# Сборка
WORKDIR /src/Messenger.API
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app .

# Директория для загрузок
RUN mkdir -p /app/uploads

ENTRYPOINT ["dotnet", "Messenger.API.dll"]
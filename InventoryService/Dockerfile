# Build stage
FROM golang:1.21-alpine AS build
WORKDIR /app

# Установка необходимых зависимостей
RUN apk add --no-cache gcc musl-dev

# Копирование файлов проекта
COPY InventoryService/go.mod InventoryService/go.sum ./
COPY InventoryService/*.go ./

# Загрузка зависимостей и генерация go.sum
RUN go mod download
RUN go mod tidy

# Сборка приложения
RUN go build -o main .

# Final stage
FROM alpine:latest
WORKDIR /app
COPY --from=build /app/main .
EXPOSE 8082
CMD ["./main"] 
# Build stage
FROM golang:1.21-alpine AS build
WORKDIR /app
COPY UserService/go.mod ./
RUN go mod download
COPY UserService/ ./
RUN go build -o main .

# Final stage
FROM alpine:latest
WORKDIR /app
COPY --from=build /app/main .
EXPOSE 8081
CMD ["./main"] 
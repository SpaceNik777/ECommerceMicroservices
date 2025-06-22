# Проверка наличия Minikube
Write-Host "Проверка Minikube..."
minikube status
if ($LASTEXITCODE -ne 0) {
    Write-Host "Запуск Minikube..."
    minikube start --driver=docker
}

# Настройка Docker для использования Minikube
Write-Host "Настройка Docker для Minikube..."
minikube docker-env | Invoke-Expression

# Сборка и загрузка образов
Write-Host "Сборка и загрузка образов..."
Write-Host "Сборка orderservice..."
docker build -t orderservice:latest -f OrderService.API/Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Error "Ошибка сборки orderservice"
    exit 1
}

Write-Host "Сборка userservice..."
docker build -t userservice:latest -f UserService/Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Error "Ошибка сборки userservice"
    exit 1
}

Write-Host "Сборка inventoryservice..."
docker build -t inventoryservice:latest -f InventoryService/Dockerfile .
if ($LASTEXITCODE -ne 0) {
    Write-Error "Ошибка сборки inventoryservice"
    exit 1
}

# Создание namespace
Write-Host "Создание namespace..."
kubectl create namespace ecommerce

# Применение конфигурации
Write-Host "Применение конфигурации..."
kubectl apply -f k8s/config.yaml

# Развертывание сервисов
Write-Host "Развертывание сервисов..."
kubectl apply -f k8s/order-service.yaml
kubectl apply -f k8s/user-service.yaml
kubectl apply -f k8s/inventory-service.yaml

# Ожидание готовности сервисов
Write-Host "Ожидание готовности сервисов..."
Write-Host "Ожидание готовности order-service..."
kubectl wait --for=condition=ready pod -l app=order-service -n ecommerce --timeout=300s
if ($LASTEXITCODE -ne 0) {
    Write-Error "order-service не запустился в течение 5 минут"
    exit 1
}

Write-Host "Ожидание готовности user-service..."
kubectl wait --for=condition=ready pod -l app=user-service -n ecommerce --timeout=300s
if ($LASTEXITCODE -ne 0) {
    Write-Error "user-service не запустился в течение 5 минут"
    exit 1
}

Write-Host "Ожидание готовности inventory-service..."
kubectl wait --for=condition=ready pod -l app=inventory-service -n ecommerce --timeout=300s
if ($LASTEXITCODE -ne 0) {
    Write-Error "inventory-service не запустился в течение 5 минут"
    exit 1
}

# Развертывание Ingress
Write-Host "Развертывание Ingress..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

# Ожидание готовности Ingress
Write-Host "Ожидание готовности Ingress..."
kubectl wait --namespace ingress-nginx `
  --for=condition=ready pod `
  --selector=app.kubernetes.io/component=controller `
  --timeout=120s

# Проброс порта для Ingress
Write-Host "Настройка проброса порта для Ingress..."
Start-Process powershell -ArgumentList "kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 30080:80"

# Вывод информации о доступе
Write-Host "Сервисы доступны по следующим адресам:"
Write-Host "- Order Service: http://localhost:30080/orders"
Write-Host "- User Service: http://localhost:30080/users"
Write-Host "- Inventory Service: http://localhost:30080/inventory"

# Проверка статуса
Write-Host "Проверка статуса развертывания..."
kubectl get pods -n ecommerce

# Ожидание запуска подов
Write-Host "Ожидание запуска подов..."
kubectl wait --for=condition=Ready pods --all -n ecommerce --timeout=180s 
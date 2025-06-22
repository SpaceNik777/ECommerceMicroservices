# Postgres Helm Chart

Рекомендуется использовать официальный Helm-чарт Bitnami:
https://artifacthub.io/packages/helm/bitnami/postgresql

Пример установки:

```
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
helm install postgres bitnami/postgresql -n postgres --create-namespace \
  --set auth.username=postgres \
  --set auth.password=postgres \
  --set auth.database=orderservice
```

Пример values.yaml для ArgoCD:
```yaml
auth:
  username: postgres
  password: postgres
  database: orderservice
primary:
  persistence:
    enabled: true
    size: 1Gi
``` 
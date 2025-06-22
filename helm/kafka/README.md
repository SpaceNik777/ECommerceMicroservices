# Kafka Helm Chart

Рекомендуется использовать официальный Helm-чарт Bitnami:
https://artifacthub.io/packages/helm/bitnami/kafka

Пример установки:

```
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
helm install kafka bitnami/kafka -n kafka --create-namespace \
  --set replicaCount=1 \
  --set zookeeper.enabled=true
```

Пример values.yaml для ArgoCD:
```yaml
replicaCount: 1
zookeeper:
  enabled: true
``` 
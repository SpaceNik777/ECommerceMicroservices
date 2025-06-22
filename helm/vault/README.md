# Vault (bank-vaults) Helm Chart

Рекомендуется использовать официальный Helm-чарт bank-vaults:
https://github.com/bank-vaults/bank-vaults/tree/main/charts/vault

Пример установки:

```
helm repo add banzaicloud-stable https://kubernetes-charts.banzaicloud.com
helm repo update
helm install vault banzaicloud-stable/vault -n vault --create-namespace
```

Пример values.yaml для ArgoCD:
```yaml
vault:
  server:
    dataStorage:
      enabled: true
      size: 1Gi
```

Для интеграции с mutating webhook и автоматической подстановки секретов в окружение контейнеров используйте аннотации:
```yaml
annotations:
  vault.security.banzaicloud.io/vault-addr: "http://vault.vault:8200"
  vault.security.banzaicloud.io/vault-role: "default"
  vault.security.banzaicloud.io/vault-path: "kubernetes"
  vault.security.banzaicloud.io/vault-env: "true"
  vault.security.banzaicloud.io/vault-env-passthrough: "DB_PASSWORD"
``` 
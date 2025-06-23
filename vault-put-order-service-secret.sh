#!/bin/bash
# Скрипт для загрузки секрета order-service в Vault (kv v2)

vault kv put secret/order-service \
  POSTGRES_USER=postgres \
  POSTGRES_PASSWORD=postgres \
  ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=orders;Username=postgres;Password=postgres" 
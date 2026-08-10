# Kubernetes

Este directorio contiene los manifiestos de despliegue del Sistema de Gestión de Licitaciones.

La guía de despliegue y verificación se mantiene en [`docs/kubernetes.md`](../docs/kubernetes.md).

Los manifiestos usan el namespace `licitaciones`. El Secret versionado contiene
solo marcadores y debe configurarse antes de ejecutar `kubectl apply -f k8s/`.

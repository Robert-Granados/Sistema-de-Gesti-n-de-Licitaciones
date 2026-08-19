# Despliegue en Kubernetes

## Requisitos

- Un clúster Kubernetes con una `StorageClass` predeterminada.
- `kubectl` configurado para el clúster objetivo.
- La imagen `licitaciones-app:1.0.0` publicada en un registro accesible o
  cargada en el clúster local.

Para un clúster local puede construir la imagen con:

```powershell
docker build -t licitaciones-app:1.0.0 .
```

La carga local de la imagen se realiza con
`minikube image load licitaciones-app:1.0.0` en Minikube o con
`kind load docker-image licitaciones-app:1.0.0` en Kind.

## Configuración segura

`k8s/app-secret.yaml` contiene exclusivamente marcadores sin credenciales
reales. Antes de desplegar, reemplácelos localmente o gestione el Secret con el
administrador de secretos del clúster. Deben coincidir `POSTGRES_USER`,
`POSTGRES_PASSWORD`, `POSTGRES_DB` y los valores de
`ConnectionStrings__DefaultConnection`.

El repositorio no contiene credenciales reales. Los Secret de Kubernetes
codifican los valores, pero por sí solos no los cifran en etcd; un entorno de
producción requiere cifrado en reposo y control RBAC.

## Aplicación

Una vez configurado el Secret:

```powershell
kubectl apply -f k8s/
kubectl wait --namespace licitaciones --for=condition=complete job/licitaciones-migration --timeout=10m
kubectl rollout status deployment/licitaciones-app --namespace licitaciones --timeout=10m
```

El Job espera a PostgreSQL y ejecuta la imagen con
`Database__MigrationsOnly=true`. Los pods de la aplicación mantienen desactivada
la migración automática y su initContainer no termina hasta encontrar
`20260807150000_HU43LicitacionLifecycleColumns` en
`__EFMigrationsHistory`. Por ello el Service no recibe endpoints listos con un
esquema incompleto.

Una nueva versión de migración requiere actualizar `EXPECTED_MIGRATION`, cambiar
la imagen y recrear el Job:

```powershell
kubectl delete job licitaciones-migration --namespace licitaciones --ignore-not-found
kubectl apply -f k8s/migration-job.yaml
```

## Verificación

```powershell
kubectl get pods,svc,pvc --namespace licitaciones
kubectl get jobs --namespace licitaciones
kubectl logs job/licitaciones-migration --namespace licitaciones
kubectl describe deployment licitaciones-app --namespace licitaciones
kubectl port-forward service/licitaciones-app 8080:80 --namespace licitaciones
```

Con el `port-forward` activo, la verificación esperada es HTTP 200 en
`http://localhost:8080/health`, el PVC en estado `Bound` y los dos pods de la
aplicación en estado `Ready`.

## Eliminación

```powershell
kubectl delete namespace licitaciones
```

La eliminación del namespace también elimina el PVC definido en estos
manifiestos. La conservación de los datos requiere una copia de seguridad
previa.

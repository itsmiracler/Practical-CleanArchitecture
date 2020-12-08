# Solution Structure
![alt text](/docs/imgs/code-solution-structure.png)

# Build & Run Locally using Tye

- Install Tye
  ```
  dotnet tool install -g Microsoft.Tye --version "0.5.0-alpha.20555.1"
  dotnet tool update -g Microsoft.Tye --version "0.5.0-alpha.20555.1"
  ```
  
- Run
  ```
  tye run
  ```
  
- Open Tye Dashboard: http://localhost:8000/

# Build & Deploy to Kubernetes

- Build
  ```
  docker-compose build
  ```

- Tag
  ```
  docker tag classifiedads.backgroundserver phongnguyend/classifiedads.backgroundserver
  docker tag classifiedads.migrator phongnguyend/classifiedads.migrator
  docker tag classifiedads.notificationserver phongnguyend/classifiedads.notificationserver
  docker tag classifiedads.webapi phongnguyend/classifiedads.webapi
  docker tag classifiedads.graphql phongnguyend/classifiedads.graphql
  docker tag classifiedads.blazor phongnguyend/classifiedads.blazor
  docker tag classifiedads.identityserver phongnguyend/classifiedads.identityserver
  docker tag classifiedads.webmvc phongnguyend/classifiedads.webmvc
  ```

- Push
  ```
  docker push phongnguyend/classifiedads.backgroundserver
  docker push phongnguyend/classifiedads.migrator
  docker push phongnguyend/classifiedads.notificationserver
  docker push phongnguyend/classifiedads.webapi
  docker push phongnguyend/classifiedads.graphql
  docker push phongnguyend/classifiedads.blazor
  docker push phongnguyend/classifiedads.identityserver
  docker push phongnguyend/classifiedads.webmvc
  ```

- Apply
  ```
  kubectl apply -f .k8s
  kubectl get all
  kubectl get services
  kubectl get pods
  ```

- Delete
  ```
  kubectl delete -f .k8s
  ```
  
- Use Helm
  ```
  helm install myrelease .helm/monolith
  helm list
  helm upgrade myrelease .helm/monolith
  ```

- UnInstall
  ```
  helm uninstall myrelease
  ```
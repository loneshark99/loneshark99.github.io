---
layout: post
title:  "How to run a Flink Application in Kubernetes"
date:   2025-10-05 13:33:00 PM
categories: Flink, Kubernetes
---

How to run a Flink Application in Kubernetes.

1) Install Minikube on your maachine.

![Minikube](https://loneshark99.github.io/images/StartKubernetesCluster.png "Minikube")

2) Install Flink-Kubernetes Operator as a Custom Resource Definition.
3) Install lens Kubernetes explorer to view the different deployment and interact with the Kubernetes cluster.

   ```bash
      lens-desktop &
   ```
4) Start the Kubernetes cluster.
5) Run the following command. This is a useful step. 

   When you run this command, it outputs environment variables that, when applied to your shell session, redirect Docker commands to use Minikube's internal Docker environment. This enables you to:

    a) Build Docker images directly inside the Minikube VM
    b) Use those images in your Kubernetes pods without pushing to an external registry
    c) Access the same Docker daemon that Kubernetes uses to run containers

  After running this command, any Docker commands you issue (like docker build or docker images) will interact with Minikube's Docker daemon instead of your local one.

    ```bash
      eval $(minikube docker-env)
    ```
6) Build the application using Maven.

  ```bash
    yash@YashDevBox ~/I/flink-app> mvn clean install
  ```
  
7) Build and Load your docker image.

   Create a Dockerfile and copy the application jar you build in the previous step into the streaming folder in flink.

   ```docker
   FROM flink:1.20
   COPY flink-app-0.1.jar /opt/flink/examples/streaming/flink-app-0.1.jar
   ```

   ```bash
      docker build -t yashflink8:latest .
      minikube image load yashflink8:latest
   ```

   ![Build Docker Image](https://loneshark99.github.io/images/BuildAndLoadDockerImage.png "Build Docker Image")

9) Create a Kubernetes deployment for your application.

   Create a deployment.yaml file and make sure you use the same jarURI path as the docker image.

   ```yaml
   apiVersion: flink.apache.org/v1beta1
   kind: FlinkDeployment
   metadata:
     name: yash-example-9
   spec:
     image: yashflink8:latest
     flinkVersion: v1_20
     flinkConfiguration:
       taskmanager.numberOfTaskSlots: 2
     serviceAccount: flink
     jobManager:
       resource:
         memory: "2048m"
         cpu: 1
     taskManager:
       resource:
         memory: "2048m"
         cpu: 1
     job:
       jarURI: file:///opt/flink/examples/streaming/flink-app-0.1.jar
       entryClass: org.example.DataStreamJob
       parallelism: 2
       upgradeMode: stateless
   ```

   Run the following command to create the deployment and use port forwarding to see the Flink Dashboard.
   ```bash
      kubectl create -f /home/yash/IdeaProjects/flink-app/src/main/resources/deployment.yaml
      kubectl port-forward yash-example-9 8081
   ```

10) Check your flink deployment is created and you can view the logs as needed using the UI or from the kubectl logs command.

   ```bash
      kubectl logs -f deploy/basic-example
   ```
![Kubernetes Deployment](https://loneshark99.github.io/images/KubernetesDeployment.png "Kubernetes Deployment")

10) Delete the application from the Custom Resources Definition as sometimes running locally you cant get enough resources if you have too many applications running.


   


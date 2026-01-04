---
layout: post
title: "Kubernetes ClusterId Service"
date: 2026-01-04 13:19:00
categories: [kubernetes]
tags:  [kubernetes]
---

Services in K8s is not pods or containers. They are actually configured in the Linux Kernel network stack using IP Tables and DNS Service.

To see this in action lets deploy a nginx server  which returns the ip address of in the response when a request is sent to it.

## To do this we need to create

1. ConfigMap which will update the **default.conf** file to return the server IP Address. We will map the configMap as a volume in the container.

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: nginx-echo-config
data:
  default.conf: |
    server {
      listen 80;
      server_name localhost;
      location / {
        default_type text/plain;
        return 200 "Hello! I am Pod: \$server_addr\n";
      }
    }
```
2. Create a deployment of nginx server with the volume mounted using the mount path of /etc/nginx/config.d, We will add a tags **spec.template.metadata.labels** to identify the pods and the **spec.selector.matchLabels** for the deployment controller to search the pods with that label.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-echo
spec:
  replicas: 3
  selector:
    matchLabels:
      app: echo-app
  template:
    metadata:
      labels:
        app: echo-app
    spec:
      containers:
        - name: nginx
          image: nginx:alpine
          ports:
            - containerPort: 80
          volumeMounts:
            - name: config-volume
              mountPath: /etc/nginx/conf.d
      volumes:
        - name: config-volume
          configMap:
            name: nginx-echo-config
```


3. We create a **CLUSTERIP** service which resides inside the cluster and can be accessed **only inside the cluster**. It cannot be accessed from outside the cluster.



```mermaid
graph TD
    %% Define Styles to match the image
    classDef red fill:#e74c3c,stroke:#c0392b,color:white;
    classDef blue fill:#2980b9,stroke:#2980b9,color:white;
    classDef yellow fill:#f1c40f,stroke:#f39c12,color:black;
    classDef grey fill:#2c3e50,stroke:#xxxxxx,color:white; 

    %% Nodes
    Debug[Debug Pod / Curl]:::red
    ConfigMap[ConfigMap: nginx-echo-conf]:::yellow
    Service[Service: echo-service<br/>ClusterIP]:::blue

    subgraph Deployment [Nginx Deployment 3 Replicas]
        style Deployment fill:transparent,stroke:#bdc3c7,stroke-dasharray: 5 5,color:white
        Pod1[Pod: nginx-echo-1]:::blue
        Pod2[Pod: nginx-echo-2]:::blue
        Pod3[Pod: nginx-echo-3]:::blue
    end

    %% Relationships
    
    %% 1. Curl Request
    Debug -->|1. curl http://echo-service| Service
    
    %% 2. Load Balancing to Pods
    Service -->|2. Load Balances| Pod1
    Service --> Pod2
    Service --> Pod3

    %% ConfigMap Injection
    ConfigMap -.->|Injected as Volume| Pod1
    ConfigMap -.-> Pod2
    ConfigMap -.-> Pod3

    %% 3. Response
    Pod1 -->|3. Responds: Hello! I am Pod| Debug
```
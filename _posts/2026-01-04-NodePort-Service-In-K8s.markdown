---
layout: post
title: "Kubernetes NodePort Service"
date: 2026-01-04 13:19:00
categories: [kubernetes]
tags:  [kubernetes]
---

# Kubernetes NodePort Service

With NodePort service k8s opens a port on each node and when we access the service from outside using the ip address:port the traffic gets routed to out service.

This is generally used for local testing. It is not very secure because it open a number of port on each node on the cluster.  Difference between NODEPORT and CLUSTERIP  service type is ClusterIP can be only accessed from inside the cluster whereas the NodePORT can be accessed from outside the cluster using the NODEADDRESS:PORT.


The analogy thats given is CLUSTERIP is like Intercom in a office and NODEPORT Is like a phone provided outside the Bldg which can be used to called the internal phones.


The best way to think of different types of services provided by the Kubernetes is

   **ExternalService --> WRAPS  -->  LOAD BALANCER --> WRAPS --> NODEPORT --> WRAPS -->  CLUSTERIP**


The above this is a very important thing to remember. 


To create and test this is similar to the previous post but with some important differences.

```yaml

# ConfigMap.yaml
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

# Application.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: nginx-echo-nodeport
spec:
  replicas: 3
  selector:
    matchLabels:
      app: echo-nodeport
  template:
    metadata:
      labels:
        app: echo-nodeport
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

#Service.yaml
apiVersion: v1
kind: Service
metadata:
  name: echo-nodeport-service
spec:
  type: NodePort  
  selector:
    app: echo-nodeport  
  ports:
    - protocol: TCP
      port: 80        
      targetPort: 80  
      nodePort: 30007 

```

# Architecture Diagram


```mermaid
flowchart TD
    subgraph Cluster[Kubernetes Cluster]
        direction TB
        
        %% Components
        CM[ConfigMap: nginx-echo-config]
        Svc[Service: echo-nodeport-service]
        
        subgraph MyDeployment[Deployment: nginx-echo-nodeport]
            Pod1[Pod: nginx-echo-0]
            Pod2[Pod: nginx-echo-1]
            Pod3[Pod: nginx-echo-2]
        end
        
        %% Connections
        CM -->|Mounts /etc/nginx/conf.d| MyDeployment
        MyDeployment -->|Selects app: echo-nodeport| Svc
    end
    
    %% External/Test
    TestPod[Test Pod: curl] -->|curl http://echo-nodeport-service| Svc
    
    %% Styling
    style CM fill:#fff3e0,stroke:#e65100
    style Svc fill:#e0f2f1,stroke:#00695c
    style MyDeployment fill:#e1f5fe,stroke:#01579b
```


# DataFlow Diagram

```mermaid
sequenceDiagram
    participant User as Client (Curl Pod)
    participant Svc as Service (ClusterIP)
    participant Pod as Pod (Nginx)
    participant CM as ConfigMap (Volume)

    Note over CM, Pod: Initialization Phase
    CM->>Pod: Mounts nginx.conf to /etc/nginx/conf.d
    Pod->>Pod: Nginx starts with config

    Note over User, Pod: Request Phase
    User->>Svc: GET http://echo-nodeport-service (Port 80)
    Svc-->>Pod: Load balances to Pod IP (TargetPort 80)
    Pod-->>User: Returns "Hello! I am Pod: $server_addr"
```
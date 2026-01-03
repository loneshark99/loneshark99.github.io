---
layout: post
title: "Kubernetes Deep Dive and understanding different componets of Kubenetes using Minikube"
date: 2026-01-03 11:04:00
categories: [kubernetes]
tags:  [kubernetes]
---

[Kubernetes_process]:/assets/Kubernetes_Processes.png

To understand the different components of the Kubernetes cluster we need to see what components are current running on the system. I have a minikube instance running and I can ssh into the instance to see all the processes that are running. For this I can run the following command.

```bash
    minikube ssh
    pgrep -l kube
    pgrep -l etcd
```

![components][Kubernetes_process]


## Control Plane (Brain of the cluster)
As you can see that there are 6 main components.  The kube-controller, kube-scheduler, kube-apiserver are part of the **Control Plane** which is the **BRAIN** of the Cluster.

## etcd (Distributed Database of the Cluster)
Along with this another component thats very critical is the **etcd** which is the Key-Value Pair database of the Cluster. This contains all the resource informations like the **pods, nodes, configMaps, Secrets, services, CRD Custom Resource Definition**.

## kubectl (Systemd service)

**kubectl** is the systemd service and this is the first components that starts when the cluster is started. It looks at 
**/etc/kubernetes/manifest** folder and reads the yaml files and creates **static pods** for each of the component mentioned above.

 ```bash
    docker@minikube:~$ sudo ls -laht /etc/kubernetes/manifests
 ```

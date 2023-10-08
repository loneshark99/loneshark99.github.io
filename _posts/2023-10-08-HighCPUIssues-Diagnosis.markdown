---
layout: post
title:  "Diagnosis of High CPU issues in production"
date:   2023-10-07 11:16:00 AM
categories: Debugging
---

[HighCPU]: https://loneshark99.github.io/images/High_CPU.png
[HighCPU_1]: https://loneshark99.github.io/images/Process_1_CPU.png
[HighCPU_2]: https://loneshark99.github.io/images/Process_2_CPU.png
[HighCPU_3]: https://loneshark99.github.io/images/Process_3_CPU.png

Recently I was tasked with identifying the high CPU issues in our production environment. We were seeing issues where the CPU increase was not really following the traffic patterns. This is a shared environment where lot of different processes are running on a single machine. CPU was consistently reaching 100 percent and was affecting the throughput and latency of other processes in this environment.

![alt text][HighCPU]

The first is identifying which processes are consuming high CPU during the peak load and trying to profile those process to see if we can find some bottle neck which are causing this issue. Lot of times, it might be that the CPU is spending lot of time in GC. It maybe due to allocation of Large Object Heap [LOH] or the process is allocating lot of object and keeping reference to them or there may be thread starvation issues etc.

In this particular scenario I found 3 processes which were contributing to the High CPU, lets call them 

 - Process 1  ( CPU Pattern is very unusual and not following the traffic pattern)
 - Process 2  ( IO bound load but CPU is high)
 - Process 3  ( CPU bound load)

 You can see the CPU usage of all 3 processes.

 ![alt text][HighCPU_1]
 ![alt text][HighCPU_2]
 ![alt text][HighCPU_3]

 
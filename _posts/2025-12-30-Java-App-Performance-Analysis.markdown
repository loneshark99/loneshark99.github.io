---
layout: post
title: "Performance analysis in a Java Application"
date: 2025-12-30 14:04:00
categories: Java
---

Lately I have been working on flink java application and the app has been affected by bad performance issue. This got me reading and understanding what tools are provided in Java that can help me understand the backpressure issues.

On the flink dasboard, you get 5 ways you can do this analysis with each tool providing specific insight into the application state.

- Thread Dump
- CPU Profiling Dump to analyze the flame graph.
- Allocation Profiling to analyze the flame graph.
- Metrics and Dashboard which can collected by the Prometheus Collector and shown in the Grafana Dashboard.
- Exception history dashboard.
- Memory metrics provided in the flink dashboard.
- TaskExecutor and JobManager Logs for analysis 


# ThreadDump 

Below is a thread dump example. This is a **point in time** data about each thread in the JVM. It provides information about what the thread is executing and metadata about the thread.

 It shows:
- **Thread states** (RUNNABLE, WAITING, BLOCKED, etc.)
- **Stack traces** for each thread (what method each thread is executing)
- **Lock information** (what locks are held, what threads are waiting for locks)
- **Thread priorities and daemon status**

## Thread States

Understanding thread states is key to analysis:

*   **RUNNABLE**: The thread is potentially executing code. It might be actively using the CPU or waiting for OS resources (like IO).
    *   *High CPU Usage*: Look for many threads in RUNNABLE state with the same stack trace (working on the same logic).
*   **BLOCKED**: The thread is waiting to acquire a lock (monitor) that another thread currently holds.
    *   *Bottleneck*: If many threads are BLOCKED on the same lock, you have a concurrency issue.
*   **WAITING**: The thread is waiting indefinitely for another thread to perform an action (e.g., `Object.wait()`, `Thread.join()`).
    *   *Idle*: Thread pools often have idle threads in this state waiting for tasks.
*   **TIMED_WAITING**: Similar to WAITING, but with a timeout (e.g., `Thread.sleep(1000)`).

```json
{
  "threadInfos": [
    {
      "threadName": "main",
      "stringifiedThreadInfo": "\"main\" Id=1 WAITING on java.util.concurrent.CompletableFuture$Signaller@2993e0bc
        at java.base@17.0.12/jdk.internal.misc.Unsafe.park(Native Method)
        - waiting on java.util.concurrent.CompletableFuture$Signaller@2993e0bc
        at java.base@17.0.12/java.util.concurrent.locks.LockSupport.park(Unknown Source)
        ...
        at app//org.apache.flink.runtime.taskexecutor.TaskManagerRunner.main(TaskManagerRunner.java:475)"
    },
    {
      "threadName": "flink-pekko.actor.default-dispatcher-4",
      "stringifiedThreadInfo": "\"flink-pekko.actor.default-dispatcher-4\" Id=35 RUNNABLE
        at java.management@17.0.12/sun.management.ThreadImpl.dumpThreads0(Native Method)
        at java.management@17.0.12/sun.management.ThreadImpl.dumpAllThreads(Unknown Source)
        at app//org.apache.flink.runtime.util.JvmUtils.createThreadDump(JvmUtils.java:50)
        ...
        at java.base@17.0.12/java.util.concurrent.ForkJoinWorkerThread.run(Unknown Source)"
    },
    {
      "threadName": "Source: Kafka Source - CounterUnifiedEventSlimV2 -> Filter (25/128)#15",
      "stringifiedThreadInfo": "\"Source: Kafka Source - CounterUnifiedEventSlimV2 -> Filter (25/128)#15\" Id=118752 TIMED_WAITING
        at java.base@17.0.12/jdk.internal.misc.Unsafe.park(Native Method)
        at app//org.apache.flink.streaming.runtime.tasks.mailbox.TaskMailboxImpl.take(TaskMailboxImpl.java:149)
        at app//org.apache.flink.streaming.runtime.tasks.mailbox.MailboxProcessor.runMailboxLoop(MailboxProcessor.java:229)"
    },
    {
      "threadName": "Aggregate - Sliding Window -> Update - Lifelong Counter (25/128)#15",
      "stringifiedThreadInfo": "\"Aggregate - Sliding Window -> Update - Lifelong Counter (25/128)#15\" Id=118778 RUNNABLE
        at app//org.apache.flink.contrib.streaming.state.RocksDBCachingPriorityQueueSet.isPrefixWith(RocksDBCachingPriorityQueueSet.java:334)
        at app//org.apache.flink.contrib.streaming.state.RocksDBCachingPriorityQueueSet.peek(RocksDBCachingPriorityQueueSet.java:134)
        at app//org.apache.flink.streaming.api.operators.InternalTimerServiceImpl.onProcessingTime(InternalTimerServiceImpl.java:293)"
    }
  ]
}
```
## How to Read & Analyze

### Identify the Problem Pattern
*   **Application Hang**: Look for **deadlocks** (threads waiting on each other) or all threads waiting on a database/IO resource.
*   **High CPU**: Focus on **RUNNABLE** threads.
*   **Slowness**: Look for **BLOCKED** threads.

## Use cases
- Diagnosing deadlocks
- Identifying blocked threads
- Understanding what code is running at a specific moment
- Troubleshooting hangs or performance issues

## Characteristics
- **Point-in-time snapshot**
- **Relatively lightweight**
- **Shows current execution state only**
- **Doesn't show historical patterns or resource consumption**


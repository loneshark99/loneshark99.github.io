---
layout: post
title: "Performance Analysis in Java Applications:  A Flink Deep Dive"
date: 2025-12-30 14:04:00
categories: [Java, Performance, Flink]
tags:  [profiling, debugging, thread-dump, flame-graph, observability]
---

## Background

Recently, I've been working on a Flink Java application experiencing significant performance issues. This led me down a path of discovering the rich set of diagnostic tools available in the Java ecosystem for identifying and resolving backpressure and performance bottlenecks.

The Flink dashboard provides multiple complementary analysis tools, each offering specific insights into application state and behavior.

---

## Performance Analysis Toolkit

### Available Tools in Flink

1. **Thread Dump** - Point-in-time snapshot of all thread states
2. **CPU Profiling** - Flame graph analysis of CPU consumption
3. **Allocation Profiling** - Flame graph analysis of memory allocation patterns
4. **Metrics & Dashboards** - Prometheus + Grafana for time-series data
5. **Exception History** - Historical view of errors and exceptions
6. **Memory Metrics** - Built-in Flink dashboard metrics
7. **Logs** - TaskExecutor and JobManager logs for detailed analysis

Each tool serves a distinct purpose in the diagnostic workflow.  Let's dive deeper into one of the most fundamental tools:  the thread dump.

---

## Thread Dump Analysis

### What Is a Thread Dump?

A thread dump is a **point-in-time snapshot** of all threads in the JVM. It captures:

- **Thread states** (RUNNABLE, WAITING, BLOCKED, TIMED_WAITING)
- **Stack traces** (what method each thread is currently executing)
- **Lock information** (held locks and waiting threads)
- **Thread metadata** (priority, daemon status, thread ID)

### Characteristics

| Property | Description |
|----------|-------------|
| **Capture Type** | Point-in-time snapshot |
| **Overhead** | Very lightweight |
| **Scope** | Current execution state only |
| **Limitations** | No historical data or resource consumption patterns |

### Common Use Cases

- ✅ Diagnosing deadlocks
- ✅ Identifying blocked threads
- ✅ Understanding current code execution
- ✅ Troubleshooting application hangs
- ✅ Analyzing concurrency bottlenecks

---

## Understanding Thread States

Thread states are critical for interpreting dumps correctly:

### RUNNABLE
**The thread is actively executing or ready to execute.**

- May be using CPU or waiting for OS resources (I/O, network)
- **Performance signal**: Many threads in RUNNABLE with identical stack traces indicate a CPU bottleneck in that code path

### BLOCKED
**The thread is waiting to acquire a monitor lock held by another thread.**

- **Performance signal**: Multiple threads BLOCKED on the same lock indicate a concurrency bottleneck
- Common cause of reduced throughput

### WAITING
**The thread is waiting indefinitely for another thread's action.**

- Triggered by:  `Object.wait()`, `Thread.join()`, `LockSupport.park()`
- **Common pattern**: Idle thread pool workers waiting for tasks

### TIMED_WAITING
**The thread is waiting with a specified timeout.**

- Triggered by: `Thread.sleep()`, `Object.wait(timeout)`, `LockSupport.parkNanos()`
- Similar to WAITING but will automatically resume after timeout

---

## Example Thread Dump (Flink Application)

```json
{
  "threadInfos": [
    {
      "threadName": "main",
      "stringifiedThreadInfo": "\"main\" Id=1 WAITING on java.util.concurrent.CompletableFuture$Signaller@2993e0bc
        at java.base@17.0.12/jdk.internal.misc.Unsafe.park(Native Method)
        - waiting on java.util.concurrent.CompletableFuture$Signaller@2993e0bc
        at java.base@17.0.12/java.util.concurrent.locks.LockSupport. park(Unknown Source)
        ...
        at app//org.apache.flink.runtime.taskexecutor.TaskManagerRunner.main(TaskManagerRunner.java:475)"
    },
    {
      "threadName": "flink-pekko.actor.default-dispatcher-4",
      "stringifiedThreadInfo": "\"flink-pekko.actor. default-dispatcher-4\" Id=35 RUNNABLE
        at java.management@17.0.12/sun.management.ThreadImpl.dumpThreads0(Native Method)
        at java.management@17.0.12/sun.management.ThreadImpl.dumpAllThreads(Unknown Source)
        at app//org.apache. flink.runtime.util.JvmUtils.createThreadDump(JvmUtils.java:50)
        ...
        at java.base@17.0.12/java.util.concurrent. ForkJoinWorkerThread. run(Unknown Source)"
    },
    {
      "threadName": "Source: Kafka Source - CounterUnifiedEventSlimV2 -> Filter (25/128)#15",
      "stringifiedThreadInfo": "\"Source:  Kafka Source - CounterUnifiedEventSlimV2 -> Filter (25/128)#15\" Id=118752 TIMED_WAITING
        at java. base@17.0.12/jdk.internal.misc. Unsafe.park(Native Method)
        at app//org.apache. flink.streaming.runtime.tasks.mailbox.TaskMailboxImpl.take(TaskMailboxImpl. java:149)
        at app//org.apache.flink. streaming.runtime.tasks.mailbox.MailboxProcessor.runMailboxLoop(MailboxProcessor. java:229)"
    },
    {
      "threadName":  "Aggregate - Sliding Window -> Update - Lifelong Counter (25/128)#15",
      "stringifiedThreadInfo": "\"Aggregate - Sliding Window -> Update - Lifelong Counter (25/128)#15\" Id=118778 RUNNABLE
        at app//org.apache.flink.contrib.streaming.state.RocksDBCachingPriorityQueueSet.isPrefixWith(RocksDBCachingPriorityQueueSet.java:334)
        at app//org.apache.flink.contrib.streaming.state. RocksDBCachingPriorityQueueSet.peek(RocksDBCachingPriorityQueueSet. java:134)
        at app//org.apache.flink. streaming.api.operators.InternalTimerServiceImpl.onProcessingTime(InternalTimerServiceImpl.java:293)"
    }
  ]
}
```

---

## How to Read & Analyze Thread Dumps

### Step 1: Identify the Problem Pattern

| Symptom | What to Look For |
|---------|------------------|
| **Application Hang** | Deadlocks (circular lock dependencies) or all threads waiting on external resources (DB, I/O) |
| **High CPU Usage** | Many threads in RUNNABLE state with similar stack traces |
| **Slow Performance** | Many threads in BLOCKED state waiting on the same lock |
| **Backpressure** | Task threads in WAITING/TIMED_WAITING while data accumulates |

### Step 2: Analyze Thread States Distribution

```bash
# Quick command to count thread states (if you have text dump)
grep "java.lang.Thread. State" threaddump.txt | sort | uniq -c
```

- **Healthy application**:  Balanced distribution with most threads WAITING (idle pool workers)
- **CPU-bound problem**: High percentage of RUNNABLE threads
- **Lock contention**: High percentage of BLOCKED threads

### Step 3: Look for Repetitive Stack Traces

Multiple threads with identical stack traces indicate: 
- **CPU hotspot**: If threads are RUNNABLE
- **Lock bottleneck**: If threads are BLOCKED
- **Design issue**:  Possible need for async processing or better concurrency design

### Step 4: Check for Deadlocks

Modern JVM thread dumps automatically detect and report deadlocks:

```
Found one Java-level deadlock:
=============================
"Thread-1": 
  waiting to lock monitor 0x00007f8a1c004e00 (object 0x00000000d5f78a20, a java.lang.Object),
  which is held by "Thread-2"
"Thread-2":
  waiting to lock monitor 0x00007f8a1c007360 (object 0x00000000d5f78a30, a java.lang.Object),
  which is held by "Thread-1"
```

---

## Flink-Specific Insights

### Kafka Source Thread (TIMED_WAITING)
```
"Source: Kafka Source - CounterUnifiedEventSlimV2 -> Filter (25/128)#15"
TIMED_WAITING at TaskMailboxImpl.take()
```
**Interpretation**: Source thread is idle, waiting for messages from Kafka.  This is normal behavior when there's no data to process.

### Aggregate Thread (RUNNABLE)
```
"Aggregate - Sliding Window -> Update - Lifelong Counter (25/128)#15"
RUNNABLE at RocksDBCachingPriorityQueueSet.isPrefixWith()
```
**Interpretation**: Thread is actively processing window aggregations using RocksDB state backend.  If many threads show this, it could indicate RocksDB performance issues.

---

## Best Practices

### 1. Take Multiple Dumps
A single thread dump is a snapshot.  Take 3-5 dumps with 5-10 second intervals to see patterns.

### 2. Correlate with Metrics
Combine thread dumps with:
- CPU usage metrics
- GC logs
- Application-specific metrics (throughput, latency)

### 3. Use Proper Tools
- **jstack**: Command-line tool for thread dumps
- **VisualVM**: GUI with thread dump analysis
- **FastThread**: Online thread dump analyzer ([fastthread.io](https://fastthread.io))
- **Flink Dashboard**: Built-in thread dump feature

### 4. Document Baseline Behavior
Take thread dumps during normal operation to understand healthy patterns.

---

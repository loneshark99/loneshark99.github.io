---
layout: post
title: "Java Garbage Collection"
date: 2025-12-29 13:38:00
categories: Java
---

Garbage collection and the wait times are the some of the most important metrics you need to track to see if you application is running with good performance.  

For tuning the application you need to watch of the 

GC Frequency/ GC Cycles
GC Time / Time spent in running GC. When Full GC runs, it stops your application and thus adds latency.

**Try to use primitives when possible.  If need null or List<>  then consider using Wrappers.**
**Wrapper Classes (Integer,Long,Double) are immutable, so it needs boxing to create a new instance. Watch out for these.**

Below is a java program that demostrates this.

```java
package com.example.learning;

import java.util.concurrent.TimeUnit;
import java.lang.management.*;

public class GCPressures {
    public static void main(String[] args) throws InterruptedException {
        long iteration = 50_000_000L;
        System.out.println("-- Starting benchmark");

        System.gc();
        TimeUnit.MILLISECONDS.sleep(200);


        // Primitives no boxing
        long startGCCount = getGCCount();
        long startGCTime = getGCTime();
        long startPrimitive = System.nanoTime();

        long sumPrimitive = testPrimitive(iteration);

        long endPrimitive = System.nanoTime();
        long endGCTime = getGCTime();
        long endGCCount = getGCCount();


        printMetrics("1. Primitive (long)",
                sumPrimitive,
                endPrimitive - startPrimitive,
                endGCCount - startGCCount,
                endGCTime - startGCTime);

        System.gc();
        TimeUnit.MILLISECONDS.sleep(200);

        startGCCount = getGCCount();
        startGCTime = getGCTime();
        startPrimitive = System.nanoTime();

        sumPrimitive = testWrapper(iteration);

        endPrimitive = System.nanoTime();
        endGCTime = getGCTime();
        endGCCount = getGCCount();

        printMetrics("1. Wrapper (Long)",
                sumPrimitive,
                endPrimitive - startPrimitive,
                endGCCount - startGCCount,
                endGCTime - startGCTime);

    }

    private static void printMetrics(String label, long result, long durationNs, long gcCount, long gcTimeMs) {
        double durationMs = durationNs / 1_000_000.0;
        System.out.println(label + " Result: " + result);
        System.out.printf("   Execution Time: %.2f ms\n", durationMs);
        System.out.println("   GC Count:       " + gcCount);
        System.out.println("   GC Time:        " + gcTimeMs + " ms");
        System.out.println("------------------------------------------------");
    }

    private static long testPrimitive(long iteration) {
        long sum = 0;
        for (int i = 0; i < iteration; i++) {
            sum += i;
        }

        return sum;
    }

    private static Long testWrapper(long iteration) {
        Long sum = 0L;
        for (Integer i = 0; i < iteration; i++) {
            sum += i;
        }

        return sum;
    }

    private static long getGCTime() {
        long total = 0;
            for (GarbageCollectorMXBean gc : ManagementFactory.getGarbageCollectorMXBeans())
        {
            long count = gc.getCollectionTime();
            if (count > 0)
            {
                total += count;
            }
        }

        return  total;
    }

    private static long getGCCount() {
       long total = 0;
       for (GarbageCollectorMXBean gc : ManagementFactory.getGarbageCollectorMXBeans())
       {
           long count = gc.getCollectionCount();
           if (count > 0) {
               total += count;
           }
       }
       return total;
    }
}

```


The method `GarbageCollectorMXBean.getCollectionCount()` returns the **total number of times** the garbage collector has run since the Java application started.

Here is the specific breakdown of the metric:

### 1. What it returns

* **Data Type:** `long`
* **Value:** An incrementing counter (0, 1, 2, 3...).
* **Special Case:** It returns `-1` if the count is undefined for that specific collector.

### 2. What is it the "count" of?

It counts **completed GC cycles**.

Every time the JVM runs out of space in a specific part of memory (the Heap), it triggers a "collection cycle" to find unused objects and delete them. This method counts those distinct events.

In the context of the **Boxing/Unboxing** code we wrote, this count distinguishes between two types of events (as modern Java uses multiple collectors):

#### A. Minor GCs (The "Young" Generation)

* **Trigger:** The "Eden" space is full.
* **Cause in your code:** The loop created millions of temporary `Long` objects.
* **The Count:** This number increases rapidly (e.g., from 0 to 50). This tells you your application is "churning" memory—creating and destroying objects very fast.

#### B. Major GCs (The "Old" Generation)

* **Trigger:** The main memory area is full.
* **Cause:** Long-lived objects that survive many Minor GCs.
* **The Count:** This usually stays low (0 or 1) in your benchmark because the `Long` objects are deleted almost immediately and don't get moved to the Old Generation.

### Summary: Count vs. Time

* **`getCollectionCount()` (Frequency):** Answers "How often is the application stopping to clean memory?" (High count = High memory pressure).
* **`getCollectionTime()` (Duration):** Answers "How much total time did we lose to these stops?" (High time = Application lag).

---


The method `GarbageCollectorMXBean.getCollectionTime()` returns the **approximate accumulated collection elapsed time in milliseconds**.

Here is the breakdown of what that means practically:

### 1. The Unit

It returns a `long` value in **milliseconds** (ms).

* If the result is `500`, that means the Garbage Collector has spent a total of 0.5 seconds running since the JVM started.
* If the time is undefined for the collector, it returns `-1`.

### 2. It is Cumulative

This is not the time of the *last* specific GC event. It is a running total (an odometer) of all time spent performing garbage collection since the application launched.

In your benchmark code, when we did:

```java
endGcTime - startGcTime

```

We were calculating the "delta" to find out exactly how many milliseconds were "lost" to the Garbage Collector specifically during the execution of that loop.

### 3. What counts as "Time"?

This depends slightly on the Garbage Collector algorithm (G1, Parallel, ZGC, etc.), but generally:

* **For "Stop-the-World" events:** It measures the actual wall-clock time your application was paused/frozen while the GC did its work.
* **For Concurrent events:** It measures the time the GC threads were active.

### Why does this metric matter?

This is arguably more important than the *count*.

* **Scenario A:** GC runs 100 times, but total time is 50ms.
* *Verdict:* **Healthy.** The GC is running frequently but very quickly (likely cleaning up short-lived objects in the "Young" generation). This has minimal impact on user experience.


* **Scenario B:** GC runs 5 times, but total time is 2000ms.
* *Verdict:* **Critical.** The GC ran fewer times, but it took 2 full seconds. If this is a web server or a game, the user just experienced a 2-second freeze.



### Summary

* **`getCollectionCount()`** tells you **how busy** the memory manager is (frequency).
* **`getCollectionTime()`** tells you **how expensive** that management is (latency/cost).

In the context of the **Boxing** benchmark, a high `getCollectionTime()` proves that using wrapper objects doesn't just waste memory—it actively steals CPU cycles away from your actual calculation logic.

Would you like to see how to distinguish between **Young Gen** time (cheap) and **Old Gen** time (expensive) in the code?
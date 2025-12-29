---
layout: post
title: "Non blocking ThreadExecutor Shutdown  Api and Blocking awaitTermination Api."
date: 2025-12-29 15:17:00
categories: Java
---

I was curious about the Executor.Shutdown method. I thought that this was a blocking method but its not. Its basically flips the switch of the executor to Shutting down.
Pretty interesting and someone can easily make a mistake. Below it shows how to shutdown a Executor Service correctly. **Follow this!**


The short answer is **No, it is NOT a blocking method.**

In Java's `ExecutorService`, calling `.shutdown()` returns **immediately**. It does not wait for the active tasks to complete.

Here is the detailed breakdown of what happens when you call it versus how you actually make it wait.

### 1. What `shutdown()` actually does (The Signal)

When you call `executor.shutdown()`:

1. **State Change:** It flips the internal switch of the executor from "Running" to "Shutting Down".
2. **Gate Closing:** It stops accepting **new** tasks. If you try to submit a new task after this, it throws a `RejectedExecutionException`.
3. **Continuation:** It allows the threads to keep running until they finish all the tasks that were already in the queue.
4. **Return:** It returns control to your main thread immediately. Your code moves to the next line while the background threads are still working.

### 2. How to make it block? (`awaitTermination`)

If you want your main thread to pause and wait for the threads to finish (which is usually what people want when they ask this question), you must use a specific second method: **`.awaitTermination()`**.

### The Correct "Graceful Shutdown" Pattern

You almost never use `shutdown()` alone. The standard industry pattern (often called the "two-phase shutdown") looks like this:

```java
ExecutorService executor = Executors.newFixedThreadPool(5);

// 1. Signal the shutdown (NON-BLOCKING)
executor.shutdown(); 

try {
    // 2. Block and wait for existing tasks to finish (BLOCKING)
    // We give it a timeout (e.g., 60 seconds).
    if (!executor.awaitTermination(60, TimeUnit.SECONDS)) {
        
        // 3. If time runs out and tasks are still running, force them to stop
        executor.shutdownNow(); 
        
        // 4. Wait a bit more for the forced stop to take effect
        if (!executor.awaitTermination(60, TimeUnit.SECONDS)) {
            System.err.println("Pool did not terminate");
        }
    }
} catch (InterruptedException ie) {
    // (Re-)Cancel if current thread also interrupted
    executor.shutdownNow();
    Thread.currentThread().interrupt();
}

```

### Comparison of Methods

| Method | Blocking? | Description |
| --- | --- | --- |
| **`shutdown()`** | **NO** | "Polite" signal. "Please finish what you have, but don't take new work." Returns immediately. |
| **`shutdownNow()`** | **NO** | "Forceful" signal. "Drop everything." Attempts to stop running tasks (via Thread interruption) and returns a list of waiting tasks that were never started. |
| **`awaitTermination(time)`** | **YES** | The actual blockade. "I will stand here and wait until you are done OR until the timeout expires." |

### Common Mistake

A common bug is writing code like this:

```java
executor.shutdown();
System.out.println("All Done!"); // WRONG!

```

The "All Done!" message will print **before** the tasks are actually finished, because `shutdown()` didn't wait. The application might even exit while tasks are halfway done (if they are Daemon threads) or hang forever (if they are Non-Daemon threads).
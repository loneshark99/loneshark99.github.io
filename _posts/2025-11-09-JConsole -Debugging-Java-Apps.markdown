---
layout: post
title: "JConsole and process deep dive"
date: 2025-11-09 10:55:00
categories: Java, JConsole
---
[ActiveThreads]: https://github.com/loneshark99/loneshark99.github.io/blob/master/images/Active%20Threads.png?raw=true
[JVMMemory]: https://github.com/loneshark99/loneshark99.github.io/blob/master/images/JVM%20Memory.png?raw=true
[LoadedClassCount]: https://github.com/loneshark99/loneshark99.github.io/blob/master/images/Loaded%20and%20Unloaded%20class%20count.png?raw=true
[JPS]: https://github.com/loneshark99/loneshark99.github.io/blob/master/images/JPS.png?raw=true



I was recently trying to understand deeper into the java process and how memory model used by a java process. I found the following analysis with jconsole very helpful. jconsole is a debugging tool provided in the java jdk and provided a visual way to understand you application at a deeper level. With jconsole you can 

- view the threads used in the application and there corresponding stack trace.
- view the Heap Memory and Non heap memory and see the different GC generation and how the memory is getting used. Is there any sign of memory leaks and get deeper in memory allocation etc. This is also very useful in diagnosing the OOM issues. It provides a real time insight into a running application.
- View the classes loaded and unloaded.
- CPU monitoring.

**JPS Command**

jps (Java process status) is another command which you can use the find the JVM's running on a machine and what are the different arguments that were passed into it, full jar path.

jps -l - Shows full package name or full JAR path
jps -m - Shows arguments passed to the main() method
jps -v - Shows JVM arguments (like -Xmx, -Xlog:gc*, etc.)

![JPS][JPS]

Below sample code

- Simulates active threads which you can see in the jconsole.
![Active threads][ActiveThreads]

- Simulates active heap memory increases and we can see the garbage collection stats
![JVM Memory][JVMMemory]

-- Simulates the active loaded class count and unloaded class count.
![Loaded class count][LoadedClassCount]


It is very important to understand this to solve lot of issues in Java applications.

```java
public static void main(String[] args) {

        System.out.println("JConsole Demo started. PID: " + ProcessHandle.current().pid());
        System.out.println("Connect Jconsole now. App will run for 5 mins");

        // Task 1 (Simulates the active threads and it can viewed in JConsole Threads tab)
        ExecutorService executor = Executors.newFixedThreadPool(5);
        for (int i = 0; i < 5; i++) {
            final int taskId = i;
            executor.submit(() -> {
               while(running) {
               try {
                   Thread.sleep(2000);
                   System.out.println("Worker-" + taskId + " active");
               }
               catch (InterruptedException e) {
                   Thread.currentThread().interrupt();
               }
               }
            });
        }

        // Task 2 (Simulates memory consumption and it can be viewed in JConsole Memory tab)
        ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);
        scheduler.scheduleAtFixedRate(() -> {
           memoryConsumers.add(new byte[10 * 1024 * 1024]); //Allocate 10MB
              System.out.println("Allocated 10MB, total allocations: " + memoryConsumers.size() * 10 + "MB");
        }, 5,5, TimeUnit.SECONDS);

        // Task 3 (Simulates class loading activity and it can be viewed in JConsole Classes tab)
        scheduler.scheduleAtFixedRate(() -> {
            try {
                Class.forName("java.util.HashMap$Node");
                System.out.println("Class Loaded");
            }
            catch (ClassNotFoundException e) {
                e.printStackTrace();
            }
        }, 0, 3, TimeUnit.SECONDS);

        //Task 4 (Simulates CPU activity and it can be viewed in JConsole CPU tab)
        executor.submit(() -> {
            while (running) {
                double result = 0;
                for (int i = 0; i < 1_000_000; i++) {
                    result += Math.sqrt(i);
                }
                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }
            }
        });

        try {
            Thread.sleep(300_000); // Run for 5 minutes
        }
        catch (Exception e){
            e.printStackTrace();
        }


        running = false;
        executor.shutdown();
        scheduler.shutdownNow();

        System.out.println("JConsole Demo finished.");
    }

```
---
layout: post
title: "Java Garbage Collection"
date: 2025-12-29 14:03:00
categories: Java
---


Flink provides a dedicated set of data types located primarily in the `org.apache.flink.types` package. These are designed to overcome the performance limitations of standard Java types (immutability and heavy serialization).

Here are the main categories of types Flink provides for performance:

### 1. The `Value` Types (Mutable Primitives)

These are the direct performance replacements for Java's wrapper classes (`Integer`, `String`, etc.). As discussed, their superpower is **mutability** (Object Reuse) and **lightweight serialization**.

| Flink Type | Java Equivalent | Key Performance Feature |
| --- | --- | --- |
| `IntValue` | `Integer` | Mutable; updates in place without GC. |
| `LongValue` | `Long` | Mutable; 8 bytes on wire (no overhead). |
| `StringValue` | `String` | Mutable! Can change text buffer without creating new String objects. |
| `BooleanValue` | `Boolean` | Single byte serialization. |
| `NullValue` | `null` | Efficient way to represent "nothing" in streams without null pointer risks. |
| `ListValue` / `MapValue` | `List` / `Map` | Mutable collections that implement efficient serialization. |

**When to use:** Use these when writing custom `ProcessFunction` or `AggregateFunction` logic where you need to update a state variable millions of times per second.

### 2. The `Tuple` Types

Java does not have built-in tuples, so Flink provides `Tuple0` through `Tuple25`. These are generic classes that can hold a fixed number of fields of various types.

* **Structure:** `Tuple2<String, Integer>`, `Tuple3<Long, Double, String>`, etc.
* **Fields:** Accessed publicly via `.f0`, `.f1`, `.f2`, etc.

**Why they perform well:**

1. **Optimized Serialization:** Flink's TypeSystem detects Tuples automatically and generates efficient serializers for them. It knows exactly how many fields there are and their types, avoiding reflection.
2. **Key Selection:** You can use field positions (e.g., `.keyBy(0)`) which is very fast for Flink to process compared to analyzing a complex object.

### 3. The `Row` Type

The `Row` type is effectively a dynamic Tuple. It is used primarily in the **Table API** and **Flink SQL**.

* **Structure:** It works like a database row; it can hold an arbitrary number of fields, but it is not strongly typed like a Tuple (it stores `Object`).
* **Performance:** While slightly slower than a POJO or Tuple due to casting overhead, Flink optimizes `Row` heavily for SQL operations. In modern Flink versions, the internal binary format for Rows allows operations on data *without* fully deserializing it.

### 4. POJOs (Plain Old Java Objects)

While this is a standard Java concept, Flink treats POJOs as a **special performance citizen**. If you define a class that follows specific rules, Flink treats it as a "POJO Type" rather than a "Generic Type."

**The Rules for High Performance:**

1. The class must be `public`.
2. It must have a public **no-argument constructor** (default constructor).
3. All fields are either `public` or have standard getters/setters.

**Performance Impact:**

* **If it IS a POJO:** Flink analyzes the fields once and creates a super-fast serializer (POJO Serializer).
* **If it is NOT a POJO:** Flink falls back to **Kryo** serialization. Kryo is a generic serialization framework; it is powerful but significantly slower and produces larger binary data than Flink's native serializers.

### Summary: Hierarchy of Performance

If you want the absolute fastest application, here is the hierarchy of types you should choose:

1. **Best:** `Value` Types (e.g., `IntValue`) + Object Reuse enabled. (Zero GC, smallest size).
2. **Great:** POJOs (must follow rules). (Very fast serialization, readable code).
3. **Good:** Tuples (`Tuple2`, `Tuple3`). (Fast, but less readable code `f0`, `f1`).
4. **Avoid:** Generic classes that don't follow POJO rules (Falls back to Kryo/Slow).

### Code Example: Tuple vs POJO

Even though Tuples are "Flink Native," most developers prefer POJOs for readability because the performance difference is negligible.

**Tuple (Hard to read, fast):**

```java
DataStream<Tuple2<String, Integer>> stream = ...
stream.map(t -> t.f1 + 1); // What is f1? You have to remember it's the count.

```

**POJO (Easy to read, fast):**

```java
public class UserCount {
    public String name;
    public int count; // Primitive!
    public UserCount() {} // Necessary for Flink POJO performance
}

DataStream<UserCount> stream = ...
stream.map(u -> u.count + 1); // Clear and optimized.

```
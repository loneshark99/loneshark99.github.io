---
layout: post
title: "Jar files in Java"
date: 2025-10-18 14:06:00
categories: Java
---
[JavaDecompiler]: https://github.com/loneshark99/loneshark99.github.io/blob/master/images/Java-Decompiler.png?raw=true

# Jar files in Java

Lot of times when you are working on Java projects, you want to examine the jar file and see what files it contains and if it has the necessary files or not.

The best way to do that is use jd-gui which is a java decompiler. This app decompiles the .class files and shows the code, it also shows the Manifest information which is very useful to see the Main Class and other important information about the app.

Github url : https://github.com/java-decompiler/jd-gui


Execute  a Jar file.
```bash
  java -jar target/flink-app-0.1.jar
```


Java decompiler is a very useful tool. It will show the classes by packages.


![alt text][JavaDecompiler]


Jar files are essentially a zip file, you can use the jar command to extract it to a different folder.
```bash
  jar xf target/flink-app-0.1.jar -C /path/to/destination/folder
```

Fat jar is a jar files which includes all the required dependencies. It is generally created with the Maven Shade Plugin. In the POM file include the following settings.

```xml
<!-- We use the maven-shade plugin to create a fat jar that contains all necessary dependencies. -->
<plugin>
	<groupId>org.apache.maven.plugins</groupId>
	<artifactId>maven-shade-plugin</artifactId>
	<version>3.1.1</version>
	<executions>
		<!-- Run shade goal on package phase -->
		<execution>
			<phase>package</phase>
			<goals>
				<goal>shade</goal>
			</goals>
			<configuration>
				<createDependencyReducedPom>false</createDependencyReducedPom>
				<artifactSet>
					<excludes>
						<exclude>com.google.code.findbugs:jsr305</exclude>
					</excludes>
				</artifactSet>
				<filters>
					<filter>
						<!-- Do not copy the signatures in the META-INF folder.
						Otherwise, this might cause SecurityExceptions when using the JAR. -->
						<artifact>*:*</artifact>
						<excludes>
							<exclude>META-INF/*.SF</exclude>
							<exclude>META-INF/*.DSA</exclude>
							<exclude>META-INF/*.RSA</exclude>
						</excludes>
					</filter>
				</filters>
				<transformers>
					<transformer implementation="org.apache.maven.plugins.shade.resource.ServicesResourceTransformer"/>
					<transformer implementation="org.apache.maven.plugins.shade.resource.ManifestResourceTransformer">
						<mainClass>org.example.DataGenSourceConnector</mainClass>
					</transformer>
				</transformers>
			</configuration>
		</execution>
	</executions>
</plugin>
```


If you want to see all the methods that are defined in .class file you can use the javap command line cool. This is a really cool tool to see if the methods are available or not.

-cp will copy the jar file into the classpath.

javap  -p -cp target/flink-1.19-app-1.0-SNAPSHOT.jar com.example.TestKeyedProcessFunction


```bash
yash@YashDevBox ~/f/P/flink-1.19-app> javap  -p -cp target/flink-1.19-app-1.0-SNAPSHOT.jar com.example.TestKeyedProcessFunction
Compiled from "TestKeyedProcessFunction.java"
public class com.example.TestKeyedProcessFunction extends org.apache.flink.streaming.api.functions.KeyedProcessFunction<java.lang.String, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>> {
  private org.slf4j.Logger LOG;
  private org.apache.flink.api.common.state.ValueState<java.lang.Integer> valueState;
  private org.apache.flink.api.common.state.ValueState<java.lang.Long> timestampState;
  public com.example.TestKeyedProcessFunction();
  public void open(org.apache.flink.configuration.Configuration) throws java.lang.Exception;
  public void processElement(org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>, org.apache.flink.streaming.api.functions.KeyedProcessFunction<java.lang.String, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>>.Context, org.apache.flink.util.Collector<org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>>) throws java.lang.Exception;
  public void onTimer(long, org.apache.flink.streaming.api.functions.KeyedProcessFunction<java.lang.String, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>, org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>>.OnTimerContext, org.apache.flink.util.Collector<org.apache.flink.api.java.tuple.Tuple2<java.lang.String, java.lang.Integer>>) throws java.lang.Exception;
  public void processElement(java.lang.Object, org.apache.flink.streaming.api.functions.KeyedProcessFunction$Context, org.apache.flink.util.Collector) throws java.lang.Exception;
}
```
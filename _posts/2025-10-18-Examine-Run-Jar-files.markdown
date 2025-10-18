---
layout: post
title: "Jar files in Java"
date: 2025-10-18 14:06:00
categories: Java
---

# Jar files in Java

Lot of times when you are working on Java projects, you want to examine the jar file and see what files it contains and if it has the necessary files or not.

The best way to do that is use jd-gui which is a java decompiler. This app decompiles the .class files and shows the code, it also shows the Manifest information which is very useful to see the Main Class and other important information about the app.

Github url : https://github.com/java-decompiler/jd-gui


Execute  a Jar file.
```bash
  java -jar target/flink-app-0.1.jar
```


Java decompiler is a very useful tool. It will show the classes by packages.


![Java Decompiler View](https://loneshark99.github.io/images/KJava-Decompiler.png "Java Decompiler View")


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

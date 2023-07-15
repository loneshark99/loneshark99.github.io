---
layout: post
title:  "MetaProgramming (aka) CodeGeneration"
date:   2019-03-02 12:03:00 PM
categories: MetaProgramming, CodeGeneration
---

Overtime I have come to realize that creating framework is where the real fun in programming is for me. It gives me immense satisfaction and sense of focus that is very important for me as a developer.

When creating a framework, once of the most important thing to keep in mind is Code Generation. Meta programming is an important skill to learn as a developer.

C# and DotNet provides many ways to do code generation. I am listing some but there are many more.

- **MS Build Tasks which generates code.**

  Config validation, environment config creation etc are common tasks that fall in this category.

  Here is a sample example of code generation through MSBuild Task.

  https://learn.microsoft.com/en-us/visualstudio/msbuild/tutorial-custom-task-code-generation?view=vs-2022

- **T4 Templates**

   T4 templates are templates file which contain some code which generated .cs file and which are added and compiled as a part of your source code.

   One of the common use case if creating the DataModel classes for the database tables. You write the template .tt code which will go through the metadata and then generate correspoding classes for you. It is pretty cool technique.

- **Expression Tree**

   Expression Trees are the not executable code but are a representation of your code. You can analyze the intent of the programmer/code by looking at the expression tree. For example. what functions the programmer wants to call or What properties he wants to use.

   With System.Linq.ExpressionTree work generally falls into 2 categories.

    a) Analysis of the ExpressionTree, i.e figuring out the intent and then taking some actions on it. Moq framework is a very good example of this.
    b) Creating Expression tree or do code generation based on some dynamic parameters. This is a common use case when you want to do filtering but you dont know the parameters beforehand. i.e use selects the filtering columns etc.

       PredicateBuild by Linqpad Author is a very good example of this. 

       https://www.albahari.com/nutshell/predicatebuilder.aspx

- **CodeDom**
  
   Write cs file using Codedom is a very common technique for code generation. CodeDOM stands for Code Document Object Model. You can write code which generates code and then compile it into an assembly and run it in your project.


I will be writing about each of these techniques in detail and will also be recording some videos. 

**Happy Learning and improving one day at a time**

**Happy Learning and improving one day at a time**




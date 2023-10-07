---
layout: post
title:  "Environment Variables"
date:   2023-10-07 11:14:00 AM
categories: Productivity, Environment Variables
---

Today going to list some simple commands which should be in every developers toolkit. These are things that you will need to do quite often in your daily job and its best to incorporate these in your muscle memory.


To the environment variable values using **echo** command.

```console
C:\>echo %PATH%

C:\>echo %COMPUTERNAME%

C:\>echo C:\\%filename%
```

%abc%  The percent sign in windows is used as a variable name. When used with the percent sign, the variable will be substituted by its value.



To view environment variables using the **set** command
```console
C:\>set

C:\>set PATH

C:\>set COMPUTERNAME
```

To update an environment variable like path you can do it with the **setx** command.
```console
C:\>setx newvar "%PATH%;C:\FolderName;"

C:\>setx /m newvar "%PATH%;C:\FolderName;"
```
Replace newvar with PATH.

/M is used to set the environment at the System/Machine level.


**Happy Learning and improving one day at a time**

---
layout: post
title: "tr and tar Linux Command"
date: 2023-10-14 5:26:00 PM
categories: Development
---

**tr command**

tr command is a very useful command in linux and helps a lot with day to day activity. tr is a short form of translate.

Below is a common usage for me to view the Path environment variable with each folder in a new line.

```bash
yash@yash-ThinkPad-T430:~$ printenv PATH
/home/yash/.nvm/versions/node/v14.15.1/bin:/home/yash/gems/bin:/home/yash/gems/bin:/home/yash/gems/bin:/home/yash/.local/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/games:/usr/local/games:/snap/bin:/home/yash/.dotnet/tools

yash@yash-ThinkPad-T430:~$ printenv PATH | tr : \n
/home/yash/.nvm/versions/node/v14.15.1/bin
/home/yash/gems/bin
/home/yash/gems/bin
/home/yash/gems/bin
/home/yash/.local/bin
/usr/local/sbin
/usr/local/bin
/usr/sbin
/usr/bin
/sbin
/bin
/usr/games
/usr/local/games
/snap/bin
/home/yash/.dotnet/tools
```

It can also be used to remove certain characters like below with the -d option.

```bash
yash@yash-ThinkPad-T430:~$ printenv PATH | tr : \n | tr -d yash
/ome//.nvm/verion/node/v14.15.1/bin
/ome//gem/bin
/ome//gem/bin
/ome//gem/bin
/ome//.locl/bin
/ur/locl/bin
/ur/locl/bin
/ur/bin
/ur/bin
/bin
/bin
/ur/gme
/ur/locl/gme
/np/bin
/ome//.dotnet/tool
```

**tar -- TAPE ARCHIVE**

Create a Tar ball from a list of files

Tar has some options which are commonly used.

- c -- Create File
- x -- Extract File
- f -- File Name
- v -- Verbose
- r -- Append
- t -- List Contents
- z -- Zip Content


To Create a Tar Ball 
    ```bash
    tar cvf AllHtmls.tar *.html
    ```

To Extract a Tar Balll 
    ```bash
    tar xvf AllHtmls.tar OR tar -C ~/test -xvf AllHtmls.tar
    ```

**Happy learning and improving one day at a time**
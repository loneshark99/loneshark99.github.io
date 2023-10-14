---
layout: post
title: "Find -- Linux Command"
date: 2023-10-14 5:29:00 PM
categories: Development
---

**find Command**
Some of the common patterns with the find command

- iname for case insensitive search and -mtime/-atime for last modified time or when any file attributes were updated.
    ```bash
    find . -iname *.txt -mtime +5
    find . -iname *.txt -atime -5
    ```

- type is for type of the resource like f for file, d for directory, l for symbolic link
    ```bash
    find . -type f -iname *.txt
    ```


- print adds a trailing new line to each results and print0 adds a trailing null at the end of the each results. It is useful when a file has spaces.
    ```bash
    find . -iname *.txt print or find -iname *.txt print0
    ```

- This command can be used to find all the file greater than 1GB file size.
    ```bash
    find . -iname *.txt -size +1G
    ```

- This command can be used to find all the files that a user can access.
    ```bash
    find . -type f -user yash -print
    ```

- This command find any file which is great than 1GB in size and it will redirect the “permission denied” errors to the /dev/null so that your output will be clean.
    ```bash
    sudo find . -iname '*.*' -size +1G 2>/dev/null
    ```

**Happy learning and improving one day at a time**
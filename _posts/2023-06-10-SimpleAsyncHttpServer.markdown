---
layout: post
title:  "Http Async Server"
date:   2023-02-26 15:29:00 PM
categories: CSharp
---

HttpListener provides the functionality to host bare bore Http Service in apps for interprocess communication. In Microservice architecture it is sometimes required that you have to host some service in your application, for this scenario it is critical to create lightweight Http Service.

**HttpListener** from System.Net namespace provides this capability. Important API in this class is **GetContext**. This is a blocking call and waits for the incoming requests. Once a req is received, we can get the context. Request hangs of the Context and we can access all the needed info about the Request, like Url,Headers,Parameters etc.

```csharp
       var httpContext = await listener.GetContextAsync();
```

One of the important thing is we need to keep the actual processing in another Task. This is needed because we dont want to block the server while we are processing other request. So we can just wrap the processing in a Task and let it complete at which point, we need to flush and close the Response.

SourceCode :

```csharp
 namespace SimpleHttpService
 {
    using System.Net;
    using Newtonsoft.Json;
    public class Program
    {
        static HttpListener listener = new HttpListener();
        static int count  = 0;
        public static async Task Main()
        {
            listener.Prefixes.Add("http://*:" + 8080 + "/");
            listener.Start();
            Console.WriteLine("Service Started...");
            try
            {
                while(true)
                {
                    var httpContext = await listener.GetContextAsync();
                    Console.WriteLine("Request Url :: " + httpContext.Request.Url);
                    Interlocked.Increment(ref count);
                    var _ = Task.Run(async () => {
                        try
                        {
                            using (var reader  = new StreamReader(httpContext.Request.InputStream))
                            {
                                var input = await reader.ReadToEndAsync();
                                if (httpContext?.Request?.Url?.PathAndQuery == "/")
                                {
                                    switch (httpContext?.Request?.HttpMethod)
                                    {
                                        case "GET":
                                            httpContext.Response.ContentType = "application/json";
                                            var responseObj = new Output() { RequestCount = count };
                                            var response = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObj));
                                            await httpContext.Response.OutputStream.WriteAsync(response, 0, response.Length).ConfigureAwait(false);
                                            await httpContext.Response.OutputStream.FlushAsync().ConfigureAwait(false);
                                            Console.WriteLine("Request Completed");
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Request Exception : {ex}");
                        }
                        finally
                        {
                            httpContext.Response.OutputStream.Close();
                            httpContext.Response.Close();
                        }
                    });
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"FATAL :: {ex}");
            }
            finally
            {
                listener?.Stop();
                listener?.Close();
            }
        }
    }

    class Output
    {
        public int RequestCount { get; set; }
    }
 }
```
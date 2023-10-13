---
layout: post
title:  "Background Thread Upgrade"
date:   2023-10-13 2:41:00 PM
categories: Development
---

Lot of times in day to day development we need to do some background processing task, for example reading some file and updating the list. For this it makes sense to use a dedicated thread to do the processing.  We could have also used a Timer for such processing.

Here is a simple implementation of that. We spawn a new thread, lock before doing any updates. Once we are ready to stop, we wait for the thread to join. Simple enough.

```csharp
    public static void Main()
    {
        MachineListManager.GetInstance().Initialize();
        int counter = 0;
        while (counter < 30)
        {
            var machines = MachineListManager.GetInstance().GetMachines();
            if (machines == null || machines.Count < 1)
            {
                Console.WriteLine("No Machines");
            }

            foreach (var machine in machines)
            {
                Console.WriteLine(machine);
            }

            Console.WriteLine("*******************************");
            counter++;
            Thread.Sleep(3000);
        }

        MachineListManager.GetInstance().Stop();
    }
}

public class MachineListManager
{
    private static MachineListManager manager = new MachineListManager();
    private DateTime _lastWriteTimeUtc = DateTime.MinValue;
    private readonly object lockObj = new object();
    private Dictionary<string, MachineInfo> _machines = new Dictionary<string, MachineInfo>(StringComparer.OrdinalIgnoreCase);
    private Thread _monitorThread;
    private readonly CancellationTokenSource _stopToken = new CancellationTokenSource();

    public static MachineListManager GetInstance()
    {
        return manager;
    }

    public void Initialize()
    {
        Start();
    }

    private void Start()
    {
        _monitorThread = new Thread(Monitor);
        _monitorThread.Start();
    }

    public void Stop()
    {
        _stopToken.Cancel();
        _monitorThread.Join();
    }

    public Dictionary<string, MachineInfo> GetMachines()
    {
        lock (lockObj)
        {
            return _machines;
        }
    }

    public void SetMachines(Dictionary<string, MachineInfo> machines)
    {
        lock (lockObj)
        {
            if (machines == null || machines.Count < 1)
            {
                Console.WriteLine("Machine list is empty");
            }

            _machines = machines;
        }
    }

    private void Monitor()
    {
        while (!_stopToken.IsCancellationRequested)
        {
            try
            {
                var machineListFilePath = "Machines.txt";
                var fileLastWriteTime = File.GetLastWriteTimeUtc(machineListFilePath);
                if (fileLastWriteTime > this._lastWriteTimeUtc)
                {
                    Console.WriteLine("Updating the Machines...");
                    var machines = new Dictionary<string, MachineInfo>(StringComparer.OrdinalIgnoreCase);
                    using (var reader = new StreamReader(machineListFilePath))
                    {
                        Console.WriteLine("Reading the file...");
                        string line = string.Empty;
                        while ((line = reader.ReadLine()) != null && !string.IsNullOrWhiteSpace(line))
                        {
                            Console.WriteLine("Inside file loop...");
                            var parts = line.Split(',');
                            var machine = new MachineInfo()
                            {
                                MachineName = parts[0],
                                Sku = parts[1]
                            };

                            machines[machine.MachineName] = machine;
                        }
                    }

                    this.SetMachines(machines);
                    this._lastWriteTimeUtc = fileLastWriteTime;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

public class MachineInfo
{
    public string MachineName { get; set; }
    public string Sku { get; set; }
    public override string ToString() => this.MachineName;
}
```

**Happy Learning and improving one day at a time**
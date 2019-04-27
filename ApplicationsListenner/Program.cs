using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace ApplicationsListenner
{
    // Data types
    using AppEvent = KeyValuePair<DateTime, KeyValuePair<string, DateTime>>;
    using AppEvents = Dictionary<DateTime, KeyValuePair<string, DateTime>>;
    using AppLog = KeyValuePair<string, Dictionary<DateTime, KeyValuePair<string, DateTime>>>;
    using GeneralLog = Dictionary<string, Dictionary<DateTime, KeyValuePair<string, DateTime>>>;
    using AppState = KeyValuePair<string, KeyValuePair<string, DateTime>>;
    using AppsStates = Dictionary<string, KeyValuePair<string, DateTime>>;

    class Program
    {
        public static void GenerateLog(
            GeneralLog eventsRegistered,
            AppsStates currentStates,
            DateTime currentTime
        ) {
            foreach (AppState window in currentStates)
            {
                string name = window.Key;
                string title = window.Value.Key;

                if (!eventsRegistered.ContainsKey(name))
                    eventsRegistered[name] = new AppEvents();

                eventsRegistered[name][currentStates[name].Value] =
                    new KeyValuePair<string, DateTime>(
                        currentStates[name].Key,
                        currentTime
                    );
            }

            // TODO: Present information somehow else
            StreamWriter log = new StreamWriter(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\" + currentTime.ToString("yyyy-MM-dd") + ".log"
            );

            foreach (AppLog program in eventsRegistered)
            {
                log.WriteLine("--- " + program.Key + " ---");
                foreach (AppEvent register in program.Value)
                {
                    log.WriteLine(
                        register.Key + " - " + register.Value.Value +
                        " ==> " + register.Value.Key
                    );
                }
                log.WriteLine("\n");
            }

            log.Close();
        }

        static void Main(string[] args)
        {
            GeneralLog eventsRegistered = new GeneralLog();
            AppsStates currentStates = new AppsStates();

            DateTime currentTime;
            DateTime lastSave = DateTime.Now;

            while (true)
            {
                Thread.Sleep(5000);
                currentTime = DateTime.Now;
                if((currentTime - lastSave).TotalMinutes >= 1)
                {
                    lastSave = currentTime;
                    GenerateLog(eventsRegistered, currentStates, currentTime);
                }

                List<Process> processes = Process.GetProcesses().Where(
                    p => p.MainWindowHandle != IntPtr.Zero &&
                         p.ProcessName != "explorer"
                ).ToList();
                
                foreach(Process process in processes)
                {
                    string name = process.ProcessName;
                    string title = process.MainWindowTitle;
                    var currentState = new KeyValuePair<string, DateTime>(
                        title,
                        currentTime
                    );

                    // An app stated
                    if (!currentStates.ContainsKey(name))
                        currentStates[name] = currentState;

                    // An app changed its window title
                    else if (currentStates[name].Key != title)
                    {
                        if (!eventsRegistered.ContainsKey(name))
                            eventsRegistered[name] = new AppEvents();

                        eventsRegistered[name][currentStates[name].Value] =
                            new KeyValuePair<string, DateTime>(
                                currentStates[name].Key,
                                currentTime
                            );

                        currentStates[name] = currentState;
                    }
                }

                foreach(AppState window in currentStates)
                {
                    string name = window.Key;
                    string title = window.Value.Key;

                    // An app was closed
                    if(!processes.Any(p => p.ProcessName == name))
                    {
                        if (!eventsRegistered.ContainsKey(name))
                            eventsRegistered[name] = new AppEvents();

                        eventsRegistered[name][currentStates[name].Value] =
                            new KeyValuePair<string, DateTime>(
                                currentStates[name].Key,
                                currentTime
                            );

                        currentStates.Remove(name);
                        break;
                    }
                }
            }
        }
    }
}

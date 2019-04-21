﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace ApplicationsListenner
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventsRegistered = new Dictionary<
                string,
                Dictionary<
                    DateTime,
                    KeyValuePair<
                        string,
                        DateTime
                    >
                >
            >();

            var currentStates = new Dictionary<
                string,
                KeyValuePair<
                    string,
                    DateTime
                >
            >();


            bool running = true;

            new Thread(() => {
                Console.ReadKey(true);
                running = false;
            }).Start();

            DateTime currentTime = DateTime.Now;
            while(running)
            {
                currentTime = DateTime.Now;
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
                            eventsRegistered[name] = new Dictionary<
                                DateTime,
                                KeyValuePair<
                                    string,
                                    DateTime
                                >
                            >();

                        eventsRegistered[name][currentStates[name].Value] =
                            new KeyValuePair<string, DateTime>(
                                currentStates[name].Key,
                                currentTime
                            );

                        currentStates[name] = currentState;
                    }
                }

                foreach(KeyValuePair<string, KeyValuePair<string, DateTime>> window in currentStates)
                {
                    string name = window.Key;
                    string title = window.Value.Key;

                    // An app was closed
                    if(!processes.Any(p => p.ProcessName == name))
                    {
                        if (!eventsRegistered.ContainsKey(name))
                            eventsRegistered[name] = new Dictionary<
                                DateTime,
                                KeyValuePair<
                                    string,
                                    DateTime
                                >
                            >();

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

            // TODO: Present information somehow else
            foreach (KeyValuePair<string, KeyValuePair<string, DateTime>> window in currentStates)
            {
                string name = window.Key;
                string title = window.Value.Key;
                
                if (!eventsRegistered.ContainsKey(name))
                    eventsRegistered[name] = new Dictionary<
                        DateTime,
                        KeyValuePair<
                            string,
                            DateTime
                        >
                    >();

                eventsRegistered[name][currentStates[name].Value] =
                    new KeyValuePair<string, DateTime>(
                        currentStates[name].Key,
                        currentTime
                    );
            }

            foreach (KeyValuePair<string, Dictionary<DateTime, KeyValuePair<string, DateTime>>> program in eventsRegistered)
            {
                Console.WriteLine("--- " + program.Key + " ---");
                foreach(KeyValuePair<DateTime, KeyValuePair<string, DateTime>> register in program.Value)
                {
                    Console.WriteLine(
                        register.Key + " - " + register.Value.Value +
                        " ==> " + register.Value.Key
                    );
                }
                Console.WriteLine("\n");
            }

            Console.ReadKey(true);
        }
    }
}

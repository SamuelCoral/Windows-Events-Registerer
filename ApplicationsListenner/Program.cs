using System;
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
            var pasado = new Dictionary<string, Dictionary<DateTime, KeyValuePair<string, DateTime>>>();
            var presente = new Dictionary<string, KeyValuePair<string, DateTime>>();

            bool correr = true;
            new Thread(() => {
                Console.ReadKey(true);
                correr = false;
            }).Start();

            DateTime ahora = DateTime.Now;
            while(correr)
            {
                ahora = DateTime.Now;
                List<Process> procesos = Process.GetProcesses().Where(
                    p => p.MainWindowHandle != IntPtr.Zero &&
                         p.ProcessName != "explorer"
                ).ToList();
                
                // Contrastar el presente con el instante anterior
                foreach(Process proceso in procesos)
                {
                    string nombre = proceso.ProcessName;
                    string titulo = proceso.MainWindowTitle;
                    var estadoActual = new KeyValuePair<string, DateTime>(titulo, ahora);

                    // Si una aplicación inició
                    if (!presente.ContainsKey(nombre)) presente[nombre] = estadoActual;
                    // Si una aplicación cambió su barra de título
                    else if (presente[nombre].Key != titulo)
                    {
                        if (!pasado.ContainsKey(nombre))
                            pasado[nombre] = new Dictionary<DateTime, KeyValuePair<string, DateTime>>();

                        pasado[nombre][presente[nombre].Value] =
                            new KeyValuePair<string, DateTime>(presente[nombre].Key, ahora);

                        presente[nombre] = estadoActual;
                    }
                }

                // Contrastar el instante anterior con el pasado
                foreach(KeyValuePair<string, KeyValuePair<string, DateTime>> ventana in presente)
                {
                    string nombre = ventana.Key;
                    string titulo = ventana.Value.Key;
                    // Si una aplicación se cerró
                    if(!procesos.Any(p => p.ProcessName == nombre))
                    {
                        if (!pasado.ContainsKey(nombre))
                            pasado[nombre] = new Dictionary<DateTime, KeyValuePair<string, DateTime>>();

                        pasado[nombre][presente[nombre].Value] =
                            new KeyValuePair<string, DateTime>(presente[nombre].Key, ahora);

                        presente.Remove(nombre);
                        break;
                    }
                }
            }

            // Cerrar la aplicación
            foreach (KeyValuePair<string, KeyValuePair<string, DateTime>> ventana in presente)
            {
                string nombre = ventana.Key;
                string titulo = ventana.Value.Key;
                
                if (!pasado.ContainsKey(nombre))
                    pasado[nombre] = new Dictionary<DateTime, KeyValuePair<string, DateTime>>();

                pasado[nombre][presente[nombre].Value] =
                    new KeyValuePair<string, DateTime>(presente[nombre].Key, ahora);
            }

            foreach (KeyValuePair<string, Dictionary<DateTime, KeyValuePair<string, DateTime>>> programa in pasado)
            {
                Console.WriteLine("--- " + programa.Key + " ---");
                foreach(KeyValuePair<DateTime, KeyValuePair<string, DateTime>> registro in programa.Value)
                {
                    Console.WriteLine(registro.Key + " - " + registro.Value.Value + " ==> " + registro.Value.Key);
                }
                Console.WriteLine("\n");
            }

            Console.ReadKey(true);
        }
    }
}

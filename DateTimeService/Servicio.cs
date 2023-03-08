using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DateTimeService
{
    class Servicio
    {
        private bool finaliza;
        public bool Finaliza { set; get; }

        public void writeEvent(string mensaje)
        {
            string nombre = "DateTimeService";
            string logDestino = "Application";
            if (!EventLog.SourceExists(nombre))
            {
                EventLog.CreateEventSource(nombre, logDestino);
            }
            EventLog.WriteEntry(nombre, mensaje);
        }

        public void Init()
        {
            int port;

            string rutaPuerto = Environment.ExpandEnvironmentVariables("%PROGRAMDATA%") + "\\port.config";
            try
            {
                using (StreamReader sr = new StreamReader(rutaPuerto))
                {
                    port = Convert.ToInt32(sr.ReadLine());
                }
            }
            catch (Exception e)
            {
                writeEvent("Error al leer el archivo");
                port = 31416;
            }

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);

            writeEvent("Puerto de escucha: " + ie.Port);
            
            try
            {
                s.Bind(ie);
                s.Listen(10);

            }
            catch (SocketException e)
            {
                writeEvent("Socket: " + e.Message);
            }
            catch (Exception e)
            {
                writeEvent("Excepcion servidor distinta de SocketException");
            }

            while (!Finaliza)
            {
                try
                {
                    Socket sClient = null;

                    if (!Finaliza)
                    {
                        sClient = s.Accept();
                        IPEndPoint ieClient = (IPEndPoint)sClient.RemoteEndPoint;
                        Console.WriteLine("Client connected:{0} at port {1}", ieClient.Address, ieClient.Port);

                        using (NetworkStream ns = new NetworkStream(sClient))
                        using (StreamReader sr = new StreamReader(ns))
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            string welcome = "Welcome";
                            sw.WriteLine(welcome);
                            sw.Flush();
                            DateTime dt = DateTime.Now;
                            string msg = "";

                            try
                            {
                                msg = sr.ReadLine();
                                switch (msg)
                                {
                                    case "time":
                                        sw.WriteLine(dt.Hour + ":" + dt.Minute + ":" + dt.Second);
                                        sw.Flush();
                                        break;

                                    case "date":
                                        sw.WriteLine(dt.ToLongDateString());
                                        sw.Flush();
                                        if (msg == "all")
                                        {
                                            goto case "time";
                                        }
                                        break;

                                    case "all":
                                        goto case "date";

                                    case string pass when pass.StartsWith("close "):
                                        string contrasenaInsertada = msg.Substring(6);

                                        if (contrasenaInsertada != "")
                                        {
                                            string ruta = Environment.ExpandEnvironmentVariables("%PROGRAMDATA%") + "\\contrasena.txt";
                                            using (StreamReader srArch = new StreamReader(ruta))
                                            {
                                                if (srArch.ReadToEnd() == contrasenaInsertada)
                                                {
                                                    sw.WriteLine("La contraseña es correcta");
                                                    sw.Flush();
                                                    Finaliza = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    sw.WriteLine("Error. La contraseña insertada no es correcta");
                                                    sw.Flush();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sw.WriteLine("Error. No ha ingresado la contraseña");
                                            sw.Flush();
                                        }
                                        break;

                                    default:
                                        sw.WriteLine("Error. El comando insertado no es correcto");
                                        sw.Flush();
                                        break;
                                }
                            }
                            catch (IOException e)
                            {
                                msg = null;
                            }
                        }
                        
                    }

                    Console.WriteLine("Client disconnected.\nConnection closed");
                    sClient.Close();
                }
                catch (Exception e)
                {
                    writeEvent("Excepcion cliente");
                }
            }
            s.Close();
        }
    }
}

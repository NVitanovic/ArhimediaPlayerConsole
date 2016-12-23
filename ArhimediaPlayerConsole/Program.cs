using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using CIITLAB_UltraSonic2016;
using System.Diagnostics;
using System.IO.Ports;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.InteropServices;

namespace ArhimediaPlayerConsole
{
    class Program
    {
        protected static WebClient client;
        protected static Config configuration = null;
        protected static bool currentState; //ON - true, //OFF - false
        protected static Process videoApplication;
        protected static int currentDistance;
        protected static int numOfReads;
        protected static SerialPort serialClient;
        protected static int lastDistance = 0;
        const Int32 SW_MINIMIZE = 6;

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] Int32 nCmdShow);

        private static void MinimizeConsoleWindow()
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_MINIMIZE);
        }
        static void initSerial()
        {
            try
            {
                serialClient = new SerialPort();
                serialClient.PortName = configuration.comPortSettings.port;
                serialClient.BaudRate = configuration.comPortSettings.baudRate;
                serialClient.Parity = Parity.None;
                serialClient.Handshake = Handshake.None;
                serialClient.DataBits = 8;
                serialClient.StopBits = StopBits.One;
                if (!serialClient.IsOpen)
                    serialClient.Open();
                else
                {
                    serialClient.Close();
                    serialClient.Open();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException + "\n\n\n" + e.Message + "\n\n\n" + e.StackTrace);
            }
        }
        static void Main(string[] args)
        {
            MinimizeConsoleWindow();
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            StreamReader reader = new StreamReader("config.xml");
            configuration = (Config)serializer.Deserialize(reader);
            reader.Close();
            client = new WebClient();
            currentState = false;
            videoApplication = new Process();

            videoApplication.StartInfo.FileName = configuration.applicationPath;
            videoApplication.StartInfo.UseShellExecute = true;
            currentDistance = 0;

            if (configuration.useWifi)
                client.OpenRead("http://" + configuration.serverAddress);
            else
            {
                initSerial();
            }

            Thread t = new Thread(backgroundTask);
            t.Start();
        }
        private static string ReadNumFromSerial()
        {

            byte[] buff = new byte[10];
            char b = '0';
            string val = "";

            while (b != '\n')
            {
                if (b == -1)
                    return null;
                val += b;
                b = (char)serialClient.BaseStream.ReadByte();
            }
            return val;
        }
        private static void backgroundTask()
        {

            int numOfResults = 0;
            while (true)
            {
                int distance = 0;
                lastDistance = distance;
                for (int i = 0; i < configuration.numberOfSamples; i++)
                {
                    int read = Convert.ToInt32(ReadNumFromSerial());
                    distance += read;
                    Console.WriteLine("---------" + read);
                    Thread.Sleep(configuration.samplingTime);
                }

                distance /= configuration.numberOfSamples;
                Console.WriteLine("-" + distance);

                if (distance >= configuration.minDistance && distance <= configuration.maxDistance)
                {
                    numOfResults++;
                }
                else
                {
                    numOfResults--;
                }
                if (numOfResults >= configuration.numberOfSamples / 3)
                {
                    if (!currentState)
                    {
                        videoApplication.Start();
                        currentState = true;
                        
                    }
                    numOfResults = 0;
                }
                else if(numOfResults <= -configuration.numberOfSamples / 3)
                {
                    if (currentState)
                    {
                        videoApplication.Kill();
                        currentState = false;
                    }
                    numOfResults = 0;
                }
                Console.Title = "STATE: " + currentState + " numOfResults: " + numOfResults + " config min max: " + configuration.minDistance + "," + configuration.maxDistance;
            }
                
        }

    }   
}

﻿using Microsoft.Owin.Hosting;
using OpenRastaAPIProject;

namespace OpenRasta.Owin.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string uri = "http://localhost:8080/";

            WebApp.Start<Startup>(uri);

            System.Console.ReadLine();
        }
    }
}
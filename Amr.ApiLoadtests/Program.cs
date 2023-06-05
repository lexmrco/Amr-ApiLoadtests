// See https://aka.ms/new-console-template for more information
using Amr.ApiLoadtests;

Console.WriteLine("Hello, World!");
ApiLoadtestHelper ApiLoadTest = new ApiLoadtestHelper("http://18.188.64.91:8010/mtmessage");
await ApiLoadTest.RestLoadTest(1000, 50); // 1.000 Clientes. 50 solicitudes por cliente. Tiempo promedio 16 segundos


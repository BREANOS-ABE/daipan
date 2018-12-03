//-----------------------------------------------------------------------

// <copyright file="Program.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CWFStateless
{
    internal static class Program
    {
        /// <summary>
        /// Dies ist der Einstiegspunkt des Diensthostprozesses.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Die Datei "ServiceManifest.XML" definiert mindestens einen Diensttypnamen.
                // Durch die Registrierung eines Diensts wird ein Diensttypname einem .NET-Typ zugeordnet.
                // Wenn Service Fabric eine Instanz dieses Diensttyps erstellt,
                // wird eine Instanz der Klasse in diesem Hostprozess erstellt.

                ServiceRuntime.RegisterServiceAsync("CWFStatelessType",
                    context => new CWFStateless(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(CWFStateless).Name);

                // Verhindert, dass dieser Hostprozess beendet wird, damit die Dienste weiterhin ausgeführt werden.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}

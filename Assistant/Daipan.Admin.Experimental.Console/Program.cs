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

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace Daipan.Admin.Assistent
{
    class Program
    {
        [Verb("add", HelpText = "Add file contents to the index.")]
        class AddOptions
        {
            [Option]
            public string UserId { get; set; }
            //normal options here
            public AddOptions()
            {

            }
        }
        [Verb("commit", HelpText = "Record changes to the repository.")]
        class CommitOptions
        {
            //normal options here
            public CommitOptions()
            {

            }

        }
        [Verb("clone", HelpText = "Clone a repository into a new directory.")]
        class CloneOptions
        {
            //normal options here
            public CloneOptions()
            {

            }
        }

        [Verb("exit", HelpText = "Exit from this application.")]
        class ExitOptions
        {
            //normal options here
            public ExitOptions()
            {

            }
        }

        [Verb("mute", HelpText = "Mutes the speech output.")]
        class MuteOptions
        {
            //normal options here
            public MuteOptions()
            {

            }
        }

        [Verb("worker", HelpText = "Lists all Worker instances and gets its state.")]
        class WorkerOptions
        {
            //normal options here
            public WorkerOptions()
            {

            }
        }

        private static List<VoiceInfo> GetInstalledVoices()
        {
            var listOfVoiceInfo = from voice
                                  in speaker.GetInstalledVoices()
                                  select voice.VoiceInfo;

            return listOfVoiceInfo.ToList<VoiceInfo>();
        }


        static bool _exit;
        private static SpeechSynthesizer speaker;
        static string userName;

        static void Main(string[] args)
        {           
            speaker = new SpeechSynthesizer();

            List<VoiceInfo> list = GetInstalledVoices();

            //In dem Fall unnötig, aber falls zB vorher OutputToWav eingestellt war
            speaker.SetOutputToDefaultAudioDevice();
            //Geschwindigkeit (-10 - 10)
            speaker.Rate = -3;
            //Lautstärke (0-100)
            speaker.Volume = 100;
          
            //Such passende Stimme zu angegebenen Argumenten
            speaker.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Senior);
            //Text wird ausgegeben (abbrechen mit speaker.CancelAsync())
            userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            int idx = userName.IndexOf('\\');
            userName = userName.Substring(idx+1, userName.Length-idx-1);
            speaker.Speak("Hello" + userName +"good to see you");
            speaker.Speak("Please enter a command");

            while (_exit == false)
            {
                System.Console.Write("Please enter a command ");
                string str = System.Console.ReadLine();
                args = str.Split(' ');

                int i = CommandLine.Parser.Default.ParseArguments<AddOptions, CommitOptions, CloneOptions, ExitOptions, MuteOptions, WorkerOptions>(args)
                .MapResult(
                  (AddOptions opts) => RunAddAndReturnExitCode(opts),
                  (CommitOptions opts) => RunCommitAndReturnExitCode(opts),
                  (CloneOptions opts) => RunCloneAndReturnExitCode(opts),
                  (ExitOptions opts) => ExitAndReturnExitCode(opts),
                  (MuteOptions opts) => MuteAndReturnExitCode(opts),
                  (WorkerOptions opts) => WorkerAndReturnExitCode(opts),
                  (errs) => HandleParseError(errs));
             
                if (i == 7) _exit = true;
            } 
        }

        private static int WorkerAndReturnExitCode(WorkerOptions opts)
        {
            System.Console.WriteLine("Command worker was selected");
            speaker.Speak("Fetching workers from database");
            return 1;
        }

        private static int MuteAndReturnExitCode(MuteOptions opts)
        {
            System.Console.WriteLine("Command mute was selected");
            speaker.Speak("Voice output muted");
            speaker.Speak("You will not hear me anymore");
            speaker.Pause();
            return 1;
        }

        static int ExitAndReturnExitCode(ExitOptions opts)
        {
            try
            {
                speaker.Speak("Good bye" + userName);
            }
            catch (Exception e)
            {

            }

            System.Console.WriteLine("Command exit was selected");
            return 7;
        }

        static int RunAddAndReturnExitCode(AddOptions opts)
        {
            System.Console.WriteLine("Command add was selected");
            return 1;
        }

        private static int RunCommitAndReturnExitCode(CommitOptions opts)
        {
            System.Console.WriteLine("Command commit was selected");
            return 1;
        }

        private static int RunCloneAndReturnExitCode(CloneOptions opts)
        {
            System.Console.WriteLine("Command clone was selected");
            return 1;
        }
        static int HandleParseError(IEnumerable<Error> errs)
        {
            System.Console.WriteLine("Syntax error ready.");
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechTest
{
    class Program
    {
        private static readonly ManualResetEvent WaitEvent = new ManualResetEvent(false);

        static void Main()
        {
            PrintInstalledVoices();
            SpeechSynthesis();
            SpeechRecognition();
        }

        private static void PrintInstalledVoices()
        {
            using (var synth = new SpeechSynthesizer())
            {
                Console.WriteLine("Stimmen:");
                foreach (var voice in synth.GetInstalledVoices())
                {
                    var voiceInfo = voice.VoiceInfo;
                    Console.WriteLine(" * {0} ({1})", voiceInfo.Name, voiceInfo.Culture);
                }
            }
        }

        private static void SpeechSynthesis()
        {
            using (var synth = new SpeechSynthesizer())
            {
                synth.SetOutputToDefaultAudioDevice();

                foreach (var phrase in GetThingsToSay())
                {
                    synth.Speak(phrase);
                }
            }
        }

        static IEnumerable<string> GetThingsToSay()
        {
            while (true)
            {
                Console.WriteLine("Was soll ich sagen? (Enter zum Beenden)");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    yield break;
                }

                yield return line;
            }
        }

        private static void SpeechRecognition()
        {
            using (var engine = new SpeechRecognitionEngine())
            {
                var phrases = new[] {"Apfel", "Käse", "Wurst", "Ende"};
                var choices = new Choices(phrases);
                var builder = new GrammarBuilder(choices);
                var grammar = new Grammar(builder);
                engine.LoadGrammar(grammar);
                engine.SetInputToDefaultAudioDevice();
                engine.SpeechRecognized += EngineOnSpeechRecognized;
                engine.RecognizeAsync(RecognizeMode.Multiple);

                Console.WriteLine("Spracherkennung läuft. Erkannt werden: {0}.", string.Join(", ", phrases));
                Console.WriteLine("Zum Beenden Enter-Taste drücken oder 'Ende' sagen.");
               
                WaitForExit();
            }
        }

        private static void EngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            Console.WriteLine("Erkannt: {0}", speechRecognizedEventArgs.Result.Text);
            if (speechRecognizedEventArgs.Result.Text == "Ende")
            {
                WaitEvent.Set();
            }
        }

        private static void WaitForExit()
        {
            Task.Run(() =>
                     {
                         Console.ReadLine();
                         WaitEvent.Set();
                     });
            WaitEvent.WaitOne();
        }
    }
}
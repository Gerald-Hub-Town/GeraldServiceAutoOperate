using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace KillProcessInstaller.Common
{
    public class Speech
    {
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        SpeechSynthesizer speech = new SpeechSynthesizer();

        public void SpeechVideo_Read(int rate, int volume, string speektext)
        {
            speech.Rate = rate;
            speech.Volume = volume;
            speech.Speak(speektext);
        }

        public void CountDownRead(int rate, int volume, int count)
        {
            speech.Rate = rate;
            speech.Volume = volume;
            speech.Speak("生产系统关闭倒计时开始");
            for (int i = count; i > 0; i--)
            {
                speech.Speak(i.ToString());
            }
        }
    }
}

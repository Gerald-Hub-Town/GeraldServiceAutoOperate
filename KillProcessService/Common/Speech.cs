using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace KillProcessService.Common
{
    public class Speech
    {
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        SpeechSynthesizer speech = new SpeechSynthesizer();

        public void SpeechTextRead(int rate, int volume, string speektext)
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
        //SoundPlayer播放本地音频
        //项目添加引用→COM类型库：Windows Media Player
        //SoundPlayer play = new SoundPlayer();
        //play.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "music.wav"; //本地音频位置，这里放在了当前项目bin→debug下
        //play.Load();  //加载声音
        //play.Play(); //播放
    }
}

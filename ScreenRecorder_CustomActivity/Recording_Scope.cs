using Accord.Video.FFMPEG;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Activities.Statements;
using System.Threading;
using ScreenRecorder_CustomActivity;

namespace Screen_Recorder
{
    [Designer(typeof(RecordingScope_Designer))]
    public class Recording_Scope : NativeActivity
    {
        [Category("Input")]
        [RequiredArgument]
        public InArgument<String> FilePath { get; set; }

        [Browsable(false)]
        public OutArgument<VideoFileWriter> VideoWriter { get; set; }

        [Browsable(false)]
        public ActivityAction<string> Body { get; set; }


        public static VideoFileWriter vf;
    
        public Recording_Scope()
        {
            Console.WriteLine("Recording Started...");
            Body = new ActivityAction<string>
            {

                Argument = new DelegateInArgument<string>("FilePath"),
                Handler = new Sequence { DisplayName = "Do" }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            string filePath = FilePath.Get(context);

            Thread t = new Thread(new ParameterizedThreadStart(Start_Recording));
            t.Start(filePath);

            //Start_Recording(FilePath.Get(context));

            if (Body != null)
            {
                //scheduling the execution of the child activities
                // and passing the value of the delegate argument
                context.ScheduleAction<string>(Body, filePath, OnCompleted, OnFaulted);
            }

        }

        public static void Start_Recording(object file)
        {
            try
            {
                vf = new Accord.Video.FFMPEG.VideoFileWriter();

                vf.Open(file as string, 1600, 900, 25, VideoCodec.MPEG4, 1000000);

                while (true)
                {
                    using (var bitmap = timer1_Tick())
                    {
                        if (bitmap == null) { break; }
                        //#if (debug == true)                 
                        
                        vf.WriteVideoFrame(bitmap);////// THIS LINE ////////
                       
                    }
                }

                //VideoWriter.Set(context, vf);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }

        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            Console.WriteLine("Recording Stopped...");
            vf.Close();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Console.WriteLine("Recording Completed...");
            vf.Close();
        }

        public static Bitmap timer1_Tick()
        {
            Bitmap bitmap = new Bitmap(1600, 900);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(bitmap.Width, bitmap.Height));
            }
            return bitmap;
        }
    }
}

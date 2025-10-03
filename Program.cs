using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreenRecorder
{
    class Program
    {
        private static Recorder recorder;
        private static string outputPath;

        static void Main(string[] args)
        {
            Console.WriteLine("=== Screen Recorder CLI ===");
            Console.WriteLine("Commands: start | pause | resume | stop | exit");
            Console.WriteLine("---------------------------");

            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(command))
                    continue;

                switch (command)
                {
                    case "start":
                        StartRecording();
                        break;
                    case "pause":
                        PauseRecording();
                        break;
                    case "resume":
                        ResumeRecording();
                        break;
                    case "stop":
                        StopRecording();
                        break;
                    case "exit":
                        if (recorder != null)
                        {
                            Console.WriteLine("⚠ Recorder still active, stopping...");
                            recorder.Stop();
                        }
                        return;
                    default:
                        Console.WriteLine("Unknown command. Use: start | pause | resume | stop | exit");
                        break;
                }
            }
        }

        private static void StartRecording()
        {
            if (recorder != null)
            {
                Console.WriteLine("⚠ A recording is already in progress.");
                return;
            }

            // Ensure output folder
            string videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string recordingsFolder = Path.Combine(videosFolder, "ScreenRecordings");
            Directory.CreateDirectory(recordingsFolder);

            outputPath = Path.Combine(recordingsFolder,
                $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            Console.WriteLine($"Saving to: {outputPath}");

            // Choose main monitor
            var sources = new List<RecordingSourceBase>
            {
                new DisplayRecordingSource(DisplayRecordingSource.MainMonitor)
            };

            // Recorder options
            var options = new RecorderOptions
            {
                SourceOptions = new SourceOptions
                {
                    RecordingSources = sources
                },
                AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                    IsOutputDeviceEnabled = true, // system sound
                    IsInputDeviceEnabled = true,  // mic
                    Bitrate = AudioBitrate.bitrate_128kbps
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    Framerate = 30,
                    IsFixedFramerate = true,
                    Bitrate = 8000 * 1000, // 8 Mbps
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.CBR,
                        EncoderProfile = H264Profile.Main
                    }
                }
            };

            recorder = Recorder.CreateRecorder(options);

            // Hook events
            recorder.OnRecordingComplete += (s, e) =>
                Console.WriteLine($"✅ Recording complete. File saved: {e.FilePath}");
            recorder.OnRecordingFailed += (s, e) =>
                Console.WriteLine($"❌ Recording failed: {e.Error}");
            recorder.OnStatusChanged += (s, e) =>
                Console.WriteLine($"ℹ Status: {e.Status}");

            Console.WriteLine("🎥 Starting recording...");
            recorder.Record(outputPath);
        }

        private static void PauseRecording()
        {
            if (recorder == null)
            {
                Console.WriteLine("⚠ No active recording.");
                return;
            }
            recorder.Pause();
            Console.WriteLine("⏸ Recording paused.");
        }

        private static void ResumeRecording()
        {
            if (recorder == null)
            {
                Console.WriteLine("⚠ No active recording.");
                return;
            }
            recorder.Resume();
            Console.WriteLine("▶ Recording resumed.");
        }

        private static void StopRecording()
        {
            if (recorder == null)
            {
                Console.WriteLine("⚠ No active recording.");
                return;
            }
            recorder.Stop();
            recorder = null; // reset so you can start again
            Console.WriteLine("⏹ Recording stopped.");
        }
    }
}

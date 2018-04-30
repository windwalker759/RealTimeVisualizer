using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFSoundVisualizationLib;

namespace RealTimeVisualizer
{
    public partial class MainWindow : Window, ISpectrumPlayer
    {
        public MainWindow()
        {
            InitializeComponent();
            bwp = new BufferedWaveProvider(wasapi.WaveFormat);
            wasapi.DataAvailable += Wasapi_DataAvailable;
            bwp.BufferLength = (int)Math.Pow(2, 11);
            bwp.DiscardOnBufferOverflow = true;
            wasapi.StartRecording();
            Analyzer.RegisterSoundPlayer(this);
        }
        public BufferedWaveProvider bwp;
        public static WasapiLoopbackCapture wasapi = new WasapiLoopbackCapture();
        private void Wasapi_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (!e.Buffer.ToList().TrueForAll(x => x == 0))
                bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
        public bool IsPlaying => true;
        public event PropertyChangedEventHandler PropertyChanged;
        public bool GetFFTData(float[] data)
        {
            if (bwp.BufferedBytes == 0) return true; // if captured buffer is null then return null fft data
            int count = bwp.BufferedBytes; //count of buffered bytes
            float[] fft = new float[count]; //creating empty float array for future filling
            Complex[] comp = new Complex[count]; //creating empty complex array for future filling
            bwp.ToSampleProvider().Take(bwp.BufferedDuration).Read(fft, 0, count); //filling float array with captured data
            for (int i = 0; i < count && fft[i] != 0; i++)
                comp[i] = new Complex(fft[i], 0.0); // filling complex array
            Accord.Math.FourierTransform.FFT(comp, Accord.Math.FourierTransform.Direction.Forward); //perfroming fft transform
            for (int i = 0; i < fft.Length; i++)
                data[i] = (float)comp[i].Magnitude; //load fft data in specified array
            return IsPlaying; // always return true because we have never-ending wasapiloppback
        }

        readonly int fftDataSize = (int)FFTDataSize.FFT2048;
        public int GetFFTFrequencyIndex(int frequency) => (int)((frequency / (wasapi.WaveFormat.SampleRate / 2.0d)) * (fftDataSize / 2));
    }
}

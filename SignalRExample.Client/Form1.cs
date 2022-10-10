using AForge.Video.DirectShow;
using System.Linq;
using AForge.Video;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRExampleRazorPages.Shared.Models;

namespace SignalRExample.Client
{
    public partial class Form1 : Form
    {
        HubConnection connection;
        bool isCameraRunning = false;
        bool isMicrophoneJustStarted = false;
        private VideoCaptureDevice _maiDevice;
        private CamaraDeviceInfo slectedInfo;
        private List<CamaraDeviceInfo> devicesInfos  = new List<CamaraDeviceInfo>();

        //VideoCapture capture;
        //VideoWriter outputVideo;
        //Recording audioRecorder;

        //Mat frame;
        Bitmap imageAlternate;
        Bitmap image;
        bool isUsingImageAlternate = false;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7274/camarahub")
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
                .Build();
            
            await connection.StartAsync();

            lblStatus.Text = "";

            //var videoDevices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (var device  in devices)
            {
                if (device is FilterInfo camara)
                {
                    var info = new CamaraDeviceInfo(camara);
                    devicesInfos.Add(info);
                    ddlVideoDevices.Items.Add(info);
                }
            }

            await connection.SendAsync("SendCamaras", devicesInfos.Select(x => new CamaraModel
            {
                Model = x.FilterInfo.Name,
                SourceName = x.FilterInfo.MonikerString,
                Id = x.Id
            }));

        }

        private async void btnRecord_Click(object sender, EventArgs e)
        {
            if (ddlVideoDevices.SelectedIndex < 0)
            {
                MessageBox.Show("Please choose a video device as the Video Source.", "Video Source Not Defined", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!isCameraRunning)
            {
                lblStatus.Text = "Starting recording...";

                StartCamera(ddlVideoDevices.SelectedItem as CamaraDeviceInfo);
                isCameraRunning = true;
                await connection.SendAsync("PostData", new CaramaraStream
                {
                    sourceName = slectedInfo.FilterInfo.MonikerString,
                    src = "test"

                }, cancellationToken: CancellationToken.None);
                await connection.SendAsync("SendCamaras", devicesInfos.Select(x => new CamaraModel
                {
                    Model = x.FilterInfo.Name,
                    SourceName = x.FilterInfo.MonikerString,
                    Id = x.Id
                }));
                lblStatus.Text = "Recording...";
            }
            else
            {
               
               _maiDevice.NewFrame -= MaiDeviceOnNewFrame;

              await Task.Delay(1000);
                _maiDevice.Stop();
                lblStatus.Text = "Recording ended.";

                isCameraRunning = false;
            }
        }

        private void StartCamera(CamaraDeviceInfo? deviceInfo)
        {
            if(deviceInfo is null) return;
            _maiDevice = new VideoCaptureDevice(deviceInfo.FilterInfo.MonikerString);
            _maiDevice.NewFrame +=  MaiDeviceOnNewFrame;
            _maiDevice.Start();
            slectedInfo = deviceInfo;
            //DisposeCameraResources();

            //isCameraRunning = true;

            //btnRecord.Text = "Stop";

            //int deviceIndex = ddlVideoDevices.SelectedIndex;
            //capture = new VideoCapture(deviceIndex);
            //capture.Open(deviceIndex);

            //outputVideo = new VideoWriter("video.mp4", FourCC.AVC, 29, new OpenCvSharp.Size(640, 480));
        }

        private void _maiDevice_SnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private async void MaiDeviceOnNewFrame(object sender, NewFrameEventArgs eventargs)
        {
           using var  memoryStream = new MemoryStream();
            (eventargs.Frame.Clone() as Bitmap).Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            string base64ImageRepresentation = Convert.ToBase64String(memoryStream.ToArray());
           await connection.SendAsync("PostData", new CaramaraStream
            {
                 sourceName = slectedInfo.FilterInfo.MonikerString,
                 src = $"data:image/png;base64,{base64ImageRepresentation}",
                 id = slectedInfo.Id

           },cancellationToken:CancellationToken.None);

            //pictureBox1.Image = (Bitmap)eventargs.Frame.Clone();
        }

        private void StopCamera()
        {
            isCameraRunning = false;

            btnRecord.Text = "Start";

            recordingTimer.Stop();
            recordingTimer.Enabled = false;

            DisposeCaptureResources();
        }

        private void StartMicrophone()
        {
            //audioRecorder = new Recording();
            //audioRecorder.Filename = "sound.wav";
            //isMicrophoneJustStarted = true;
        }

        private void StopMicrophone()
        {
            //audioRecorder.StopRecording();
        }

        private async Task OutputRecordingAsync()
        {
            //string outputPath = $"output_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.mp4";

            //try
            //{
            //    FFMpeg.ReplaceAudio("video.mp4", "sound.wav", outputPath, true);

            //    lblStatus.Text = $"Recording saved to local disk with the file name {outputPath}.";

            //    string azureStorageConnectionString = txtAzureStorageConnectionString.Text;
            //    if (!string.IsNullOrWhiteSpace(azureStorageConnectionString))
            //    {
            //        try
            //        {
            //            BlobServiceClient blobServiceClient = new BlobServiceClient(azureStorageConnectionString);
            //            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("webcam-videos");
            //            BlobClient blobClient = containerClient.GetBlobClient(outputPath);

            //            using FileStream uploadFileStream = File.OpenRead(outputPath);
            //            await blobClient.UploadAsync(uploadFileStream, true);
            //            uploadFileStream.Close();

            //            lblStatus.Text = $"Recording saved to both local disk and Azure Blob Storage with the file name {outputPath}.";
            //        }
            //        catch (Exception ex)
            //        {
            //            lblStatus.Text = $"Recording saved to both local disk with the file name {outputPath} but cannot be saved on Azure Blob Storage.";
            //            MessageBox.Show(ex.Message, "Error on Saving to Azure Blob Storage", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    lblStatus.Text = "Recording cannot be saved.";

            //    MessageBox.Show($"Recording cannot be saved because {ex.Message}", "Error on Recording Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void DisposeCameraResources()
        {
            //if (frame != null)
            //{
            //    frame.Dispose();
            //}

            //if (image != null)
            //{
            //    image.Dispose();
            //}

            //if (imageAlternate != null)
            //{
            //    imageAlternate.Dispose();
            //}
        }

        private void DisposeCaptureResources()
        {
            //if (capture != null)
            //{
            //    capture.Release();
            //    capture.Dispose();
            //}

            //if (outputVideo != null)
            //{
            //    outputVideo.Release();
            //    outputVideo.Dispose();
            //}
        }

        private void recordingTimer_Tick(object sender, EventArgs e)
        {
            //if (capture.IsOpened())
            //{
            //    try
            //    {
            //        frame = new Mat();
            //        capture.Read(frame);
            //        if (frame != null)
            //        {
            //            if (imageAlternate == null)
            //            {
            //                isUsingImageAlternate = true;
            //                imageAlternate = BitmapConverter.ToBitmap(frame);
            //            }
            //            else if (image == null)
            //            {
            //                isUsingImageAlternate = false;
            //                image = BitmapConverter.ToBitmap(frame);
            //            }

            //            pictureBox1.Image = isUsingImageAlternate ? imageAlternate : image;

            //            outputVideo.Write(frame);
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        pictureBox1.Image = null;
            //    }
            //    finally
            //    {
            //        if (frame != null)
            //        {
            //            frame.Dispose();
            //        }

            //        if (isUsingImageAlternate && image != null)
            //        {
            //            image.Dispose();
            //            image = null;
            //        }
            //        else if (!isUsingImageAlternate && imageAlternate != null)
            //        {
            //            imageAlternate.Dispose();
            //            imageAlternate = null;
            //        }
            //    }

            //    if (isMicrophoneJustStarted)
            //    {
            //        audioRecorder.StartRecording();
            //        isMicrophoneJustStarted = false;
            //    }
            //}
        }
    }
}
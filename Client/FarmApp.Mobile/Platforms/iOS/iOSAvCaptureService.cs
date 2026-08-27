using AVFoundation;
using FarmApp.Mobile.Services.Interfaces;
using Foundation;
using UIKit;

namespace FarmApp.Mobile.Platforms.iOS
{
    public class iOSAvCaptureService : IMediaCaptureService
    {
        AVCaptureSession? _session;
        AVCaptureDeviceInput _videoInput;

        AVCaptureMovieFileOutput? _movieOutput;
        AVCapturePhotoOutput? _photoOutput;

        UIViewController? _controller;

        TaskCompletionSource<FileResult?>? _tcs;

        public async Task<FileResult?> CapturePhotoOrVideoAsync()
        {
            _tcs = new TaskCompletionSource<FileResult?>();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _controller = new CameraViewController(this);
                Platform.GetCurrentUIViewController()
                    ?.PresentViewController(_controller, true, null);
            });

            return await _tcs.Task;
        }

        internal void SetupSession()
        {
            _session = new AVCaptureSession
            {
                SessionPreset = AVCaptureSession.PresetHigh,
            };

            var device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, AVCaptureDevicePosition.Back);


            if (device == null)
            {
                Cancel();
                return;
            }

            _videoInput = AVCaptureDeviceInput.FromDevice(device);



            _session.AddInput(_videoInput);

            _photoOutput = new AVCapturePhotoOutput();
            _session.AddOutput(_photoOutput);

            _movieOutput = new AVCaptureMovieFileOutput();
            _session.AddOutput(_movieOutput);

            _session.StartRunning();
        }

        internal AVCaptureSession Session => _session!;

        internal void TakePhoto()
        {
            var settings = AVCapturePhotoSettings.Create();
            _photoOutput?.CapturePhoto(settings, new PhotoDelegate(this));
        }

        internal void StartVideo()
        {
            var path = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.mov");

            _movieOutput?.StartRecordingToOutputFile(NSUrl.FromFilename(path), new VideoDelegate(this, path));
        }
        internal void StopVideo()
        {
            _movieOutput?.StopRecording();
        }

        internal void SwitchCamera()
        {
            var newPos = _videoInput.Device.Position == AVCaptureDevicePosition.Back 
                ? AVCaptureDevicePosition.Front 
                : AVCaptureDevicePosition.Back;

            var device = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, newPos);

            if (device == null)
            {
                Cancel();
                return;
            }

            var newInput = AVCaptureDeviceInput.FromDevice(device);

            if (newInput == null)
            {
                Cancel();
                return;
            }
            _session?.BeginConfiguration();
            _session?.RemoveInput(_videoInput);
            _session?.AddInput(newInput);
            _session?.CommitConfiguration();

            _videoInput = newInput;
        }
        internal void ToggleFlash()
        {
            var device = _videoInput.Device;
            if (!device.HasTorch)
                return;

            device.LockForConfiguration(out _);

            device.TorchMode = device.TorchMode == AVCaptureTorchMode.On 
                ? AVCaptureTorchMode.Off : AVCaptureTorchMode.On;

            device.UnlockForConfiguration();
        }
        internal void Complete(string path)
        {
            _controller?.DismissViewController(false, null);
            _tcs?.TrySetResult(new FileResult(path));
        }
        internal void Cancel()
        {
            _controller?.DismissViewController(true, null);
            _tcs?.TrySetResult(null);
        }

        private class PhotoDelegate : AVCapturePhotoCaptureDelegate
        {
            iOSAvCaptureService _parent;
            public PhotoDelegate(iOSAvCaptureService parent)
            {
                _parent = parent;
            }
            public override void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError? error)
            {
                if (error != null)
                {
                    _parent.Cancel();
                    return;
                }

                var data = photo.FileDataRepresentation;
                var path = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");

                File.WriteAllBytes(path, data.ToArray());

                _parent.Complete(path);
            }
        }

        private class VideoDelegate : AVCaptureFileOutputRecordingDelegate
        {
            private iOSAvCaptureService _parent;
            private string _path;
            public VideoDelegate(iOSAvCaptureService parent, string path)
            {
                _parent = parent;
                _path = path;
            }

            public override void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError? error)
            {
                if (error != null)
                {
                    _parent.Cancel();
                    return;
                }

                _parent.Complete(_path);
            }
        }
    }
}

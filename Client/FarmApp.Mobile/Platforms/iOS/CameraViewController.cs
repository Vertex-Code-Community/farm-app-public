using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using static System.Net.Mime.MediaTypeNames;

namespace FarmApp.Mobile.Platforms.iOS
{
    class CameraViewController : UIViewController
    {
        iOSAvCaptureService _service;

        AVCaptureVideoPreviewLayer _preview;

        enum CaptureMode { Photo, Video }
        CaptureMode _mode = CaptureMode.Photo;

        bool _recording;
        bool _torchEnabled;

        UIButton _shutter;
        UIButton _cancelButton;
        UIButton _switchBtn;
        UIButton _flashBtn;

        UIVisualEffectView _bottomBar;
        UISegmentedControl _modeControl;
        UILabel _timerLabel;
        UILabel _videoTimerLabel;

        UILabel _photoLabel;
        UILabel _videoLabel;

        NSTimer _timer;
        int _seconds;

        public CameraViewController(iOSAvCaptureService service)
        {
            _service = service;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Black;

            _service.SetupSession();

            _preview = new AVCaptureVideoPreviewLayer(_service.Session)
            {
                Frame = View.Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            View.Layer.AddSublayer(_preview);

            BuildTopBar();
            BuildBottomBar();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            _preview.Frame = View.Bounds;

            var safeBottom = View.SafeAreaInsets.Bottom;
            var panelHeight = 180;

            var panelY = View.Bounds.Height - panelHeight - safeBottom;

            _bottomBar.Frame = new CGRect(
                0,
                panelY,
                View.Bounds.Width,
                panelHeight);

            LayoutBottomBarSubviews();
        }
        void LayoutBottomBarSubviews()
        {
            var width = View.Bounds.Width;
            if (_mode == CaptureMode.Photo)
            {
                _photoLabel.Frame =
                    new CGRect(width / 2 - 25, 30, 60, 20);

                _videoLabel.Frame =
                    new CGRect(width / 2 - 90, 30, 70, 20);

            }
            else
            {
                _videoLabel.Frame =
                    new CGRect(width / 2 - 25, 30, 70, 20);

                _photoLabel.Frame =
                    new CGRect(width / 2 + 40, 30, 60, 20);

            }
            _cancelButton.Frame =
                new CGRect(24, 90, 100, 40);

            _shutter.Frame =
                new CGRect(width / 2 - 40, 70, 80, 80);

            var inner = _shutter.ViewWithTag(999);
            inner!.Frame = new CGRect(6, 6, 68, 68);

            _switchBtn.Frame =
                new CGRect(width - 80, 85, 56, 56);

            _switchBtn.SetPreferredSymbolConfiguration(UIImageSymbolConfiguration.Create(24), UIControlState.Normal);
        }

        void BuildTopBar()
        {
            _flashBtn = new UIButton(new CGRect(20, 56, 36, 36));
            _flashBtn.SetImage(UIImage.GetSystemImage("bolt.slash"), UIControlState.Normal);

            _flashBtn.TintColor = UIColor.White;

            _flashBtn.TouchUpInside += (_, _) =>
            {
                _torchEnabled = !_torchEnabled;
                _service.ToggleFlash();

                AnimateFlashIcon();

                _flashBtn.SetImage(UIImage.GetSystemImage(_torchEnabled ? "bolt.fill" : "bolt.slash"),
                    UIControlState.Normal);
            };

            View?.AddSubview(_flashBtn);

            _videoTimerLabel = new UILabel(new CGRect(View.Bounds.Width / 2 - 50, 55, 100, 22))
            {
                TextColor = UIColor.Red,
                Font = UIFont.MonospacedDigitSystemFontOfSize(13, UIFontWeight.Semibold),
                TextAlignment = UITextAlignment.Right,
                Hidden = true
            };

            View.AddSubview(_videoTimerLabel);
        }

        void BuildBottomBar()
        {
            var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);

            _bottomBar = new UIVisualEffectView(blur);
            View.AddSubview(_bottomBar);

            _videoLabel = CreateModeLabel("ВІДЕО");
            _photoLabel = CreateModeLabel("ФОТО");

            _bottomBar.ContentView.AddSubviews(
                _videoLabel,
                _photoLabel);

            UpdateModeUI();

            _cancelButton = new UIButton();
            _cancelButton.SetTitle("Скасувати", UIControlState.Normal);
            _cancelButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _cancelButton.TouchUpInside += (_, __) => _service.Cancel();
            _bottomBar.ContentView.AddSubview(_cancelButton);

            _shutter = new UIButton();
            _shutter.Layer.CornerRadius = 40;
            _shutter.Layer.BorderWidth = 4;
            _shutter.Layer.BorderColor = UIColor.White.CGColor;
            _shutter.BackgroundColor = UIColor.Clear;
            _shutter.TouchUpInside += OnShutter;

            var inner = new UIView();
            inner.Tag = 999;
            inner.BackgroundColor = UIColor.White;
            inner.Layer.CornerRadius = 30;

            _bottomBar.ContentView.AddSubview(_shutter);

            _switchBtn = new UIButton();
            _switchBtn.SetImage(
                UIImage.GetSystemImage("camera.rotate"),
                UIControlState.Normal);
            _switchBtn.TintColor = UIColor.White;
            _switchBtn.TouchUpInside += (_, __) =>
                _service.SwitchCamera();
            _bottomBar.ContentView.AddSubview(_switchBtn);
        }

        void AnimateFlashIcon()
        {
            UIView.Animate(0.15, () =>
            {
                _flashBtn.Transform =
                    CGAffineTransform.MakeScale(1.3f, 1.3f);
            },
            () =>
            {
                UIView.Animate(0.15, () =>
                {
                    _flashBtn.Transform =
                        CGAffineTransform.MakeIdentity();
                });
            });
        }

        void OnShutter(object sender, EventArgs e)
        {
            AnimateShutter();

            if (_mode == CaptureMode.Photo)
            {
                _service.TakePhoto();
                return;
            }

            if (_recording)
            {
                _service.StopVideo();
                StopTimer();
            }
            else
            {
                _service.StartVideo();
                StartTimer();
            }

            _recording = !_recording;

            var inner = _shutter.ViewWithTag(999);

            if (_mode == CaptureMode.Video)
            {
                inner!.BackgroundColor =
                    _recording ? UIColor.Red : UIColor.White;
            }
            else
            {
                inner!.BackgroundColor = UIColor.White;
            }
        }

        void StartTimer()
        {
            _seconds = 0;
            _timerLabel.Hidden = false;

            _timer = NSTimer.CreateRepeatingScheduledTimer(
                TimeSpan.FromSeconds(1), _ =>
                {
                    _seconds++;
                    _timerLabel.Text =
                    TimeSpan.FromSeconds(_seconds)
                    .ToString(@"mm\:ss");
                });
        }

        void StopTimer()
        {
            _timer?.Invalidate();
            _timer = null;
            _timerLabel.Hidden = true;
        }
        void AnimateShutter()
        {
            UIView.Animate(0.1, () =>
            {
                _shutter.Transform =
                    CGAffineTransform.MakeScale(0.9f, 0.9f);
            },
            () =>
            {
                UIView.Animate(0.1, () =>
                {
                    _shutter.Transform =
                        CGAffineTransform.MakeIdentity();
                });
            });
        }

        UILabel CreateModeLabel(string text)
        {
            var label = new UILabel();
            label.Text = text;
            label.Font = UIFont.SystemFontOfSize(13, UIFontWeight.Semibold);
            label.TextColor = UIColor.White;
            label.UserInteractionEnabled = true;

            var tap = new UITapGestureRecognizer(() =>
            {
                _mode = text == "ФОТО"
                    ? CaptureMode.Photo
                    : CaptureMode.Video;

                UpdateModeUI();
            });

            label.AddGestureRecognizer(tap);
            return label;
        }

        void UpdateModeUI()
        {
            var width = View.Bounds.Width;

            UIView.AnimateNotify(
                0.35,
                0,
                0.7f,
                0.8f,
                UIViewAnimationOptions.CurveEaseInOut,
                () =>
                {
                    if (_mode == CaptureMode.Photo)
                    {
                        _photoLabel.Center =
                            new CGPoint(width / 2, 30);

                        _videoLabel.Center =
                            new CGPoint(width / 2 - 70, 30);

                        _photoLabel.Transform =
                            CGAffineTransform.MakeScale(1.1f, 1.1f);

                        _videoLabel.Transform =
                            CGAffineTransform.MakeIdentity();
                    }
                    else
                    {
                        _videoLabel.Center =
                            new CGPoint(width / 2, 30);

                        _photoLabel.Center =
                            new CGPoint(width / 2 + 70, 30);

                        _videoLabel.Transform =
                            CGAffineTransform.MakeScale(1.1f, 1.1f);

                        _photoLabel.Transform =
                            CGAffineTransform.MakeIdentity();
                    }
                },
                null);

            _photoLabel.TextColor =
                _mode == CaptureMode.Photo
                    ? UIColor.Yellow
                    : UIColor.White;

            _videoLabel.TextColor =
                _mode == CaptureMode.Video
                    ? UIColor.Yellow
                    : UIColor.White;
        }
    }
}

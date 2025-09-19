using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScheduleChartView
{
    public class TimelineViewModel : INotifyPropertyChanged
    {
        // ----- 설정값 -----
        public int WindowSeconds { get; } = 60;               // 항상 최근 60초

        private int _updateIntervalSeconds = 2;               // n초(변경 가능)

        public int UpdateIntervalSeconds
        {
            get => _updateIntervalSeconds;
            set
            {
                if (value < 1) value = 1; _updateIntervalSeconds = value; OnPropertyChanged();
                RestartTimer();
            }
        }

        private double _pixelsPerSecond = 15;                 // 스케일 조절

        public double PixelsPerSecond
        {
            get => _pixelsPerSecond;
            set { _pixelsPerSecond = value; OnPropertyChanged(); RecalcCanvas(); }
        }

        public double RowHeight { get; set; } = 50;
        public double SegmentHeight => Math.Max(12, RowHeight - 12);

        public DateTime NowUtc
        {
            get => _nowUtc;
            private set { _nowUtc = value; OnPropertyChanged(); _visibleView.Refresh(); }
        }

        private DateTime _nowUtc = DateTime.UtcNow;
        public DateTime NowLocal => NowUtc.ToLocalTime();

        public ObservableCollection<SegmentVM> Segments { get; } = new();
        public ICollectionView VisibleSegments => _visibleView;
        private readonly ListCollectionView _visibleView;

        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; private set; }

        public int[] Rows { get; } = new[] { 0, 1, 2, 3, 4 };

        private readonly Random _rand = new();
        private DispatcherTimer _tickTimer;
        private DispatcherTimer _clockTimer; // 1Hz로 NowUtc 슬라이딩(부드럽게)

        private int _barCounter = 0;

        public TimelineViewModel()
        {
            // 보이는 항목: 최근 60초만
            _visibleView = new ListCollectionView(Segments);
            _visibleView.Filter = o =>
            {
                var s = (SegmentVM)o;
                return s.StartTime >= NowUtc.AddSeconds(-WindowSeconds) &&
                       s.StartTime <= NowUtc.AddSeconds(0);
            };

            RecalcCanvas();

            // 데이터 주입 타이머(n초)
            _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(UpdateIntervalSeconds) };
            _tickTimer.Tick += (s, e) => InjectBatch();
            _tickTimer.Start();

            // 시계 타이머(1초) – 화면 창을 앞으로 밀기
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => NowUtc = DateTime.UtcNow;
            _clockTimer.Start();
        }

        private void RestartTimer()
        {
            if (_tickTimer != null)
            {
                _tickTimer.Stop();
                _tickTimer.Interval = TimeSpan.FromSeconds(UpdateIntervalSeconds);
                _tickTimer.Start();
            }
        }

        private void RecalcCanvas()
        {
            CanvasWidth = WindowSeconds * PixelsPerSecond;
            CanvasHeight = Rows.Length * RowHeight;
            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
        }

        // n초마다 들어온 데이터 샘플을 “해당 구간의 시간대들”에 랜덤하게 넣어줌
        private void InjectBatch()
        {
            var now = DateTime.UtcNow;
            var batchCount = _rand.Next(2, 6); // 이번 배치에 2~5개
            var colors = new[] { Colors.IndianRed, Colors.Gold, Colors.MediumTurquoise, Colors.RoyalBlue, Colors.MediumOrchid };

            for (int i = 0; i < batchCount; i++)
            {
                int row = Rows[_rand.Next(Rows.Length)];
                // 이번 n초 구간 안의 임의 지점
                double offsetSec = _rand.NextDouble() * UpdateIntervalSeconds;
                var start = now.AddSeconds(-offsetSec);
                var dur = TimeSpan.FromMilliseconds(_rand.Next(300, 1500)); // 0.3~1.5s

                Segments.Add(new SegmentVM
                {
                    BarIndex = ++_barCounter,
                    Row = row,
                    StartTime = start,
                    Duration = dur,
                    Brush = new SolidColorBrush(colors[row])
                });
            }

            // 오래된 것 정리 (성능)
            var limit = now.AddSeconds(-WindowSeconds - 5); // 5초 버퍼
            for (int i = Segments.Count - 1; i >= 0; i--)
                if (Segments[i].StartTime < limit)
                    Segments.RemoveAt(i);

            // Now를 최신으로 맞추고 뷰 갱신
            NowUtc = now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class SegmentVM
    {
        public int BarIndex { get; set; }
        public int Row { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Brush Brush { get; set; }

        public string Tooltip => $"{BarIndex}번 Bar, {Duration.TotalMilliseconds:0} ms 동안 진행";
    }
}
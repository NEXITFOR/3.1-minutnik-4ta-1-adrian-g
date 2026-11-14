using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly List<string> _history = new();
    private const int DefaultWorkMinutes = 25;
    private const int DefaultBreakMinutes = 5;

    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;
    private int _completedCycles;
    private bool _isBreak;
    private bool _isRunning;
    private bool _followDuration = true;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;

        WorkMinutesBox.ValueChanged += OnWorkMinutesChanged;

        _remainingSeconds = ReadMinutes(WorkMinutesBox, DefaultWorkMinutes) * 60;
        UpdateDisplay();
        Closed += OnClosed;
    }

    private void OnStartPauseClick(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _timer.Stop();
            _isRunning = false;
            UpdateDisplay();
            return;
        }

        if (_followDuration)
        {
            _remainingSeconds = CurrentPhaseSeconds();
        }

        _followDuration = false;
        _isRunning = true;
        _timer.Start();
        UpdateDisplay();
    }

    private void OnResetClick(object? sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _isRunning = false;
        _isBreak = false;
        _followDuration = true;
        _completedCycles = 0;
        _remainingSeconds = ReadMinutes(WorkMinutesBox, DefaultWorkMinutes) * 60;
        UpdateDisplay();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 1)
        {
            _remainingSeconds--;
            UpdateDisplay();
            return;
        }

        if (!_isBreak)
        {
            _isBreak = true;
            _followDuration = false;
            _remainingSeconds = ReadMinutes(BreakMinutesBox, DefaultBreakMinutes) * 60;
            UpdateDisplay();
            return;
        }

        _timer.Stop();
        _isRunning = false;
        _isBreak = false;
        _followDuration = true;
        _completedCycles++;
        _remainingSeconds = ReadMinutes(WorkMinutesBox, DefaultWorkMinutes) * 60;
        UpdateDisplay();
    }

    private void OnWorkMinutesChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_followDuration && !_isBreak && !_isRunning)
        {
            _remainingSeconds = ReadMinutes(WorkMinutesBox, DefaultWorkMinutes) * 60;
            UpdateDisplay();
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _timer.Stop();
    }

    private int CurrentPhaseSeconds()
    {
        return (_isBreak
            ? ReadMinutes(BreakMinutesBox, DefaultBreakMinutes)
            : ReadMinutes(WorkMinutesBox, DefaultWorkMinutes)) * 60;
    }

    private static int ReadMinutes(NumericUpDown box, int fallback)
    {
        if (box.Value is not decimal value)
        {
            return fallback;
        }

        int minutes = (int)value;
        if (minutes < 1)
        {
            return 1;
        }

        if (minutes > 180)
        {
            return 180;
        }

        return minutes;
    }

    private void UpdateDisplay()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        TimeText.Text = $"{minutes:00}:{seconds:00}";
        CyclesText.Text = _completedCycles.ToString();
        PhaseText.Text = _isBreak ? "Przerwa" : "Praca";
        StartPauseButton.Content = _isRunning ? "Pauza" : "Start";
    }
}

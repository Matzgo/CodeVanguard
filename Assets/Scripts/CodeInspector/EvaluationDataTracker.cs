using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class EvaluationDataTracker : MonoBehaviour
{
    public static EvaluationDataTracker Instance { get; private set; }

    private float _sessionStartTime;
    private Dictionary<string, float> _activeTimers = new Dictionary<string, float>();
    private Dictionary<string, float> _trackedTimes = new Dictionary<string, float>(); // Stores cumulative times

    private Dictionary<string, int> _puzzleTestAttempts = new Dictionary<string, int>(); // Tracks test attempts
    private Dictionary<string, int> _puzzleAttemptsBeforeSuccess = new Dictionary<string, int>(); // Tracks failed attempts before success
    private Dictionary<string, int> _puzzleCompletedAfterSuccess = new Dictionary<string, int>(); // Tracks completions after first success
    private Dictionary<string, GradingResult> _gradingResults = new Dictionary<string, GradingResult>(); // Stores grading results
    private HashSet<string> _puzzlesCompletedAtLeastOnce = new HashSet<string>(); // Tracks solved puzzles

    private List<string> _globalTimestamps = new List<string>(); // Stores all timestamps globally


    private Dictionary<string, int> _eventCount = new Dictionary<string, int>(); // Stores grading results

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _sessionStartTime = Time.unscaledTime; // Record session start time
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    // ====== TIMER TRACKING METHODS ====== //

    public void TrackTime(string key)
    {
        if (!_activeTimers.ContainsKey(key))
        {
            _activeTimers[key] = Time.unscaledTime; // Start tracking time
            AddTimestamp($"Started tracking {key}");
        }
    }

    public void TrackEvent(string key)
    {
        if (!_eventCount.ContainsKey(key))
            _eventCount.Add(key, 1);

        _eventCount[key]++;
    }

    public void StopTrackTime(string key)
    {
        if (_activeTimers.TryGetValue(key, out float startTime))
        {
            float elapsedTime = Time.unscaledTime - startTime;
            _activeTimers.Remove(key); // Stop tracking this puzzle


            // Track total elapsed time for everything
            if (_trackedTimes.ContainsKey(key))
                _trackedTimes[key] += elapsedTime;
            else
                _trackedTimes[key] = elapsedTime;

            AddTimestamp($"Stopped tracking {key}");
        }
    }

    private void StopAllActiveTimers()
    {
        List<string> activeKeys = new List<string>(_activeTimers.Keys);

        foreach (string key in activeKeys)
        {
            StopTrackTime(key); // Stop and store all active timers
        }
    }

    // ====== PUZZLE ATTEMPTS TRACKING ====== //

    public void RecordPuzzleTestAttempt(string key)
    {
        if (_puzzleTestAttempts.ContainsKey(key))
        {
            _puzzleTestAttempts[key]++;
        }
        else
        {
            _puzzleTestAttempts[key] = 1; // First recorded test attempt
        }

        AddTimestamp($"Tested {key}");
    }

    public void RecordPuzzleAttempt(string key)
    {
        if (!_puzzlesCompletedAtLeastOnce.Contains(key))
        {
            // If the puzzle has never been completed successfully, track failed attempts
            if (_puzzleAttemptsBeforeSuccess.ContainsKey(key))
            {
                _puzzleAttemptsBeforeSuccess[key]++;
            }
            else
            {
                _puzzleAttemptsBeforeSuccess[key] = 1; // First recorded attempt
            }
        }
        else
        {
            // If puzzle was already completed successfully, track further completions
            if (_puzzleCompletedAfterSuccess.ContainsKey(key))
            {
                _puzzleCompletedAfterSuccess[key]++;
            }
            else
            {
                _puzzleCompletedAfterSuccess[key] = 1; // First extra completion
            }
        }

        AddTimestamp($"Executed {key}");
    }

    public void MarkPuzzleCorrect(string key, GradingResult gradingResult)
    {
        if (!_puzzlesCompletedAtLeastOnce.Contains(key))
        {
            _puzzlesCompletedAtLeastOnce.Add(key); // First time solving it

        }

        // Store grading results
        _gradingResults[key] = gradingResult;
        AddTimestamp($"Solved {key}\n{gradingResult}");
    }

    // ====== TIMESTAMP TRACKING ====== //

    private void AddTimestamp(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss"); // Format: HH:MM:SS
        _globalTimestamps.Add($"{timestamp} - {message}");
    }

    // ====== SAVE DATA ON QUIT ====== //

    public void SaveData()
    {
        StopAllActiveTimers(); // Ensure all times are saved before writing to the file
        SaveTrackedTimesToFile();
        SaveTrackedTimesToCSV(); // Also save as CSV
    }
    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void SaveTrackedTimesToFile()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string folderPath = Path.Combine(desktopPath, "CodeVanguard"); // Folder on Desktop

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate a concise timestamp for the filename: YYYY-MM-DD_HH-mm
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string filePath = Path.Combine(folderPath, $"CodeVanguard_{CodeVanguardManager.Instance.UniqueId}_{timestamp}.txt");

        using (StreamWriter writer = new StreamWriter(filePath, true)) // Append to file
        {
            writer.WriteLine($"--- {CodeVanguardManager.Instance.UniqueId} ---");
            writer.WriteLine($"--- Session {DateTime.Now:yyyy-MM-dd HH:mm} ---");
            writer.WriteLine($"\n");
            writer.WriteLine($"STAR SYSTEM: {CodeVanguardManager.Instance.UseStarSystem}");
            writer.WriteLine($"\n");
            float totalPlaytime = Time.unscaledTime - _sessionStartTime;
            writer.WriteLine($"Total session playtime: {totalPlaytime} seconds ({FormatPlaytime(totalPlaytime)})");

            // ====== PUZZLE EXECUTION TIMES ======
            writer.WriteLine("\nPuzzle Times:");
            foreach (var entry in _trackedTimes)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value} seconds ({FormatPlaytime(entry.Value)})");
            }

            // ====== PUZZLE TEST ATTEMPTS ======
            writer.WriteLine("\nPuzzle Test Attempts:");
            foreach (var entry in _puzzleTestAttempts)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value} test attempts before execution");
            }

            // ====== ATTEMPTS BEFORE SUCCESS ======
            writer.WriteLine("\nPuzzle Attempts Before Success:");
            foreach (var entry in _puzzleAttemptsBeforeSuccess)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value} failed attempts before success");
            }

            // ====== COMPLETIONS AFTER SUCCESS ======
            writer.WriteLine("\nPuzzle Completions Successfull:");
            foreach (var entry in _puzzleCompletedAfterSuccess)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value} successful completions after first success");
            }

            // ====== EVENT TRACKING ======
            writer.WriteLine("\nEvent Counts:");
            foreach (var entry in _eventCount)
            {
                writer.WriteLine($"{entry.Key}: {entry.Value} occurrences");
            }


            // ====== GRADING RESULTS ======
            writer.WriteLine("\nGrading Results:");
            foreach (var entry in _gradingResults)
            {
                writer.WriteLine($"{entry.Key}:\n{entry.Value}");
            }

            // ====== GLOBAL TIMESTAMPS (SORTED CHRONOLOGICALLY) ======
            writer.WriteLine("\nAll Events (Sorted Chronologically):");
            _globalTimestamps.Sort(); // Ensure chronological order
            foreach (var entry in _globalTimestamps)
            {
                writer.WriteLine(entry);
            }

            writer.WriteLine();
        }

        Debug.Log($"Tracked times and grading results saved to: {filePath}");

    }


    private string FormatPlaytime(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    private void SaveTrackedTimesToCSV()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string folderPath = Path.Combine(desktopPath, "CodeVanguard");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate timestamp for filename
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string filePath = Path.Combine(folderPath, $"CodeVanguard_Data_{CodeVanguardManager.Instance.UniqueId}_{timestamp}.csv");

        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            // Write session metadata
            writer.WriteLine("DATA_TYPE,KEY,VALUE,FORMATTED_VALUE,ADDITIONAL_INFO");

            // Session info
            float totalPlaytime = Time.unscaledTime - _sessionStartTime;
            writer.WriteLine($"SESSION_INFO,Session_Date,{DateTime.Now:yyyy-MM-dd HH:mm},,");
            writer.WriteLine($"SESSION_INFO,Star_System_Enabled,{CodeVanguardManager.Instance.UseStarSystem},,");
            writer.WriteLine($"SESSION_INFO,Total_Playtime,{totalPlaytime},{FormatPlaytime(totalPlaytime)},seconds");

            // Puzzle times
            foreach (var entry in _trackedTimes)
            {
                writer.WriteLine($"PUZZLE_TIME,{EscapeCSV(entry.Key)},{entry.Value},{FormatPlaytime(entry.Value)},seconds");
            }

            // Test attempts
            foreach (var entry in _puzzleTestAttempts)
            {
                writer.WriteLine($"TEST_ATTEMPTS,{EscapeCSV(entry.Key)},{entry.Value},,count");
            }

            // Attempts before success
            foreach (var entry in _puzzleAttemptsBeforeSuccess)
            {
                writer.WriteLine($"ATTEMPTS_BEFORE_SUCCESS,{EscapeCSV(entry.Key)},{entry.Value},,count");
            }

            // Completions after success
            foreach (var entry in _puzzleCompletedAfterSuccess)
            {
                writer.WriteLine($"COMPLETIONS_AFTER_SUCCESS,{EscapeCSV(entry.Key)},{entry.Value},,count");
            }

            // Event counts
            foreach (var entry in _eventCount)
            {
                writer.WriteLine($"EVENT_COUNT,{EscapeCSV(entry.Key)},{entry.Value},,occurrences");
            }

            // Grading results with specific fields based on the GradingResult class
            writer.WriteLine("\nGRADING_RESULTS");
            writer.WriteLine("PUZZLE_KEY,CORRECT,PERFORMANCE_SCORE,MEMORY_SCORE,NAMING_SCORE,COMMENTS");
            foreach (var entry in _gradingResults)
            {
                string puzzleKey = EscapeCSV(entry.Key);
                GradingResult result = entry.Value;
                writer.WriteLine($"{puzzleKey},{result.Correct},{result.PerformanceScore:F2},{result.MemoryScore:F2},{result.NamingScore:F2}");
            }

            // Timestamps in chronological order
            writer.WriteLine("\nTIMESTAMPS");
            writer.WriteLine("TIMESTAMP,EVENT");
            List<string> sortedTimestamps = new List<string>(_globalTimestamps);
            sortedTimestamps.Sort();
            foreach (var entry in sortedTimestamps)
            {
                // Split the timestamp entry into parts
                int separatorIndex = entry.IndexOf(" - ");
                if (separatorIndex > 0)
                {
                    string time = entry.Substring(0, separatorIndex);
                    string message = entry.Substring(separatorIndex + 3);
                    writer.WriteLine($"{time},{EscapeCSV(message)}");
                }
                else
                {
                    writer.WriteLine($",{EscapeCSV(entry)}");
                }
            }
        }
        Debug.Log($"Data exported to CSV: {filePath}");
    }

    // Helper function to properly escape CSV fields
    private string EscapeCSV(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        // If the field contains commas, quotes, or newlines, wrap it in quotes and escape any existing quotes
        bool needsEscaping = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
        if (needsEscaping)
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

}
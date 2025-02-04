using System.Collections.Generic;

public class PowerScenarioSimulator
{
    private int cntr;
    private bool _wasBeamRedirected;
    private bool _valid;
    private int _redirectCounter;
    private bool _isFirstBeam;
    private int _nonRedirectCounter;

    public PowerScenarioSimulator()
    {
        Reset();
    }

    public void Reset()
    {
        cntr = 0;
        _wasBeamRedirected = false;
        _valid = true;
        _redirectCounter = 0;
        _isFirstBeam = true;
        _nonRedirectCounter = 0;
    }

    public void FireBeam()
    {
        // Invalid if two consecutive beam redirections 
        if (_redirectCounter > 1)
        {
            _valid = false;
        }

        // Invalid if more than two consecutive non-redirected beams
        if (_nonRedirectCounter > 1)
        {
            _valid = false;
        }

        if (!_isFirstBeam)
        {
            // Reset counters based on previous beam state
            if (_wasBeamRedirected)
            {
                _nonRedirectCounter = 0;
                _redirectCounter++;
            }
            else
            {
                _nonRedirectCounter++;
                _redirectCounter = 0;
            }

        }
        _isFirstBeam = false;

        // Reset redirect state for the new beam
        _wasBeamRedirected = false;
        cntr++;
    }

    public void RedirectBeam()
    {
        // Mark the current beam as redirected
        _wasBeamRedirected = true;
    }

    public void GeneratorStart()
    {
        // Invalid if two consecutive beam redirections 
        if (_redirectCounter > 1)
        {
            _valid = false;
        }

        // Invalid if more than two consecutive non-redirected beams
        if (_nonRedirectCounter > 1)
        {
            _valid = false;
        }

        // Ensure pattern was valid before allowing generator to start
        if (!_valid)
        {
            // Generator stall logic
        }
    }

    public (bool b, List<string> feedback, List<string> feedbackKey) CheckCorrectness()
    {
        var feedback = new List<string>();
        var feedbackKey = new List<string>();
        //Debug.Log($"{_redirectCounter}|{_nonRedirectCounter}|{_valid.ToString()}");

        if (!_valid)
        {
            feedbackKey.Add("POW_Invalid");
            if (_redirectCounter > 1)
            {
                feedback.Add("More than one consecutive redirected beam");
            }

            if (_nonRedirectCounter > 1)
            {
                feedback.Add("More than two consecutive non-redirected beams");

            }
        }
        else
        {
            feedbackKey.Add("POW_Valid");
        }

        return (_valid, feedback, feedbackKey);
    }
}
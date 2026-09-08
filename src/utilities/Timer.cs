using System;

namespace odl3d;

/// <summary>
/// A simple timer utility class that tracks duration, finish time, and remaining time.
/// </summary>
public class Timer
{
    /// <summary>
    /// The duration of the timer in seconds.
    /// </summary>
    public double Duration { get; private set; }

    /// <summary>
    /// The finish time of the timer in seconds.
    /// </summary>
    public double FinishTime { get; private set; }

    /// <summary>
    /// The remaining time of the timer in seconds. Returns 0 if the timer has finished.
    /// </summary>
    public double RemainingTime => Math.Max(0d, FinishTime - GLFW.glfwGetTime());

    /// <summary>
    /// Indicates whether the timer has finished.
    /// </summary>
    public bool IsFinished => GLFW.glfwGetTime() >= FinishTime;

    /// <summary>
    /// Called when the timer finishes as long as the object is updated.
    /// </summary>
    public Action? OnFinished;

    /// <summary>
    /// Indicates whether the OnFinished action has been called.
    /// </summary>
    public bool CalledOnFinished { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Timer class with the specified duration.
    /// </summary>
    /// <param name="duration">The duration of the timer in seconds.</param>
    public Timer(double duration)
    {
        Duration = duration;
        Reset(duration);
    }

    /// <summary>
    /// Resets the timer with the specified duration.
    /// </summary>
    /// <param name="duration">The duration of the timer in seconds.</param>
    public void Reset(double duration)
    {
        Duration = duration;
        Reset();
    }

    /// <summary>
    /// Resets the timer with the current duration.
    /// </summary>
    public void Reset()
    {
        FinishTime = GLFW.glfwGetTime() + Duration;
        CalledOnFinished = false;
    }

    /// <summary>
    /// Updates the timer and invokes the OnFinished action if the timer has finished and the action has not been called yet.
    /// </summary>
    public void Update()
    {
        if (IsFinished && !CalledOnFinished)
        {
            OnFinished?.Invoke();
            CalledOnFinished = true;
        }
    }
}
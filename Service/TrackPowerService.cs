using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z21;
using Z21.Enums;
using Z21.Events;

namespace Service
{
  public class TrackPowerService
  {
    private bool lastTrackPowerUpdateWasShort = false;

    private TrackPower trackPower;

    public TrackPowerService(Client z21Client)
    {
      Z21Client = z21Client;
      Z21Client.TrackPowerChanged += Controller_TrackPowerChanged;
      Z21Client.OnStatusChanged += Controller_OnStatusChanged;
    }

    /// <summary>
    /// Occurs when a property changes.
    /// </summary>
    public event EventHandler? StateChanged;

    public TrackPower TrackPower
    {
      get => trackPower;
      private set
      {
        if (!lastTrackPowerUpdateWasShort)
        {
          trackPower = value;
        }

        lastTrackPowerUpdateWasShort = value == TrackPower.Short;
        OnStateChanged();
      }
    }

    public bool TrackPowerOn => TrackPower == TrackPower.ON;

    private Client Z21Client { get; }

    /// <summary>
    ///  Sets the track power.
    /// </summary>
    /// <param name="state"></param>
    public void SetTrackPower(TrackPower state)
    {
      SetTrackPower(state == TrackPower.ON);
    }

    /// <summary>
    /// Sets the track power. If <paramref name="state"/> is true then the track power is on. If <paramref name="state"/> is false then the power will be turned off.
    /// </summary>
    /// <param name="state"></param>
    public void SetTrackPower(bool state)
    {
      if (state)
      {
        Z21Client.SetTrackPowerON();
      }
      else
      {
        Z21Client.SetTrackPowerOFF();
      }
    }

    private void Controller_OnStatusChanged(object? sender, StateEventArgs e)
    {
      TrackPower = e.TrackPower;
    }

    private void Controller_TrackPowerChanged(object? sender, TrackPowerEventArgs e)
    {
      TrackPower = e.TrackPower;
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event. 
    /// </summary>
    private void OnStateChanged()
    {
      StateChanged?.Invoke(this, null!);
    }
  }
}
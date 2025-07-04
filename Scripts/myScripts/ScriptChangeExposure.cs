/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using UVtools.Core.Scripting;

namespace UVtools.ScriptSample;

/// <remarks>
/// This script changes the exposure time of all selected layers.
/// </remarks>
public class ScriptChangeExposure : ScriptGlobals
{
    /// <remarks>
    /// Configure script metadata for changing exposure time.
    /// </remarks>
    public void ScriptInit()
    {
        Script.Name = "Change layer exposure";
        Script.Description = "Set the exposure time of all selected layers to a fixed value.";
        Script.Author = "Tomas Civini";
        Script.Version = new Version(0, 1);
    }

    /// <remarks>
    /// No user input validation required for changing exposure.
    /// </remarks>
    /// <returns>Null if validation passes.</returns>
    public string? ScriptValidate()
    {
        return null;
    }

    /// <remarks>
    /// Sets the exposure time of each selected layer to 40 seconds.
    /// </remarks>
    /// <returns>True if the operation completes without cancellation.</returns>
    public bool ScriptExecute()
    {
        Progress.Reset("Changing exposure", Operation.LayerRangeCount); // Sets the progress name and number of items to process

        for (uint layerIndex = Operation.LayerIndexStart; layerIndex <= Operation.LayerIndexEnd; layerIndex++)
        {
            Progress.PauseOrCancelIfRequested(); // Abort operation if user requested cancellation
            var layer = SlicerFile[layerIndex]; // Get the current layer

            layer.ExposureTime = 40; // Set exposure time to 40 seconds

            Progress++; // Increment progress bar by 1
        }

        // Return true if not cancelled by user
        return !Progress.Token.IsCancellationRequested;
    }
}
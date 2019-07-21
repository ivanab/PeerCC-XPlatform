﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// This class is borrowed from here: https://github.com/webrtc-uwp/PeerCC/blob/0f53fc5045d8b5347c79b4613510663f2053f6e5/ClientCore/Utilities/SdpUtils.cs

namespace GuiCore
{
    /// <summary>
    /// Utility class to organize SDP negotiation.
    /// </summary>
    class SdpUtils
    {
        /// <summary>
        /// Forces the SDP to use the selected audio or video codec.
        /// </summary>
        /// <param name="sdp">Session description.</param>
        /// <param name="codecName">Name of the codec.</param>
        /// <param name="codecType">Type of the codec (audio or video).</param>
        /// <returns>True if succeeds to force to use the selected audio/video codecs.</returns>
        public static bool SelectCodec(ref string sdp, string codecName, string codecType)
        {
            var sdpLines = sdp.Split("\r\n");
            var mLineIndex = -1;

            for (var i = 0; i < sdpLines.Length; i++)
            {
                if (sdpLines[i].IndexOf("m=") == 0 && sdpLines[i].ToLower().IndexOf(codecType) != -1)
                {
                    mLineIndex = i;
                    break;
                }
            }

            if (mLineIndex == -1)
                return false;

            string payload = String.Empty;

            for (var i = sdpLines.Length - 1; i >= 0; i--)
            {
                var index = -1;
                for (var j = i; j >= 0; j--)
                {
                    if (sdpLines[j].IndexOf("a=rtpmap") == 0 && sdpLines[j].ToLower().IndexOf(codecName.ToLower()) != -1)
                    {
                        index = j;
                        break;
                    }
                }
                if (index != -1)
                {
                    i = index;
                    var pattern = new Regex("a=rtpmap:(\\d+) [a-zA-Z0-9-]+\\/\\d+");
                    var result = pattern.Match(sdpLines[index]).Groups;
                    if (result.Count == 2)
                        payload = result[1].Value;
                    if (payload.Length != 0)
                    {
                        var elements = new List<string>(sdpLines[mLineIndex].Split(" "));

                        var newLine = elements.GetRange(0, 3);
                        newLine.Add(payload);
                        for (var j = 3; j < elements.Count; j++)
                        {
                            if (elements[j] != payload)
                            {
                                newLine.Add(elements[j]);
                            }
                        }
                        sdpLines[mLineIndex] = string.Join(" ", newLine);
                    }
                }
                else
                {
                    break;
                }
            }

            sdp = string.Join("\r\n", sdpLines);

            return true;
        }
    }
}

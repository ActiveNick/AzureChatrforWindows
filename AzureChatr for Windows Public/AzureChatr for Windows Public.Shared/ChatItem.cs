//==========================================================================
//
// Author:  Nick Landry
// Title:   Senior Technical Evangelist - Microsoft US DX - NY Metro
// Twitter: @ActiveNick
// Blog:    www.AgeofMobility.com
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// Disclaimer: Portions of this code may been simplified to demonstrate
// useful application development techniques and enhance readability.
// As such they may not necessarily reflect best practices in enterprise 
// development, and/or may not include all required safeguards.
// 
// This code and information are provided "as is" without warranty of any
// kind, either expressed or implied, including but not limited to the
// implied warranties of merchantability and/or fitness for a particular
// purpose.
//==========================================================================
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AzureChatr
{
    /// <summary>
    /// Main class used for individual chat lines. Gets serialized in JSON
    /// to be transmitted to the cloud and saved in a table via 
    /// Azure Mobile Services.
    /// </summary>
    class ChatItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime TimeStamp { get; set; }
        
        // Read-only property not persisted in the cloud
        [JsonIgnore]
        public string ChatLine
        {
            get { return this.UserName + " - " + this.Text; }
        }
    }
}

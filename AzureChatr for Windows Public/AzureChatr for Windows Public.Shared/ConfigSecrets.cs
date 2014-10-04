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

namespace AzureChatr
{
    /// <summary>
    /// This static class is used to store all the application keys and secret codes and
    /// connection string details to connect with Azure.
    /// 
    /// You must create your own Azure account and configure the following Azure services
    /// to implement your own working version of AzureChatr:
    /// 
    ///     - Azure Mobile Services
    ///     - Azure Notification Hubs
    ///     
    /// For more information on configuring the cloud services for AzureChatr, please
    /// visit my blog at www.AgeofMobility.com
    /// </summary>
    static class ConfigSecrets
    {
        // TO DO: Once you have configured the required Azure services, you can set the 
        //        following bool constant to true
        public const bool ISAZURECONFIGDONE = false;

        // Azure Mobile Services secrets
        public const string AzureMobileServicesURI = "https://yourmobileservice.azure-mobile.net/";
        public const string AzureMobileServicesAppKey = "XXXX-INSERTYOURAZUREMOBILESERVICEAPPKEYHERE-XXXX";

        // Azure Notification Hub secrets
        public const string AzureNotificationHubName = "insertyourpushnotificationhubnamehere";
        public const string AzureNotificationHubCnxString = "XXXX-InsertYourAzureNotificationHubSharedAccessConnectionStringHere-XXXX";
    
        // Developer details for support and such
        public const string DeveloperSupportEmail = "support@yourdomain.com";
    }
}

===============================================================================

Author:  Nick Landry
Title:   Senior Technical Evangelist - Microsoft US DX - NY Metro
Twitter: @ActiveNick
Blog:    www.AgeofMobility.com

Copyright (c) Microsoft Corporation. All rights reserved.

Disclaimer: Portions of this code may been simplified to demonstrate
useful application development techniques and enhance readability.
As such they may not necessarily reflect best practices in enterprise 
development, and/or may not include all required safeguards.
 
This code and information are provided "as is" without warranty of any
kind, either expressed or implied, including but not limited to the
implied warranties of merchantability and/or fitness for a particular
purpose.
===============================================================================

WELCOME TO AZURECHATR!
----------------------

AzureChatr is a cross-platform chat client used by Microsoft Senior Technical
Evangelist Nick Landry to demonstrate mobile development techniques with a 
cloud backend using Microsoft Azure. While AzureChatr can be used to chat about
anything, the intent of the app is to bring users together to talk about cloud
development.

This is the initial beta release of AzureChatr for Windows Phone 8.1.
AzureChatr will soon be available on Windows 8.1, iOS and Android.

Visit www.AgeofMobility.com for more info on the current and upcoming features.


BEFORE YOU RUN THE APP FOR THE FIRST TIME
-----------------------------------------
If you try to run the app as soon as you open it in Visual Studio, you will
notice a popup dialog warning you about missing Cloud services.
You must create your own Azure account and configure the following Azure 
services to implement your own working version of AzureChatr:

- Azure Mobile Services
- Azure Notification Hubs

For more information on configuring the cloud services for AzureChatr, please
visit my blog at www.AgeofMobility.com. All configured app keys and secrets
should be stored in the ConfigSecrets.cs class file under the "\Common" folder
in the Shared Project.
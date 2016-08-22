# AzureChatr for Windows
Universal Windows Client for a Cloud-based Cross-Platform Chat App

## WELCOME TO AZURECHATR!
AzureChatr is a cross-platform chat client used by Microsoft Senior Technical
Evangelist Nick Landry to demonstrate mobile development techniques with a 
cloud backend using Microsoft Azure. While AzureChatr can be used to chat about
anything, the intent of the app is to bring users together to talk about cloud
development.

This is the initial beta release of [AzureChatr for Windows Phone 8.1](http://www.microsoft.com/en-US/store/Apps/AzureChatr/9WZDNCRDK76G).
AzureChatr will eventually be available on Windows 10/UWP, iOS and Android.

Visit www.AgeofMobility.com for more info on the current and upcoming features.

## BEFORE YOU RUN THE APP FOR THE FIRST TIME
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

## EDIT THE INSERT SCRIPT OF YOUR MOBILE SERVICE TABLE
- Create a Mobile Service in Azure with the name of your choice using a JavaScript backend.
- Create a new table in that Mobile Service called "ChatItem" (no quotes).
- Edit the ChatItem table Insert script and replace it with the following code:


        function insert(item, user, request) {
            var azure = require('azure');
            var hub = azure.createNotificationHubService('InsertYourNotificationHubNameHere', 
            'InsertYourNotificationHubFullAccessConnectionStringHere');
        
            // Execute the request and send notifications.
            request.execute({
                success: function() {
                    // Write the default response
                    request.respond();
        
                    // Send a notification to all users on all platforms. 
                    //hub.send(null, payload,  
                    //    function(error, outcome){
                    //        // Do something here with the outcome or error.
                    //    });
        
                    // Send a WNS notification.
                    hub.wns.sendToastText04(null, {
                        text1: item.username,
                        text2: item.text,
                        text3: ''
                    }, function(error, outcome){
                            // Do something here with the outcome or error.
                       });
                    
                    // Send a GCM notification.
                    var gcmpayload = '{"data":{"message":"' + item.text + '","username" : "' + item.username + '"}}';
                    hub.gcm.send(null, gcmpayload, 
                       function(error, outcome){
                            // Do something here with the outcome or error.
                       });
                    
                    // Send an APNS notification
                    var apnpayload = '{"aps":{"alert":"' + item.text + '"},"username" : "' + item.username + '"}';
                    hub.apns.send( null, apnpayload,
                        function (error) {
                            if (!error) {
                                // message sent successfully
                            }
                        });
                }
            });
        }
        
Make sure to insert your notification hub name and full access connection string in the placeholders above when calling createNotificationHubService. You only need to do this ONCE in Azure since the same INSERT script is used as the common backend for Windows, iOS and Android.

## Follow Me
* Twitter: [@ActiveNick](http://twitter.com/ActiveNick)
* Blog: [AgeofMobility.com](http://AgeofMobility.com)
* SlideShare: [http://www.slideshare.net/ActiveNick](http://www.slideshare.net/ActiveNick)

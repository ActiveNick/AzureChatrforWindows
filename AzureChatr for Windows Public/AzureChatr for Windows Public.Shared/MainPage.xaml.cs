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
using AzureChatr.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Live;
using Microsoft.WindowsAzure.Messaging;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.Media.SpeechSynthesis;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.NetworkInformation;

namespace AzureChatr
{
    public sealed partial class MainPage : Page
    {
        private MobileServiceUser user;
        private LiveConnectSession session;
        private string userfirstname;
        private string userlastname;
        private bool isLoggedin = false;
        private bool isSpeechEnabled = false;     // Speech synthesis is disabled by default

        public PushNotificationChannel PushChannel;

        private MobileServiceCollection<ChatItem, ChatItem> items;
        private IMobileServiceTable<ChatItem> chatTable = App.MobileService.GetTable<ChatItem>();

        // Media element used for the optional speech synthesis
        MediaElement mediaplayer;
        string lastChatline = "";

        public MainPage()
        {
            this.InitializeComponent();

            if (!CheckForInternetAccess())
            {
                string msg1 = "An Internet connection is required for this app and it appears that you are not connected." + Environment.NewLine + Environment.NewLine;
                string msg2 = "Make sure that you have an active Internet connection and try again.";
                UpdateStatus("You are not connected to the Internet", true);

                new MessageDialog(msg1 + msg2, "No Internet").ShowAsync();
            }
            else
            {
                InitNotificationsAsync();
            }
            mediaplayer = new MediaElement();

// On Windows, the Send button must be made visible since the Command Bar is not always
// visible and we don't want to force the user to swipe up every time they want to chat.
#if WINDOWS_APP
            btnWinSend.Visibility = Windows.UI.Xaml.Visibility.Visible;
            TextInput.Height = btnWinSend.Height;
#endif
        }

        // Check to see if we have Internet access since we need it to talk to Azure
        private bool CheckForInternetAccess()
        {
            // This is not the most foolproof way but at least covers basic scenarios
            // like Airplane mode
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // If we have trouble connecting to cloud services, we need to disable some UI
        // features so the user won't try to chat anyways.
        private async Task SetUIState(bool isEnabled)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TextInput.PlaceholderText = (isEnabled) ? "chat with others by typing here" : "you must be logged in to chat";
                TextInput.IsEnabled = isEnabled;
                btnWinSend.IsEnabled = isEnabled;
                ListItems.Focus(FocusState.Programmatic);
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            MessageDialog dlg;
            string msg1 = "", msg2 = "";
            Exception exception = null;

            try
            {
                // This block of code is to remind you to create your Azure services before you can
                // run AzureChatr. See details and links in the ConfigSecrets.cs class file or in
                // the README file for more information on how to proceed.
                // You can delete or comment this check once your Azure services are ready.
                if (!ConfigSecrets.ISAZURECONFIGDONE)
                {
                    UpdateStatus("AzureChatr cloud services not found.", true);
                    msg1 = "You must prepare your Azure services before running AzureChatr. Please refer to the README file in the solution.";
                    dlg = new Windows.UI.Popups.MessageDialog(msg1, "Missing Cloud Services");
                    await dlg.ShowAsync();
                } 
                
                if (CheckForInternetAccess())
                {
                    // Before we do anything else, we must authenticate the user
                    await Authenticate();

                    // Once successfully authenticated, let's retrieve the latest chat items from Azure
                    RefreshChatItems();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                msg1 = "An error has occurred while loading the application." + Environment.NewLine + Environment.NewLine;
                // TO DO: Dissect the various potential errors and provide a more appropriate
                //        error message in msg2 for each of them.
                msg2 = "Make sure that you have an active Internet connection and try again.";
                
                await new MessageDialog(msg1 + msg2, "Initialization Error").ShowAsync();
                // Since there was an initialization error, we must terminate the app
                Application.Current.Exit();
            }
        }

        // Authenticate the user via Microsoft Account (the default on Windows Phone)
        // Authentication via Facebook, Twitter or Google ID will be added in a future release.
        private async Task Authenticate()
        {
            prgBusy.IsActive = true;
            Exception exception = null;

            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    TextUserName.Text = "Please wait while we log you in...";
                });

                LiveAuthClient liveIdClient = new LiveAuthClient(ConfigSecrets.AzureMobileServicesURI);

                while (session == null)
                {
                    // Force a logout to make it easier to test with multiple Microsoft Accounts
                    // This code should be commented for the release build
                    //if (liveIdClient.CanLogout)
                    //    liveIdClient.Logout();

                    // Microsoft Account Login
                    LiveLoginResult result = await liveIdClient.LoginAsync(new[] { "wl.basic" });
                    if (result.Status == LiveConnectSessionStatus.Connected)
                    {
                        session = result.Session;
                        LiveConnectClient client = new LiveConnectClient(result.Session);
                        LiveOperationResult meResult = await client.GetAsync("me");
                        user = await App.MobileService
                            .LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

                        userfirstname = meResult.Result["first_name"].ToString();
                        userlastname = meResult.Result["last_name"].ToString();

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            var message = string.Format("Logged in as {0} {1}", userfirstname, userlastname);
                            TextUserName.Text = message;
                        });

                        // Debugging dialog, make sure it's commented for publishing
                        //var dialog = new MessageDialog(message, "Welcome!");
                        //dialog.Commands.Add(new UICommand("OK"));
                        //await dialog.ShowAsync();
                        isLoggedin = true;
                        SetUIState(true);
                    }
                    else
                    {
                        session = null;
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            UpdateStatus("You must log in before you can chat in this app.", true);
                            var dialog = new MessageDialog("You must log in.", "Login Required");
                            dialog.Commands.Add(new UICommand("OK"));
                            dialog.ShowAsync();
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                UpdateStatus("Something went wrong when trying to log you in.", true);
                string msg1 = "An error has occurred while trying to sign you in." + Environment.NewLine + Environment.NewLine;

                // TO DO: Dissect the various potential errors and provide a more appropriate
                //        error message in msg2 for each of them.
                string msg2 = "Make sure that you have an active Internet connection and try again.";

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new MessageDialog(msg1 + msg2, "Authentication Error").ShowAsync();
                });
            }
            prgBusy.IsActive = false;
        }

        // Inserts a new chat item to the conversation by posting it in the Azure Mobile 
        // Services table, and posting it in the application's chat window
        private async void InsertChatItem(ChatItem chatItem)
        {
            // This code inserts a new ChatItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            await chatTable.InsertAsync(chatItem);

            // When the following line is commented, the new chat item from the current user is
            // NOT posted to the current chat window. It instead waits until the chat item is
            // received in a push notification and intercepted by the app, and then displayed.
            //items.Add(chatItem);
        }

        // Fetches the last chat conversation items from the cloud to be displayed on screen
        private async void RefreshChatItems()
        {
            prgBusy.IsActive = true;

            if (isLoggedin)
            {
                MobileServiceInvalidOperationException exception = null;
                try
                {
                    // The max number of items to retrieve from Azure Mobile Services
                    // Note that N CANNOT be greater than 50, we'd have to use paging for more
                    int n = 20;

                    // This code refreshes the entries in the list view by querying the ChatItems table.
                    // We only want the last N members, so we have to sort by descending order and request the first N
                    items = await chatTable.OrderByDescending(chatitem => chatitem.TimeStamp).Take(n).ToCollectionAsync();
                    // But now we need to reverse the order again so the last item is always at the bottom of the list, not the top
                    // Unfortunately, both of these methods are unsupported on a Mobile Service Collection
                    // items.Reverse<ChatItem>();
                    // items.OrderBy(chatitem => chatitem.TimeStamp);

                    // Let's get creative and manually invert the order of the items by moving them one by one
                    // Since there cannot be more than 50 items, this is not an unreasonable technique to use
                    if (items.Count > 0)
                    {
                        if (items.Count < n) { n = items.Count; }

                        for (int i = 0; i < (n - 1); i++)
                        {
                            items.Move(0, n - i - 1);
                        }
                    }

                    items.CollectionChanged += (s, args) => ScrollDown();

                    ScrollDown();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        new MessageDialog(exception.Message, "Error loading items").ShowAsync();
                    });
                }
                else
                {
                    ListItems.ItemsSource = items;
                } 
            }
            prgBusy.IsActive = false;
        }

        // Forces the chat window to scroll to the bottom. This uses a special
        // extension method on the ListView control
        private void ScrollDown()
        {
            ListItems.ScrollToBottom();
        }

        // Event handler for the refresh app bar button.
        // Useful for scenarios where some notifications might have been lost
        // and the user wants to refresh the screen
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshChatItems();
        }

        // Event handler for the Send App Bar Button
        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            SendChatLine();
        }

        // Event handler for the Send App Bar Button
        private void ButtonSend_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendChatLine();
            }
        }

        // Prepares the chat item to be sent to the cloud
        private void SendChatLine()
        {
            string msg = TextInput.Text.Trim();
            if (isLoggedin && msg.Length > 0)
            {
                var chatItem = new ChatItem { Text = msg, UserName = String.Format("{0} {1}", userfirstname, userlastname), TimeStamp = DateTime.UtcNow };
                lastChatline = chatItem.Text;
                InsertChatItem(chatItem);
                TextInput.Text = "";
            }
        }

        private async void InitNotificationsAsync()
        {
            Exception exception = null; 

            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                var hub = new NotificationHub(ConfigSecrets.AzureNotificationHubName, ConfigSecrets.AzureNotificationHubCnxString);
                var result = await hub.RegisterNativeAsync(channel.Uri);

                // Displays the registration ID so you know it was successful
                if (result.RegistrationId != null)
                {
                    //var dialog = new MessageDialog("Registration successful: " + result.RegistrationId);
                    //dialog.Commands.Add(new UICommand("OK"));
                    //await dialog.ShowAsync();
                    UpdateStatus("Chat channel is ready.", false);

                    PushChannel = channel;
                    PushChannel.PushNotificationReceived += OnPushNotification;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if(exception != null)
            {
                UpdateStatus("Could not initialize cloud services to receive messages.", true);
                string msg1 = "An error has occurred while initializing cloud notifications." + Environment.NewLine + Environment.NewLine;

                // TO DO: Dissect the various potential errors and provide a more appropriate
                //        error message in msg2 for each of them.
                string msg2 = "Make sure that you have an active Internet connection and try again.";

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    new MessageDialog(msg1 + msg2, "Initialization Error").ShowAsync();
                });
            }
        }

        private async Task UpdateStatus(string status, bool isError)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StatusMsg.Text = status;

                StatusMsg.Foreground = new SolidColorBrush((isError) ? Colors.Red : Colors.Black);
            });
        }

        private async void OnPushNotification(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            String notificationContent = String.Empty;

            e.Cancel = true;

            switch (e.NotificationType)
            {
                // Badges are not yet supported and will be added in a future version
                case PushNotificationType.Badge:
                    notificationContent = e.BadgeNotification.Content.GetXml();
                    break;

                // Tiles are not yet supported and will be added in a future version
                case PushNotificationType.Tile:
                    notificationContent = e.TileNotification.Content.GetXml();
                    break;

                // The current version of AzureChatr only works via toast notifications
                case PushNotificationType.Toast:
                    notificationContent = e.ToastNotification.Content.GetXml();
                    XmlDocument toastXml = e.ToastNotification.Content;

                    // Extract the relevant chat item data from the toast notification payload
                    XmlNodeList toastTextAttributes = toastXml.GetElementsByTagName("text");
                    string username = toastTextAttributes[0].InnerText;
                    string chatline = toastTextAttributes[1].InnerText;
                    string chatdatetime = toastTextAttributes[2].InnerText;

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var chatItem = new ChatItem { Text = chatline, UserName = username };
                        // Post the new chat item received in the chat window.
                        // IMPORTANT: If you updated the code above to post new chat lines from
                        //            the current user immediately in the chat window, you will
                        //            end up with duplicates here. You need to filter-out the
                        //            current user's chat entries to avoid these duplicates.
                        items.Add(chatItem);
                        ScrollDown();
                    });

                    // This is a quick and dirty way to make sure that we don't use speech synthesis
                    // to read the current user's chat lines out loud
                    if (chatline != lastChatline){
                        ReadText(username + " says: " + chatline);
                    }

                    break;

                // Raw notifications are not used in this version
                case PushNotificationType.Raw:
                    notificationContent = e.RawNotification.Content;
                    break;
            }
            //e.Cancel = true;
        }

        // Used to read chat entries out loud to enable hands-free "reading" of what is being said
        // in the chat window.
        private async void ReadText(string mytext)
        {
            //Reminder: You need to enable the Microphone capabilitiy in Windows Phone projects

            if (isSpeechEnabled)
            {
                // The object for controlling the speech synthesis engine (voice).
                using (var speech = new SpeechSynthesizer())
                {
                    try
                    {
                        //Retrieve the first female voice
                        speech.Voice = SpeechSynthesizer.AllVoices
                            .First(i => (i.Gender == VoiceGender.Female && i.Description.Contains("United States")));
                        // Generate the audio stream from plain text.
                        SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync(mytext);

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                // Send the stream to the media object.
                                mediaplayer.SetSource(stream, stream.ContentType);
                                mediaplayer.Play();
                            });
                    }
                    catch (Exception exc)
                    {
                        //TO DO: Log the exception
                        var dlg = new Windows.UI.Popups.MessageDialog(exc.Message + Environment.NewLine + exc.InnerException, "Oops! Something went wrong with the speech");
                        dlg.ShowAsync();
                        //throw;
                    }
                } 
            }
        }

        // Placeholder for the future About screen which will come here
        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {

        }

        // Temporary feedback button. The feedback mechanism will be integrated within the 
        // new About box
        private async void ButtonFeedback_Click(object sender, RoutedEventArgs e)
        {
            // prepare a simple email pre-addressed to your support email
            var mailto = new Uri("mailto:?to=" + 
                                    ConfigSecrets.DeveloperSupportEmail + 
                                    "&subject=Windows Phone App Feedback: AzureChatr beta&body=Please type your feedback in the body of this email and send it:");
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }
    }
}

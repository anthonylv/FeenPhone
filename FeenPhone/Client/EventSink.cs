﻿using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeenPhone.Client
{

    public class OnUserEventArgs : EventArgs
    {
        public IUser User { get; private set; }
        public OnUserEventArgs(IUser user)
        {
            User = user;
        }
    }

    public class OnChatEventArgs : EventArgs
    {
        public IUser User { get; private set; }
        public string Text { get; private set; }
        public OnChatEventArgs(IUser user, string text)
        {
            User = user;
            Text = text;
        }
    }

    internal static class EventSink
    {
        public static event EventHandler<OnUserEventArgs> OnUserConnected;
        public static event EventHandler<OnUserEventArgs> OnUserDisconnected;
        public static event EventHandler<OnChatEventArgs> OnChat;

        public static void InvokeOnUserConnected(object sender, IUser user)
        {
            if (OnUserConnected != null)
                OnUserConnected(sender, new OnUserEventArgs(user));
        }

        public static void InvokeOnUserDisconnected(object sender, IUser user)
        {
            if (OnUserDisconnected != null)
                OnUserDisconnected(sender, new OnUserEventArgs(user));
        }

        public static void InvokeOnChat(object sender, IUser user, string text)
        {
            if (OnChat != null)
                OnChat(sender, new OnChatEventArgs(user, text));
        }

    }
}

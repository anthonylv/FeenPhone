﻿using Alienseed.BaseNetworkServer;
using Alienseed.BaseNetworkServer.Accounting;
using Alienseed.BaseNetworkServer.PacketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace FeenPhone.Client
{
    abstract class RemoteClient : BaseClient
    {
        protected const int readerBufferSize = ushort.MaxValue;

        protected readonly System.Net.IPAddress IP;
        protected readonly int Port;

        protected readonly ClientPacketHandler Handler;

        protected TCPPacketReader Reader = new TCPPacketReader();
        protected NetworkStream Stream = null;
        protected TCPPacketWriter Writer = new TCPPacketWriter();

        public RemoteClient(IUserClient localUser, System.Net.IPAddress IP, int port) : base(localUser)
        {
            this.IP = IP;
            this.Port = port;
            Handler = new ClientPacketHandler();

            Reader.OnReadData += Reader_OnReadData;
            Reader.OnDisconnect += Reader_OnDisconnect;
            Reader.OnBufferOverflow += Reader_OnBufferOverflow;
        }

        void Reader_OnBufferOverflow(object sender, BufferOverflowArgs e)
        {
            Console.WriteLine("Client Buffer Overflow: Truncating Buffer");
            e.handled = true;
        }

        void Reader_OnDisconnect()
        {
            Disconnect();
        }

        void Reader_OnReadData(object sender, DataReadEventArgs e)
        {
            Handler.Handle(e.data);
        }

        public abstract void Connect();

        protected virtual void ConnectionFailed(string message)
        {
            Console.WriteLine("Connection failed: {0}", message);
        }

        protected virtual void Disconnect()
        {
            EventSource.InvokeOnUserList(null, null);
            Writer.SetStream(null);
            if (Stream != null)
                Stream.Dispose();

        }

        public override abstract bool IsConnected{get;}

        public override abstract void Dispose();

        internal override void SendChat(string text)
        {
            Packet.WriteChat(Writer, LocalUser, text);
            EventSource.InvokeOnChat(this, LocalUser, text);
        }

        internal override void SendAudio(Audio.Codecs.CodecID codec, byte[] data, int dataLen)
        {
            Packet.WriteAudioData(Writer, codec, data, dataLen);
        }

        internal override void SendLoginInfo()
        {
            Console.WriteLine("Logging in as {0}", LocalUser.Nickname);
            Packet.WriteLoginRequest(Writer, LocalUser.Nickname, LocalUser.Nickname);
        }
    }
}

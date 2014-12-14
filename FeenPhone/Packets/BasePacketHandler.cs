﻿using Alienseed.BaseNetworkServer.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FeenPhone
{

    abstract class BasePacketHandler
    {
        protected delegate void Handler(IEnumerable<byte> payload);
        Dictionary<PacketID, Handler> Handlers;

        public BasePacketHandler()
        {
            Handlers = new Dictionary<PacketID, Handler>()
                {
                    {PacketID.LoginStatus, Handle_LoginStatus},
                    {PacketID.LoginRequest, Handle_LoginInfo},
                    {PacketID.Chat, Handle_OnChat}
                };
        }

        public bool ValidPacketID(byte id) { return ValidPacketID((PacketID)id); }
        public bool ValidPacketID(PacketID id) { return Handlers.ContainsKey(id); }

        public void Handle(Queue<byte> data)
        {
            Handler handler;
            ushort len;
            int consumed = Parse(data.ToArray(), out handler, out len);

            if (handler != null)
                handler(data.Skip(3).Take(len));

            for (int i = 0; i < consumed && data.Any(); i++)
            {
                data.Dequeue();
            }
        }

        private int Parse(byte[] data, out Handler handler, out ushort len)
        {
            handler = null;
            len = 0;
            int i = 0;
            while (i < data.Length - 2 && !ValidPacketID(data[i]))
            {
                i++;
            }

            if (!ValidPacketID(data[i]) || data.Length - i < 3)
            {
                return i;
            }

            PacketID packetid = (PacketID)data[i];
            len = (ushort)(data[i + 1] << 1 | data[i + 2]);
            if (data.Length < len + 3)
                return i;
            IEnumerable<byte> payload = data.Skip(3).Take(len);

            handler = Handlers[packetid];
            return len + 3;
        }

        protected abstract void OnLoginStatus(bool isLoggedIn);
        protected void Handle_LoginStatus(IEnumerable<byte> payload)
        {
            if (payload.Count() != 1)
                throw new ArgumentException("Invalid LoginStatus packet length");

            OnLoginStatus(payload.Single() == 0 ? false : true);
        }

        protected abstract void LoginInfo(string username, string password);
        protected void Handle_LoginInfo(IEnumerable<byte> payload)
        {
            string[] values = Encoding.ASCII.GetString(payload.ToArray()).Split('\t');
            if (values.Length == 2)
            {
                var username = values[0];
                var password = values[1];
                LoginInfo(username, password);
            }
        }

        protected abstract void OnChat(IUser user, string text);
        protected void Handle_OnChat(IEnumerable<byte> payload)
        {
            IUser user;
            int consumed = ReadUser(payload, out user);
            string text = Encoding.ASCII.GetString(payload.Skip(consumed).ToArray());
            OnChat(user, text);
        }

        protected abstract IUser GetUserObject(Guid id, bool isadmin, string username, string nickname);
        private int ReadUser(IEnumerable<byte> payload, out IUser user)
        {
            var count = payload.Count();

            if (count < 2)
                throw new ArgumentException("Out of data in ReadUser");

            byte[] lenBytes = payload.Take(2).ToArray();
            ushort len = (ushort)(lenBytes[0] << 1 | lenBytes[1]);

            if (len <=0 || count < 2 + len)
                throw new ArgumentException("Out of data in ReadUser");

            byte[] userData = payload.Skip(2).Take(len).ToArray();

            string[] uservalues = Encoding.ASCII.GetString(userData).Split('\t');

            if (uservalues.Length != 4)
                throw new ArgumentException("Invalid user data item count in ReadUser");

            Guid id;
            if(!Guid.TryParse(uservalues[0],out id))
                throw new ArgumentException("Invalid user id item count in ReadUser");

            bool isadmin = uservalues[1] == "1";
            string username = uservalues[2];
            string nickname = uservalues[3];

            user = GetUserObject(id, isadmin, username, nickname);

            return len + 2;
        }


    }
}
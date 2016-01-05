﻿using GDPLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDPLibrary.Entities
{
    public class Entry
    {
        public string Title { get; set; }
        public string Username { get; set; }
        public string Pass { get; set; }
        public string Url { get; set; }


        public static Entry Parse(byte[] source, User user)
        {
            var decrypted = Security.DecryptAES(source, user.Token);
            return Serializer.FromByteArray(source, typeof(Entry)) as Entry;
        }

        public static byte[] Crypt(Entry entry, User user)
        {
            var source = Serializer.ToByteArray(entry);
            return Security.EncryptAES(source, user.Token);
        }

    }
}

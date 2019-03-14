﻿using System;
using System.Text;

namespace NTMiner.Profile {
    public class SetWorkProfileRequest<T> : RequestBase, ISignatureRequest where T : IGetSignData{
        public SetWorkProfileRequest() { }

        public string LoginName { get; set; }

        public Guid WorkId { get; set; }

        public T Data { get; set; }

        public string Sign { get; set; }

        public void SignIt(string password) {
            this.Sign = this.GetSign(password);
        }

        public string GetSign(string password) {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(MessageId)).Append(MessageId)
                .Append(nameof(LoginName)).Append(LoginName)
                .Append(nameof(WorkId)).Append(WorkId)
                .Append(nameof(Data)).Append(Data.GetSignData())
                .Append(nameof(Timestamp)).Append(Timestamp.ToUlong())
                .Append(nameof(UserData.Password)).Append(password);
            return HashUtil.Sha1(sb.ToString());
        }
    }
}

﻿using NTMiner.Controllers;
using NTMiner.Core;
using NTMiner.MinerServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner {
    public static partial class OfficialServer {
        public class ServerMessageServiceFace {
            public static readonly ServerMessageServiceFace Instance = new ServerMessageServiceFace();
            private static readonly string SControllerName = ControllerUtil.GetControllerName<IServerMessageController>();

            private ServerMessageServiceFace() {
                VirtualRoot.BuildCmdPath<LoadNewServerMessageCommand>(action: message => {
                    GetServerMessagesAsync(LocalServerMessageSetTimestamp, (response, e) => {
                        if (response.IsSuccess() && response.Data.Count > 0) {
                            DateTime dateTime = LocalServerMessageSetTimestamp;
                            LinkedList<IServerMessage> data = new LinkedList<IServerMessage>();
                            foreach (var item in response.Data.OrderBy(a => a.Timestamp)) {
                                if (item.Timestamp > dateTime) {
                                    LocalServerMessageSetTimestamp = item.Timestamp;
                                }
                                data.AddLast(item);
                                VirtualRoot.LocalServerMessageSet.AddOrUpdate(item);
                            }
                            VirtualRoot.RaiseEvent(new NewServerMessageLoadedEvent(data));
                        }
                    });
                });
            }

            #region LocalServerMessageSetTimestamp
            private DateTime LocalServerMessageSetTimestamp {
                get {
                    if (VirtualRoot.LocalAppSettingSet.TryGetAppSetting(nameof(LocalServerMessageSetTimestamp), out IAppSetting appSetting) && appSetting.Value is DateTime value) {
                        return value;
                    }
                    return Timestamp.UnixBaseTime;
                }
                set {
                    AppSettingData appSetting = new AppSettingData {
                        Key = nameof(LocalServerMessageSetTimestamp),
                        Value = value
                    };
                    VirtualRoot.Execute(new SetLocalAppSettingCommand(appSetting));
                }
            }
            #endregion

            #region GetServerMessagesAsync
            public void GetServerMessagesAsync(DateTime timestamp, Action<DataResponse<List<ServerMessageData>>, Exception> callback) {
                ServerMessagesRequest request = new ServerMessagesRequest {
                    Timestamp = Timestamp.GetTimestamp(timestamp)
                };
                PostAsync(SControllerName, nameof(IServerMessageController.ServerMessages), null, request, callback);
            }
            #endregion

            #region AddOrUpdateServerMessageAsync
            public void AddOrUpdateServerMessageAsync(ServerMessageData entity, Action<ResponseBase, Exception> callback) {
                DataRequest<ServerMessageData> request = new DataRequest<ServerMessageData>() {
                    Data = entity
                };
                PostAsync(SControllerName, nameof(IServerMessageController.AddOrUpdateServerMessage), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
            #endregion

            #region RemoveServerMessageAsync
            public void RemoveServerMessageAsync(Guid id, Action<ResponseBase, Exception> callback) {
                DataRequest<Guid> request = new DataRequest<Guid>() {
                    Data = id
                };
                PostAsync(SControllerName, nameof(IServerMessageController.RemoveServerMessage), request.ToQuery(SingleUser.LoginName, SingleUser.PasswordSha1), request, callback);
            }
            #endregion
        }
    }
}

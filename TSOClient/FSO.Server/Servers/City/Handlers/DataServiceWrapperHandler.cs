﻿using FSO.Common.DataService;
using FSO.Common.Serialization.Primitives;
using FSO.Server.DataService.Model;
using FSO.Server.Framework.Aries;
using FSO.Server.Framework.Gluon;
using FSO.Server.Framework.Voltron;
using FSO.Server.Protocol.Voltron;
using FSO.Server.Protocol.Voltron.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Servers.City.Handlers
{
    public class DataServiceWrapperHandler
    {
        private IDataService DataService;

        public DataServiceWrapperHandler(IDataService dataService)
        {
            this.DataService = dataService;
        }

        public async void Handle(IGluonSession session, DataServiceWrapperPDU packet)
        {
            if (packet.Body is cTSOTopicUpdateMessage)
            {
                //Client wants to update a value in the data service
                var update = packet.Body as cTSOTopicUpdateMessage;
                DataService.ApplyUpdate(update, session);

                List<uint> resultDotPath = new List<uint>();
                foreach (var item in update.DotPath)
                {
                    resultDotPath.Add(item);
                    if (item == packet.RequestTypeID)
                    {
                        break;
                    }
                }

                var result = await DataService.SerializePath(resultDotPath.ToArray());
                if (result != null)
                {
                    session.Write(new DataServiceWrapperPDU()
                    {
                        SendingAvatarID = packet.SendingAvatarID,
                        RequestTypeID = packet.RequestTypeID,
                        Body = result
                    });
                }
            }
        }

        /// <summary>
        /// The user is asking for some in RAM data
        /// </summary>
        /// <param name="session"></param>
        /// <param name="packet"></param>
        public async void Handle(IVoltronSession session, DataServiceWrapperPDU packet)
        {
            if(packet.Body is cTSONetMessageStandard){
                
                var msg = (cTSONetMessageStandard)packet.Body;
                if(msg.ComplexParameter is cTSOTopicUpdateMessage)
                {
                    var update = msg.ComplexParameter as cTSOTopicUpdateMessage;
                    DataService.ApplyUpdate(update, session);
                    return;
                }
                
                //2317821664
                var type = MaskedStructUtils.FromID(packet.RequestTypeID);

                if (!msg.Parameter.HasValue)
                {
                    return;
                }

                Console.WriteLine(type.ToString());
                //if (type == MaskedStruct.MapView_NearZoom_Lot_Thumbnail || type == MaskedStruct.Thumbnail_Lot || type == MaskedStruct.MapView_NearZoom_Lot) { }

                if (type != MaskedStruct.MyAvatar && type != MaskedStruct.SimPage_Main && type != MaskedStruct.MapView_RollOverInfo_Lot_Price
                    && type != MaskedStruct.MapView_RollOverInfo_Lot && type != MaskedStruct.Unknown &&
                    type != MaskedStruct.SimPage_DescriptionPanel && type != MaskedStruct.PropertyPage_LotInfo &&
                    type != MaskedStruct.Messaging_Message_Avatar && type != MaskedStruct.Messaging_Icon_Avatar
                    && type != MaskedStruct.MapView_NearZoom_Lot_Thumbnail && type != MaskedStruct.Thumbnail_Lot
                    && type != MaskedStruct.CurrentCity && type != MaskedStruct.MapView_NearZoom_Lot
                    && type != MaskedStruct.Thumbnail_Avatar)
                {
                    //Currently broken for some reason
                    return;
                }

                //Lookup the entity, then process the request and send the response
                var task = DataService.Get(type, msg.Parameter.Value);
                if(task != null)
                {
                    var entity = await task;

                    var serialized = DataService.SerializeUpdate(type, entity, msg.Parameter.Value);
                    for (int i = 0; i < serialized.Count; i++)
                    {
                        session.Write(new DataServiceWrapperPDU()
                        {
                            SendingAvatarID = packet.SendingAvatarID,
                            RequestTypeID = packet.RequestTypeID,
                            Body = serialized[i]
                        });
                    }
                }
            }else if(packet.Body is cTSOTopicUpdateMessage)
            {
                //Client wants to update a value in the data service
                var update = packet.Body as cTSOTopicUpdateMessage;
                DataService.ApplyUpdate(update, session);

                List<uint> resultDotPath = new List<uint>();
                foreach(var item in update.DotPath){
                    resultDotPath.Add(item);
                    if(item == packet.RequestTypeID){
                        break;
                    }
                }

                var result = await DataService.SerializePath(resultDotPath.ToArray());
                if (result != null){
                    session.Write(new DataServiceWrapperPDU()
                    {
                        SendingAvatarID = packet.SendingAvatarID,
                        RequestTypeID = packet.RequestTypeID,
                        Body = result
                    });
                }

                /*var task = DataService.Get(update.DotPath[0], update.DotPath[1]);
                if(task != null)
                {
                    var entity = await task;

                    var serialized = DataService.SerializeUpdate(type, entity, msg.Parameter.Value);
                }*/
                /**/
            }
        }
    }
}

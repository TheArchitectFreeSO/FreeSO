﻿using FSO.Common.DataService;
using FSO.Common.Serialization.Primitives;
using FSO.Common.Utils;
using FSO.Server.Database.DA;
using FSO.Server.Database.DA.Lots;
using FSO.Server.DataService;
using FSO.Server.Framework.Aries;
using FSO.Server.Framework.Gluon;
using FSO.Server.Framework.Voltron;
using FSO.Server.Protocol.Electron.Packets;
using FSO.Server.Protocol.Gluon.Packets;
using FSO.Server.Protocol.Voltron.Packets;
using FSO.Server.Servers.Lot.Lifecycle;
using Ninject;
using Ninject.Extensions.ChildKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSO.Server.Servers.Lot.Domain
{
    public class LotHost
    {
        private Dictionary<int, LotHostEntry> Lots = new Dictionary<int, LotHostEntry>();
        private LotServerConfiguration Config;
        private IDAFactory DAFactory;
        private IKernel Kernel;
        private IDataServiceSync<FSO.Common.DataService.Model.Lot> LotStatusSync;
        private CityConnections CityConnections;

        public LotHost(LotServerConfiguration config, IDAFactory da, IKernel kernel, IDataServiceSyncFactory ds, CityConnections connections)
        {
            this.Config = config;
            this.DAFactory = da;
            this.Kernel = kernel;
            this.CityConnections = connections;

            LotStatusSync = ds.Get<FSO.Common.DataService.Model.Lot>("Lot_NumOccupants", "Lot_IsOnline");
        }

        public void Sync(LotContext context, FSO.Common.DataService.Model.Lot lot)
        {
            var city = CityConnections.GetByShardId(context.ShardId);
            if(city != null)
            {
                LotStatusSync.Sync(city, lot);
            }
        }

        public void RouteMessage(IVoltronSession session, object message)
        {
            var lot = GetLot(session);
            if (lot != null)
            {
                lot.Message(session, message);
            }
        }

        public void SessionClosed(IVoltronSession session)
        {
            var lot = GetLot(session);
            if(lot != null)
            {
                lot.Leave(session);
            }
        }

        public bool TryJoin(int lotId, IVoltronSession session)
        {
            var lot = GetLot(lotId);
            if (lot == null)
            {
                return false;
            }

            return lot.TryJoin(session);
        }

        public void RemoveLot(int id)
        {
            lock (Lots)
            {
                Lots.Remove(id);
            }
        }

        private LotHostEntry GetLot(IVoltronSession session)
        {
            var lotId = (int?)session.GetAttribute("currentLot");
            if(lotId == null)
            {
                return null;
            }
            return GetLot(lotId.Value);
        }

        private LotHostEntry GetLot(int id)
        {
            lock (Lots)
            {
                if (Lots.ContainsKey(id))
                {
                    return Lots[id];
                }
            }
            return null;
        }

        public LotHostEntry TryHost(int id, IGluonSession cityConnection)
        {
            lock (Lots)
            {
                if(Lots.Values.Count >= Config.Max_Lots)
                {
                    //No room
                    return null;
                }

                if (Lots.ContainsKey(id))
                {
                    return null;
                }

                var ctnr = Kernel.Get<LotHostEntry>();
                ctnr.CityConnection = cityConnection;
                Lots.Add(id, ctnr);
                return ctnr;
            }
        }

        public bool TryAcceptClaim(int lotId, uint claimId, string previousOwner)
        {
            using (var da = DAFactory.Get())
            {
                var didClaim = da.LotClaims.Claim(claimId, previousOwner, Config.Call_Sign);
                if (!didClaim)
                {
                    lock (Lots) Lots.Remove(lotId);
                    return false;
                }
                else
                {
                    var claim = da.LotClaims.Get(claimId);
                    if(claim == null)
                    {
                        lock (Lots) Lots.Remove(lotId);
                        return false;
                    }

                    var lot = da.Lots.Get(claim.lot_id);
                    if(lot == null)
                    {
                        lock (Lots) Lots.Remove(lotId);
                        return false;
                    }

                    GetLot(claim.lot_id).Bootstrap(new LotContext {
                        DbId = lot.lot_id,
                        Id = lot.location,
                        ClaimId = claimId,
                        ShardId = lot.shard_id 
                    });
                    return true;
                }
            }
        }

    }


    public class LotHostEntry : ILotHost
    {
        //Partial model for syncing updates
        private FSO.Common.DataService.Model.Lot Model;
        private LotHost Host;

        public LotContainer Container { get; internal set; }
        private Dictionary<uint, IVoltronSession> _Visitors = new Dictionary<uint, IVoltronSession>();
        public IGluonSession CityConnection;
        private IKernel ParentKernel;
        private IKernel Kernel;

        private LotServerConfiguration Config;
        private IDAFactory DAFactory;

        private Thread MainThread;
        private LotContext Context;

        private ManualResetEvent BackgroundNotify = new ManualResetEvent(false);
        private Thread BackgroundThread;
        private List<Callback> BackgroundTasks = new List<Callback>();


        public LotHostEntry(LotHost host, IKernel kernel, IDAFactory da, LotServerConfiguration config)
        {
            Host = host;
            DAFactory = da;
            Config = config;
            ParentKernel = kernel;

            Model = new FSO.Common.DataService.Model.Lot();
        }

        public void Send(uint avatarID, params object[] messages)
        {
            lock (_Visitors)
            {
                IVoltronSession visitor = null;
                if (_Visitors.TryGetValue(avatarID, out visitor))
                {
                    visitor.Write(messages);
                }
            }
        }

        public void Broadcast(HashSet<uint> ignoreIDs, params object[] messages)
        {
            //TODO: Make this more efficient
            lock (_Visitors)
            {
                foreach (var visitor in _Visitors.Values)
                {
                    if (ignoreIDs.Contains(visitor.AvatarId)) continue;
                    try
                    {
                        visitor.Write(messages);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        public void DropClient(uint id)
        {
            lock (_Visitors)
            {
                IVoltronSession visitor = null;
                if (_Visitors.TryGetValue(id, out visitor))
                {
                    visitor.Close();
                }
            }
        }

        public void InBackground(Callback cb)
        {
            lock (BackgroundTasks)
            {
                BackgroundTasks.Add(cb);
                BackgroundNotify.Set();
            }
        }

        public void Bootstrap(LotContext context)
        {
            this.Context = context;
            Model.Id = context.Id;
            Model.DbId = context.DbId;
            
            //Each lot gets its own set of bindings
            Kernel = new ChildKernel(
                ParentKernel
            );

            Kernel.Bind<LotContext>().ToConstant(context);
            Kernel.Bind<ILotHost>().ToConstant(this);

            Container = Kernel.Get<LotContainer>();

            BackgroundThread = new Thread(_DigestBackground);
            BackgroundThread.Start();

            MainThread = new Thread(Container.Run);
            MainThread.Start();
        }
        

        private void _DigestBackground()
        {
            while (BackgroundNotify.WaitOne())
            {
                List<Callback> tasks = new List<Callback>();
                lock (BackgroundTasks)
                {
                    tasks.AddRange(BackgroundTasks);
                    BackgroundTasks.Clear();
                }

                foreach (var task in tasks)
                {
                    try
                    {
                        task.Invoke();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        public void Message(IVoltronSession session, object message)
        {
            if (message is FSOVMCommand)
            {
                Container.Message(session, (FSOVMCommand)message);
            }
        }

        public void Leave(IVoltronSession session)
        {
            lock (_Visitors)
            {
                InBackground(() => Container.AvatarLeave(session));
            }
        }

        public bool TryJoin(IVoltronSession session)
        {
            lock (_Visitors)
            {
                if(_Visitors.Count >= 24)
                {
                    //Full
                    return false;
                }

                session.SetAttribute("currentLot", Context.DbId);
                _Visitors.Add(session.AvatarId, session);

                SyncNumVisitors();

                InBackground(() => Container.AvatarJoin(session));
                return true;
            }
        }

        private void SyncNumVisitors()
        {
            Model.Lot_NumOccupants = (byte)_Visitors.Count;
            Host.Sync(Context, Model);
        }

        public void ReleaseAvatarClaim(IVoltronSession session)
        {
            _Visitors.Remove(session.AvatarId);
            session.SetAttribute("currentLot", null);
            SyncNumVisitors();

            using (var db = DAFactory.Get())
            {
                //return claim to the city we got it from.
                db.AvatarClaims.Claim(session.AvatarClaimId, Config.Call_Sign, (string)session.GetAttribute("cityCallSign"), 0);
            }
        }

        public void Shutdown()
        {
            Host.RemoveLot(Context.DbId);
            SetOnline(false);
            ReleaseLotClaim();
        }

        public void ReleaseLotClaim()
        {
            //tell our city that we're no longer hosting this lot.
            if (CityConnection != null)
            {
                CityConnection.Write(new TransferClaim()
                {
                    Type = Protocol.Gluon.Model.ClaimType.LOT,
                    ClaimId = Context.ClaimId,
                    EntityId = Context.DbId,
                    FromOwner = Config.Call_Sign
                });
            }
        }

        public void SetOnline(bool online)
        {
            Model.Lot_IsOnline = online;
            Host.Sync(Context, Model);
        }
    }

    public interface ILotHost
    {
        void Send(uint avatarID, params object[] messages);
        void Broadcast(HashSet<uint> ignoreIDs, params object[] messages);
        void DropClient(uint avatarID);
        void InBackground(Callback cb);
        void ReleaseAvatarClaim(IVoltronSession session);
        void Shutdown();
        void SetOnline(bool online);
    }
}

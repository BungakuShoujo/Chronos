﻿using Chronos.Server.Databases.Characters;
using Chronos.Server.Game.Inventories;
using Chronos.Server.Game.Stats;
using Chronos.Server.Network;
using Chronos.Protocol.Enums;
using Chronos.Protocol.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronos.Server.Game.Actors.Characters
{
    public class Character : IStatsOwner
    {
        public CharacterRecord Record { get; }

        public Character(SimpleClient client, CharacterRecord record)
        {
            Record = record;
            Client = client;
        }
        public SimpleClient Client { get; }
        public int Id
        {
            get { return Record.Id; }
            set { Record.Id = value; }
        }
        public int AccountId
        {
            get { return Record.AccountId; }
            set { Record.AccountId = value; }
        }
        public string Name
        {
            get { return Record.Name; }
            set { Record.Name = value; }
        }
        public int SceneId
        {
            get { return Record.SceneId; }
            set { Record.SceneId = value; }
        }
        public bool Sex
        {
            get { return Record.Sex; }
            set { Record.Sex = value; }
        }
        public Single X
        {
            get { return Record.X; }
            set { Record.X = value; }
        }
        public Single Y
        {
            get { return Record.Y; }
            set { Record.Y = value; }
        }
        public Single Z
        {
            get { return Record.Z; }
            set { Record.Z = value; }
        }
        public int Level
        {
            get { return Record.Level; }
            set { Record.Level = value; }
        }
        public int Job
        {
            get { return Record.Job; }
            set { Record.Job = value; }
        }

        public int HairMesh
        {
            get { return Record.HairMesh; }
            set { Record.HairMesh = value; }
        }
        public int HairColor
        {
            get { return Record.HairColor; }
            set { Record.HairColor = value; }
        }
        public int HeadMesh
        {
            get { return Record.HeadMesh; }
            set { Record.HeadMesh = value; }
        }

        public bool IsBlocked => BlockTime != null && BlockTime > DateTime.Now;
        public DateTime BlockTime
        {
            get { return Record.BlockTime; }
            set { Record.BlockTime = value; }
        }

        public StatsFields Stats { get; private set; }
        public Inventory Inventory { get; private set; }
        public void LoadRecord()
        {
            Stats = new StatsFields(this);
            Stats.Initialize(Record);

            Inventory = new Inventory();
            Inventory.LoadData(Id);
        }
        public CharacterType GetNetwork()// Todo : Block time and IsBlocked
        {
            return new CharacterType(this.Client.Account.Characters.IndexOf(this), Name, Id, SceneId, Sex ? (byte)1 : (byte)0,
                new PositionType(X, Y, Z), Level, Job, Stats[DefineEnum.STR].Total, Stats[DefineEnum.STA].Total, Stats[DefineEnum.DEX].Total,
                Stats[DefineEnum.INT].Total, Stats[DefineEnum.SPI].Total, HairMesh, HairColor, HeadMesh, 0, 0, 10, Inventory.Items.Values.Select(x => x.GetNetwork()).ToArray(), new ClosetItemType[0]);
        }
    }
}
﻿using Chronos.ORM;
using Chronos.ORM.SubSonic.SQLGeneration.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronos.Server.Databases.Items
{
    [TableName("characters_closetitems")]
    public class ClosetItemRecord : IAutoGeneratedRecord
    {
        [PrimaryKey("Id")]
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int ClosetItemId { get; set; }
        
    }
    public class ClosetItemRecordRelator
    {
        public const string FetchQueryByOwnerId = "SELECT * FROM characters_closetitems WHERE OwnerId={0}";
    }
}

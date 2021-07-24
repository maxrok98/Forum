﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.DAL.Models
{
    public class Coment
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public string ParentComentId { get; set; }
        public virtual Coment ParentComent { get; set; }

        public virtual ICollection<Coment> SubComents { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }

        public string PostId { get; set; }
        public virtual Post Post { get; set; }

        public DateTime Date { get; set; }
    }
}

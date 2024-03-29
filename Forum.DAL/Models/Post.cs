﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.DAL.Models
{
    public abstract class Post
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public virtual Thread Thread { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }

        public string ImageLink { get; set; }

        public string Tags {  get; set; }

        public DateTime Date { get; set; }
        public int Rating { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public virtual ICollection<Coment> Coments { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
    }
}

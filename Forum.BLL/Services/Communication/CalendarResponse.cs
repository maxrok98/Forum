﻿using Forum.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.BLL.Services.Communication
{
    public class CalendarResponse : BaseResponse<Calendar>
    {
        public CalendarResponse(Calendar calendar) : base(calendar) { }
        public CalendarResponse(string message) : base(message) { }
    }
}

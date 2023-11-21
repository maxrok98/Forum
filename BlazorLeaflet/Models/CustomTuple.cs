﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorLeaflet.Models {
    public class CustomTuple<T1, T2> {
        
        public T1 Item1 {  get; set; }
        public T2 Item2 {  get; set; }

        public CustomTuple() { }
        
        public CustomTuple(T1 item1, T2 item2) {
            Item1 = item1;
            Item2 = item2;
        }
    }
}

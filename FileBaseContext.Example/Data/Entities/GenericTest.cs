using System;
using System.Collections.Generic;
using System.Text;

namespace FileBaseContextCore.Example.Data.Entities
{
    public class GenericTest<T> : Base
    {
        public T Value { get; set; }

    }
}

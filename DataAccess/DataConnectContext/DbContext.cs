﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Models;

namespace DataAccess.DataConnectContext
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions options): base (options)
        {

        }

        public DbSet<Message> Message { get; set; }
    }
}
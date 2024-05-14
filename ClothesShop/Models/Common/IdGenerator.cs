using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ClothesShop.Models.Common
{
    public class IdGenerator
    {
        //public static int a = 2;
        private static readonly Random random = new Random();
        private static readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static string GetId<TEntity>(DbSet<TEntity> dbSet, int len = 6) where TEntity : class
        {
            string id;
            do
            {
                id = "";
                for (int i = 0; i < len; i++)
                {
                    id += chars[random.Next(chars.Length)];
                }
            } while (dbSet.Find(id) != null);
            return id;
        }

    }
}
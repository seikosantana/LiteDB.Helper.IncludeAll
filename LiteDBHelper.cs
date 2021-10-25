using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace LiteDB.Helper.IncludeAll
{
    public static class LiteDBHelper
    {
        static Dictionary<Type, (Type PropertyType, string MemberName, bool IsList)> _mapper = new Dictionary<Type, (Type PropertyType, string MemberName, bool IsList)>();
        public static void Register<T, U>(Expression<Func<T, List<U>>> exp)
        {
            var property = (exp.Body as MemberExpression)?.Member as PropertyInfo;
            if (property == null) throw new ArgumentException("Expecting Member Expression");

            BsonMapper.Global.Entity<T>().DbRef(exp);
            _mapper.Add(typeof(T), (typeof(U), property.Name, true));
        }
        public static void Register<T, U>(Expression<Func<T, U>> exp)
        {
            var property = (exp.Body as MemberExpression)?.Member as PropertyInfo;
            if (property == null) throw new ArgumentException("Expecting Member Expression");

            BsonMapper.Global.Entity<T>().DbRef(exp);
            _mapper.Add(typeof(T), (typeof(U), property.Name, false));
        }

        public static ILiteCollection<T> IncludeAll<T>(this ILiteCollection<T> col)
        {
            if (!_mapper.ContainsKey(typeof(T))) return null;

            var results = new List<string>();
            var type = typeof(T);

            var subResults = new List<string>();
            var firstEntry = true;
            while (_mapper.TryGetValue(type, out var map))
            {
                var name = $"{map.MemberName}{(map.IsList ? "[*]" : "")}";
                subResults.Add(firstEntry ? $"$.{name}" : name);
                results.Add(string.Join(".", subResults));
                type = map.PropertyType;
                firstEntry = false;
            }

            foreach (var result in results)
                col = col.Include(result);

            return col;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeWars.ContestRunner
{
    static class ExceptionExtensions
    {
        public static string GetFlatMessage(this Exception ex)
        {
            var ae = ex as AggregateException;
            if (ae == null) return ex.Message;
            if (ae.InnerExceptions == null) return ae.Message;
            return $"{ae.Message}: {string.Join("; ", ae.InnerExceptions.Select(s => s.Message))}";
        }
    }
}

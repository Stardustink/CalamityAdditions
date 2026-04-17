using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8602 
namespace CalamityAdditions.Core.Utilities
{
    public static partial class Utilities
    {
        /// <summary>
        ///     Returns the namespace path to the provided object, including the object itself.
        /// </summary>
        public static string GetPath(this object obj) => obj.GetType().Namespace.Replace('.', '/') + "/" + obj.GetType().Name;
    }

}

#pragma warning restore CS8602 
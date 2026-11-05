using System;
using System.Reflection;

namespace Greenshot.Base.Core
{
    /// <summary>Public identity; version is generated from src/version.json.</summary>
    public static class ProductIdentity
    {
        public const string Name = "BuckShot Markup";
        public const string StorageName = "BuckShot-Markup";
        public const string ConfigName = "buckshot-markup.ini";
        public const string SourceUrl = "https://github.com/uloxer/BuckShot-Markup";
        public const string MutexId = "69C8E723-534D-46B2-852B-7E8B8F051666";
        public const string ToastActivatorId = "34C2D221-917F-4666-8367-119C43A08666";
        public static string Version => new Version(typeof(ProductIdentity).Assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>().Version).ToString(3);
    }
}

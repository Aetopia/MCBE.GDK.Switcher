using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

sealed class Catalog
{
    const string Bootstrapper = "https://github.com/Aetopia/Pyroclastic/releases/latest/download/gamelaunchhelper.dll";
    const string Packages = "https://raw.githubusercontent.com/MinecraftBedrockArchiver/GdkLinks/refs/heads/master/urls.json";

    static readonly HttpClient s_client = new();
    static readonly DataContractJsonSerializer s_serializer = new(typeof(Dictionary<string, Dictionary<string, string[]>>), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });

    internal readonly Versions Release, Preview;

    internal static async Task<Catalog> GetAsync()
    {
        var bytes = s_client.GetByteArrayAsync(Bootstrapper); var versions = Task.Run(async () =>
        {
            using var stream = await s_client.GetStreamAsync(Packages);
            return (Dictionary<string, Dictionary<string, string[]>>)s_serializer.ReadObject(stream);
        });
        await Task.WhenAll(versions, bytes); return new(await versions, await bytes);
    }

    Catalog(Dictionary<string, Dictionary<string, string[]>> versions, byte[] bytes)
    {
        Release = new Versions.Release(versions["release"], bytes);
        Preview = new Versions.Preview(versions["preview"], bytes);
    }

    internal abstract class Versions : IEnumerable<string>
    {
        readonly byte[] _bytes; readonly Dictionary<string, string[]> _versions = [];
        Versions(Dictionary<string, string[]> versions, byte[] bytes) => (_versions, _bytes) = (versions, bytes);

        protected abstract string PackageFamilyName { get; }

        static readonly PackageManager s_manager = new();
        static readonly AddPackageOptions s_options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

        internal sealed class Release : Versions
        {
            protected override string PackageFamilyName => "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
            internal Release(Dictionary<string, string[]> versions, byte[] bytes) : base(versions, bytes) { }
        }

        internal sealed class Preview : Versions
        {
            protected override string PackageFamilyName => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";
            internal Preview(Dictionary<string, string[]> versions, byte[] bytes) : base(versions, bytes) { }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<string> GetEnumerator() => _versions.Keys.GetEnumerator();

        static async Task<Uri?> PingAsync(string uri)
        {
            try
            {
                using var message = await s_client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                return message.IsSuccessStatusCode ? new(uri) : null;
            }
            catch { return null; }
        }

        internal async Task InstallAsync(string version, Action<int> action)
        {
            Task<Uri?>[] tasks = [.. _versions[version].Select(PingAsync)]; await Task.WhenAny(tasks);
            if (tasks.FirstOrDefault(_ => _.Status is TaskStatus.RanToCompletion) is not { } task) return;

            if (await task is not { } uri) return;
            var operation = s_manager.AddPackageByUriAsync(uri, s_options);
            operation.Progress += (sender, args) => action((int)args.percentage);

            TaskCompletionSource<bool> source = new(); operation.Completed += (sender, args) =>
            {
                switch (sender.Status)
                {
                    case AsyncStatus.Completed:
                        if (s_manager.FindPackagesForUser(string.Empty, PackageFamilyName).FirstOrDefault() is { } package)
                        {
                            var path = Path.Combine(package.InstalledPath, "gamelaunchhelper.dll");
                            File.WriteAllBytes(path, _bytes);
                        }
                        source.TrySetResult(new()); break;

                    case AsyncStatus.Error: source.TrySetException(sender.ErrorCode); break;
                    case AsyncStatus.Canceled: source.TrySetCanceled(); break;
                }
            };

            await source.Task;
        }
    }
}
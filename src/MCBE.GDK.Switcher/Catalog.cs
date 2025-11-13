
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;

partial class Catalog
{
    internal abstract class Versions : Dictionary<string, string[]>
    {
        internal sealed class Release : Versions
        {
            protected override string PackageFamilyName => "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        }

        internal sealed class Preview : Versions
        {
            protected override string PackageFamilyName => "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";
        }

        protected abstract string PackageFamilyName { get; }

        static async Task<string?> UriAsync(string uri)
        {
            try
            {
                using var message = await s_client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                return message.IsSuccessStatusCode ? uri : null;
            }
            catch { return null; }
        }

        internal async Task<Request?> GetAsync(string key, Action<int> action)
        {
            var tasks = this[key].Select(UriAsync).ToArray();
            await Task.WhenAny(tasks);

            if (tasks.FirstOrDefault(_ => _.Status is TaskStatus.RanToCompletion) is not { } task) return null;
            if (await task is not { } uri) return null;

            return new(uri, action, PackageFamilyName);
        }
    }
}

partial class Catalog
{
    internal sealed class Request
    {
        static readonly byte[] s_bytes;

        static Request()
        {
            using MemoryStream destination = new();
            using var source = Assembly.GetExecutingAssembly().GetManifestResourceStream("GameLaunchHelper.exe");
            source.CopyTo(destination); s_bytes = destination.ToArray();
        }

        static readonly AddPackageOptions s_options = new() { ForceAppShutdown = true, ForceUpdateFromAnyVersion = true };

        static readonly PackageManager s_manager = new();

        readonly TaskCompletionSource<bool> _source = new();

        readonly IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> _operation;

        internal Request(string uri, Action<int> action, string packageFamilyName)
        {
            _operation = s_manager.AddPackageByUriAsync(new(uri), s_options);

            _operation.Progress = (sender, args) =>
            {
                Console.WriteLine($"{args.state} : {args.percentage}");
                action((int)args.percentage);
            };

            _operation.Completed = (sender, args) =>
            {
                switch (sender.Status)
                {
                    case AsyncStatus.Completed:
                        var package = s_manager.FindPackagesForUser(string.Empty, packageFamilyName).First();
                        var path = Path.Combine(package.InstalledPath, "GameLaunchHelper.exe");

                        File.WriteAllBytes(path, s_bytes);
                        _source.TrySetResult(true);
                        break;

                    case AsyncStatus.Canceled:
                        _source.TrySetResult(true);
                        break;

                    case AsyncStatus.Error:
                        _source.TrySetException(sender.ErrorCode);
                        break;
                }
            };
        }

        internal bool Completed => _source.Task.IsCompleted;

        internal TaskAwaiter GetAwaiter() => ((Task)_source.Task).GetAwaiter();

        internal void Cancel() { if (!_source.Task.IsCompleted) _operation.Cancel(); }

        ~Request() => _operation.Close();
    }
}

[DataContract]
sealed partial class Catalog
{
    [DataMember(Name = "release")]
    internal readonly Versions.Release Release = [];

    [DataMember(Name = "preview")]
    internal readonly Versions.Preview Preview = [];

    static readonly DataContractJsonSerializer s_serializer = new(typeof(Catalog), new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
}

partial class Catalog
{
    const string Uri = "https://raw.githubusercontent.com/MinecraftBedrockArchiver/GdkLinks/refs/heads/master/urls.json";

    static readonly HttpClient s_client = new();

    internal static async Task<Catalog> GetAsync() => await Task.Run(async () =>
    {
        using var stream = await s_client.GetStreamAsync(Uri);
        return (Catalog)s_serializer.ReadObject(stream);
    });
}
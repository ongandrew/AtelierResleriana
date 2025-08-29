using System.Diagnostics;

namespace AtelierResleriana.Executables.Pipeline.All.Run
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CancellationToken cancellationToken = default;

            Task extractMasterDataTask = RunBranchAsync("MasterData");
            Task extractTextAssetTask = RunBranchAsync("TextAsset");

            await Task.WhenAll([extractMasterDataTask, extractTextAssetTask]);

            await RunAsync("AtelierResleriana.Executables.Pipeline.All.Finalize", cancellationToken).ConfigureAwait(false);
        }

        private static async Task RunBranchAsync(string name, CancellationToken cancellationToken = default)
        {
            await RunAsync($"AtelierResleriana.Executables.Pipeline.{name}.Extract", cancellationToken).ConfigureAwait(false);
            await RunAsync($"AtelierResleriana.Executables.Pipeline.{name}.Prepare", cancellationToken).ConfigureAwait(false);
            await RunAsync($"AtelierResleriana.Executables.Pipeline.{name}.Localize", cancellationToken).ConfigureAwait(false);
        }

        private static async Task RunAsync(string name, CancellationToken cancellationToken = default)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            processStartInfo.ArgumentList.Add("run");
            processStartInfo.ArgumentList.Add("--project");
            processStartInfo.ArgumentList.Add($"../{name}/{name}.csproj");
            Process? process = Process.Start(processStartInfo);

            if (process == null)
            {
                return;
            }

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Process {name} failed.");
            }
        }
    }
}

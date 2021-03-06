using System.IO;
using Calamari.Commands;
using Calamari.Integration.Processes;
using Calamari.Integration.ServiceMessages;
using Octostache;

namespace Calamari.Tests.Helpers
{
    public abstract class CalamariFixture
    {
        protected CommandLine Calamari()
        {
            return CommandLine.Execute(typeof (DeployPackageCommand).Assembly.FullLocalPath());
        }

        protected CalamariResult Invoke(CommandLine command, VariableDictionary variables)
        {
            var capture = new CaptureCommandOutput();
            var runner = new CommandLineRunner(new SplitCommandOutput(new ConsoleCommandOutput(), new ServiceMessageCommandOutput(variables), capture));
            var result = runner.Execute(command.Build());
            return new CalamariResult(result.ExitCode, capture);
        }

        protected CalamariResult Invoke(CommandLine command)
        {
            return Invoke(command, new VariableDictionary());
        }

        protected string MapSamplePath(string file)
        {
            var parent = Path.GetDirectoryName(GetType().Assembly.FullLocalPath());
            var path = GetType().Namespace.Replace("Calamari.Tests", "");
            path = path.Replace(".", "\\").Trim('.', '\\');
            return Path.GetFullPath(Path.Combine(parent, path, file));
        }
    }
}
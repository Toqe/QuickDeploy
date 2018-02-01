using QuickDeploy.Common.Messages;

namespace QuickDeploy.Common
{
    public interface IQuickDeployClient
    {
        string RemoteAddress { get; }

        AnalyzeDirectoryResponse AnalyzeDirectory(AnalyzeDirectoryRequest analyzeDirectoryRequest);

        SyncDirectoryResponse SyncDirectory(SyncDirectoryRequest syncDirectoryRequest);

        ChangeServiceStatusResponse ChangeServiceStatus(ChangeServiceStatusRequest changeServiceStatusRequest);

        ExecuteCommandResponse ExecuteCommand(ExecuteCommandRequest executeCommandRequest);

        ExtractZipResponse ExtractZip(ExtractZipRequest request);
    }
}

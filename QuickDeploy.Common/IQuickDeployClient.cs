using QuickDeploy.Common.Messages;

namespace QuickDeploy.Common
{
    public interface IQuickDeployClient
    {
        string RemoteAddress { get; }

        AnalyzeDirectoryResponse AnalyzeDirectory(AnalyzeDirectoryRequest analyzeDirectoryRequest);

        SyncDirectoryResponse SyncDirectory(SyncDirectoryRequest syncDirectoryRequest);

        SyncFileResponse SyncFile(string filename, byte[] fileContent, Credentials credentials);

        ChangeServiceStatusResponse ChangeServiceStatus(ChangeServiceStatusRequest changeServiceStatusRequest);

        ChangeIisAppPoolStatusResponse ChangeIisAppPoolStatus(ChangeIisAppPoolStatusRequest changeIisAppPoolStatusRequest);

        ExecuteCommandResponse ExecuteCommand(ExecuteCommandRequest executeCommandRequest);

        ExtractZipResponse ExtractZip(ExtractZipRequest extractZipRequest);

        ProxyResponse Proxy(ProxyRequest proxyRequest);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickDeploy.Common;
using QuickDeploy.Common.Messages;

namespace QuickDeploy.Client
{
    public class QuickDeployProxyClient : IQuickDeployClient
    {
        private readonly IQuickDeployClient innerClient;

        private readonly ProxyRequest proxyRequestTemplate;

        private readonly StreamHelper streamHelper = new StreamHelper();

        public QuickDeployProxyClient(
            IQuickDeployClient innerClient,
            ProxyRequest proxyRequestTemplate)
        {
            this.innerClient = innerClient;
            this.proxyRequestTemplate = proxyRequestTemplate;
        }

        public string RemoteAddress => $"TCP {this.proxyRequestTemplate.Hostname}:{this.proxyRequestTemplate.Port} (proxy through {this.innerClient.RemoteAddress})";

        public TResponse Call<TResponse>(AuthorizedRequest request) where TResponse : BaseResponse
        {
            var proxyRequest = this.streamHelper.Clone(this.proxyRequestTemplate);
            proxyRequest.Request = request;
            var proxyResponse = this.innerClient.Proxy(proxyRequest);

            if (proxyResponse?.Success == true)
            {
                return proxyResponse.Response as TResponse;
            }

            throw new ProxyException(proxyResponse?.ErrorMessage);
        }

        public AnalyzeDirectoryResponse AnalyzeDirectory(AnalyzeDirectoryRequest analyzeDirectoryRequest)
        {
            return this.Call<AnalyzeDirectoryResponse>(analyzeDirectoryRequest);
        }

        public SyncDirectoryResponse SyncDirectory(SyncDirectoryRequest syncDirectoryRequest)
        {
            return this.Call<SyncDirectoryResponse>(syncDirectoryRequest);
        }

        public SyncFileResponse SyncFile(string filename, byte[] fileContent, Credentials credentials)
        {
            var syncFileRequest = new SyncFileRequest
            {
                Credentials = credentials,
                Filename = filename,
                GzippedFile = new Zipper().Gzip(fileContent),
            };

            return this.Call<SyncFileResponse>(syncFileRequest);
        }

        public ChangeServiceStatusResponse ChangeServiceStatus(ChangeServiceStatusRequest changeServiceStatusRequest)
        {
            return this.Call<ChangeServiceStatusResponse>(changeServiceStatusRequest);
        }

        public GetServiceStatusResponse GetServiceStatus(GetServiceStatusRequest getServiceStatusRequest)
        {
            return this.Call<GetServiceStatusResponse>(getServiceStatusRequest);
        }

        public ChangeIisAppPoolStatusResponse ChangeIisAppPoolStatus(ChangeIisAppPoolStatusRequest changeIisAppPoolStatusRequest)
        {
            return this.Call<ChangeIisAppPoolStatusResponse>(changeIisAppPoolStatusRequest);
        }

        public ExecuteCommandResponse ExecuteCommand(ExecuteCommandRequest executeCommandRequest)
        {
            return this.Call<ExecuteCommandResponse>(executeCommandRequest);
        }

        public ExtractZipResponse ExtractZip(ExtractZipRequest extractZipRequest)
        {
            return this.Call<ExtractZipResponse>(extractZipRequest);
        }

        public ProxyResponse Proxy(ProxyRequest proxyRequest)
        {
            return this.Call<ProxyResponse>(proxyRequest);
        }
    }
}

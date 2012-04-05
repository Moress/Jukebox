
namespace Jukebox.Server.Services {
	using System.IO;
	using System.ServiceModel;
	using System.ServiceModel.Web;

	[ServiceContract]
	interface IPolicyService {
		[OperationContract, WebGet(UriTemplate = "/clientaccesspolicy.xml")]
		Stream GetSilverlightPolicy();
	}
}

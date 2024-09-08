using MauiCebBlaz.Shared.Services;

namespace MauiCebBlaz.Services {
	public class FormFactor : IFormFactor {
		public string GetFormFactor() {
			return DeviceInfo.Idiom.ToString();
		}

		public string GetPlatform() {
			return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
		}
	}
}

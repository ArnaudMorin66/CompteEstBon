using System;
using System.Threading;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;


namespace ExportCeb {
    public class Program {
        private static AppServiceConnection connection;
        static AutoResetEvent appServiceExit;

        static void Main() {
            appServiceExit = new AutoResetEvent(false);
            InitializeAppServiceConnection();
            appServiceExit.WaitOne();
        }
        static async void InitializeAppServiceConnection() {
            connection = new AppServiceConnection {
                AppServiceName = "ExcelInteropService",
                PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName
            };
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;
            var status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success) {
                // TODO: error handling
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args) {
            appServiceExit.Set();
        }


        private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args) {
            string result;
            switch ((args.Request.Message["Format"] as string)?.ToLower()) {
                case "excel":
                    result = args.Request.Message.ToExcel();
                    break;
                case "word":
                    result = args.Request.Message.ToWord();
                    break;
                default:
                    result = "Format introuvable";
                    break;
            }

            ValueSet response = new ValueSet { { "RESPONSE", result } };
            await args.Request.SendResponseAsync(response);
        }
    }
}

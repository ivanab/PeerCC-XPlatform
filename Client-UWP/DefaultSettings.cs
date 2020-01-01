using Client_UWP.Models;
using Client_UWP.Pages;
using ClientCore.Call;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Client_UWP
{
    public static class DefaultSettings
    {
        private static LocalSettings _localSettings = new LocalSettings();

        private static string GetWirelessIpAddress()
        {
            string ipAddress = "";
            var ni = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface item in ni)
            {
                if (item.OperationalStatus == OperationalStatus.Up && item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork & !IPAddress.IsLoopback(ip.Address))
                        {
                            ipAddress = ip.Address.ToString();
                            
                            return ipAddress;
                        }
                    }
                }
            }
            return "";
        }

        public static void AddLocalServerAccount()
        {
            string ipAddress = GetWirelessIpAddress();

            AccountModel accountModel = new AccountModel();
            accountModel.AccountName = "Local Server";
            accountModel.ServiceUri = $"http://{ipAddress}:8888";

            accountsList.Add(accountModel);

            _localSettings.SerializeAccountsList(accountsList);
        }

        static ObservableCollection<AccountModel> accountsList = new ObservableCollection<AccountModel>();

        public static void AddDefaultAccount()
        {
            if (_localSettings.DeserializeAccountsList() == null
                || !(_localSettings.DeserializeAccountsList()).Any())
            {
                AccountModel accountModel = new AccountModel();
                accountModel.AccountName = "Default Account";
                accountModel.ServiceUri = "http://40.83.179.150:8888";

                accountsList.Add(accountModel);

                _localSettings.SerializeSelectedAccount(accountModel);
                _localSettings.SerializeAccountsList(accountsList);
            }
        }

        public static void AddDefaultIceServersList(WebRtcAdapter.Call.Call call)
        {
            if (_localSettings.DeserializeIceServersList() == null
                || !(_localSettings.DeserializeIceServersList()).Any())
            {
                List<IceServer> iceServersList = new List<IceServer>();

                ObservableCollection<IceServerModel> iceServerModelList = new ObservableCollection<IceServerModel>();

                foreach (IceServer iceServer in AddDefaultIceServers)
                {
                    IceServerModel iceServerModel = new IceServerModel();
                    iceServerModel.Urls = iceServer.Urls;
                    iceServerModel.Username = iceServer.Username;
                    iceServerModel.Credential = iceServer.Credential;

                    iceServerModelList.Add(iceServerModel);
                }

                _localSettings.SerializeIceServersList(iceServerModelList);

                foreach (var ice in iceServerModelList)
                {
                    IceServer iceServer = new IceServer();
                    iceServer.Urls = ice.Urls;
                    iceServer.Username = ice.Username;
                    iceServer.Credential = ice.Credential;

                    iceServersList.Add(iceServer);
                }

                call.AddIceServers(iceServersList);
                call.SetIceServers(iceServersList);
            }
            else
            {
                List<IceServer> iceServersList = new List<IceServer>();

                ObservableCollection<IceServerModel> iceServerModelList = _localSettings.DeserializeIceServersList();

                foreach (var ice in iceServerModelList)
                {
                    IceServer iceServer = new IceServer();
                    iceServer.Urls = ice.Urls;
                    iceServer.Username = ice.Username;
                    iceServer.Credential = ice.Credential;

                    iceServersList.Add(iceServer);
                }

                call.AddIceServers(iceServersList);
                call.SetIceServers(iceServersList);
            }
        }

        public static List<IceServer> AddDefaultIceServers
            => new List<IceServer>()
            {
                new IceServer { Urls = new List<string> { "stun:stun.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun1.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun2.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun3.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun4.l.google.com:19302" } }
            };
    }
}

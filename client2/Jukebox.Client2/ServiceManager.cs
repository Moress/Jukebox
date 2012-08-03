using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Jukebox.Client2.JukeboxService;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace Jukebox.Client2
{
    public class ServiceManager
    {
        static CustomBinding _defaultBinding = new CustomBinding(new BinaryMessageEncodingBindingElement(),
            new TcpTransportBindingElement());

        static SearchServiceClient _searchServiceClient;
        static PlaylistServiceClient _playlistServiceClient;
        static PlayerServiceClient _playerServiceClient;
        static UserServiceClient _userServiceClient;

        public static EventHandler ChannelsWereChanged;

        private static string _host;

        public static string Host
        {
            get
            {
                return _host;
            }

            set
            {
                _host = value;
            }
        }

        public static SearchServiceClient GetSearchServiceClient()
        {
            if (_searchServiceClient != null && _searchServiceClient.State == CommunicationState.Faulted)
            {
                RecreateAllChannels();
            }

            if (_searchServiceClient == null)
                _searchServiceClient = new SearchServiceClient(_defaultBinding, new EndpointAddress(Host + "Search"));

            return _searchServiceClient;
        }

        public static PlaylistServiceClient GetPlaylistServiceClient()
        {
            if (_playlistServiceClient != null && _playlistServiceClient.State == CommunicationState.Faulted)
            {
                RecreateAllChannels();
            }

            if (_playlistServiceClient == null)
                _playlistServiceClient = new PlaylistServiceClient(_defaultBinding, new EndpointAddress(Host + "Playlist"));
            return _playlistServiceClient;
        }

        public static UserServiceClient GetUserServiceClient()
        {
            if (_userServiceClient != null && _userServiceClient.State == CommunicationState.Faulted)
            {
                RecreateAllChannels();
            }

            if (_userServiceClient == null)
                _userServiceClient = new UserServiceClient(_defaultBinding, new EndpointAddress(Host + "User"));
            return _userServiceClient;
        }

        /// <summary>
        /// Убивает все коммуникационные объекты и создает новые.
        /// </summary>
        public static void RecreateAllChannels()
        {
            KillChannel(_searchServiceClient);
            KillChannel(_playlistServiceClient);
            KillChannel(_playerServiceClient);
            KillChannel(_userServiceClient);

            _searchServiceClient = null;
            _playlistServiceClient = null;
            _playerServiceClient = null;
            _userServiceClient = null;

            if (ChannelsWereChanged != null)
                ChannelsWereChanged(null, null);
        }

        static void KillChannel(ICommunicationObject channel)
        {
            if (channel == null)
                return;

            if (channel.State == CommunicationState.Faulted)
            {
                channel.Abort();
                channel = null;
                return;
            }

            // TODO: закрытие
            channel = null;
        }

        public static PlayerServiceClient GetPlayerServiceClient()
        {
            if (_playerServiceClient != null && _playerServiceClient.State == CommunicationState.Faulted)
            {
                RecreateAllChannels();
            }

            if (_playerServiceClient == null)
                _playerServiceClient = new PlayerServiceClient(_defaultBinding, new EndpointAddress(Host + "Player"));
            return _playerServiceClient;
        }
    }
}

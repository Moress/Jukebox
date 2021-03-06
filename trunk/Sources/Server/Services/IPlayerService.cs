﻿
namespace Jukebox.Server.Services {
    using System;
	using System.ServiceModel;
	using Jukebox.Server.Models;

	[ServiceContract(
		SessionMode = SessionMode.Required,
		CallbackContract = typeof(IPlayerServiceCallback))]
	interface IPlayerService {
		[OperationContract]
		Track GetCurrentTrack();

        [OperationContract]
        double GetVolumeLevel();

        [OperationContract]
        string SetVolumeLevel(double value);

        [OperationContract]
        string PlayOrPause();
	}
}

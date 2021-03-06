﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Security;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TokenSecuredKnownUser
{

	public sealed partial class MainPage : Page
	{
		private TaskCompletionSource<Credential> _loginTCS;

		public MainPage()
		{
			this.InitializeComponent();

			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);
		}

		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			Debug.WriteLine(string.Format("Error while loading layer : {0} - {1}", e.Layer.ID, e.LoadError.Message));
		}

		// Base Challenge method that dispatches to the UI thread if necessary
		private async Task<Credential> Challenge(CredentialRequestInfo cri)
		{
			return await Challenge_KnownCredentials(cri);
		}

		// Challenge method that checks for service access with known credentials
		private async Task<Credential> Challenge_KnownCredentials(CredentialRequestInfo cri)
		{
			try
			{
				// Obtain credentials from a secure source
				string username = "user1";
				string password = (cri.ServiceUri.Contains("USA_secure_user1")) ? "user1" : "pass.word1";

				return await IdentityManager.Current.GenerateCredentialAsync(cri.ServiceUri, username, password, cri.GenerateTokenOptions);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Access to " + cri.ServiceUri + " denied. " + ex.Message, "Credential Error").ShowAsync();
			}

			return await Task.FromResult<Credential>(null);
		}
	}

	// Helper class to contain login information
	public class LoginInfo : INotifyPropertyChanged
	{
		private CredentialRequestInfo _requestInfo;
		public CredentialRequestInfo RequestInfo
		{
			get { return _requestInfo; }
			set { _requestInfo = value; OnPropertyChanged(); }
		}

		private string _serviceUrl;
		public string ServiceUrl
		{
			get { return _serviceUrl; }
			set { _serviceUrl = value; OnPropertyChanged(); }
		}

		private string _userName;
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; OnPropertyChanged(); }
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set { _password = value; OnPropertyChanged(); }
		}

		private string _errorMessage;
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; OnPropertyChanged(); }
		}

		private int _attemptCount;
		public int AttemptCount
		{
			get { return _attemptCount; }
			set { _attemptCount = value; OnPropertyChanged(); }
		}

		public LoginInfo(CredentialRequestInfo cri, string user, string pwd)
		{
			RequestInfo = cri;
			ServiceUrl = new Uri(cri.ServiceUri).GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Query, UriFormat.UriEscaped);
			UserName = user;
			Password = pwd;
			ErrorMessage = string.Empty;
			AttemptCount = 0;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class ValueToForegroundColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			SolidColorBrush brush;
			if (value.ToString() == "Initializing")
				brush = new SolidColorBrush(Colors.Red);
			else if (value.ToString() == "Initialized")
				brush = new SolidColorBrush(Colors.Green);
			else
				brush = new SolidColorBrush(Colors.Black);

			return brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
